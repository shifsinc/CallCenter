using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class LoginContext : INotifyPropertyChanged
    {
        public List<UserDto> Users { get; set; }
        public UserDto CurrentUser { get; set; }

        public string UserName { get; set; }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public LoginContext(string server)
        {
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", server, "asterisk", "mysqlasterisk", "asterisk");
            AppSettings.SetDbConnection(new MySqlConnection(connectionString));
            AppSettings.ConnectionString = connectionString;
            try
            {
                AppSettings.DbConnection.Open();
                var localIp = GetLocalIpAddress();
                using (var cmd = new MySqlCommand($"CALL CallCenter.GetSIPInfoByIp('{localIp}')", AppSettings.DbConnection))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            AppSettings.SetSipInfo(new SipDto {SipUser = dataReader.GetNullableString("SIPName"), SipSecret = dataReader.GetNullableString("Secret") });
                        }
                        dataReader.Close();
                    }
                }

                using (var cmd = new MySqlCommand(@"SELECT id,Login FROM CallCenter.Users where Enabled = 1 and ShowInForm = 1 order by Login", AppSettings.DbConnection))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        Users = new List<UserDto>();
                        while (dataReader.Read())
                        {
                            Users.Add( new UserDto()
                            {
                                Id = dataReader.GetInt32("Id"),
                                Login = dataReader.GetNullableString("Login")
                            });
                        }
                        dataReader.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("��������� ������ � ����������! ���������� ����� �������!\r\n" + ex.Message, "������");
                Environment.Exit(0);
                //Application.Current.Shutdown();
            }
        }

        public LoginView View { get; set; }

        private bool _canExecute = true;
        private ICommand _loginCommand;
        public ICommand LoginCommand { get { return _loginCommand ?? (_loginCommand = new CommandHandler(Login, _canExecute)); } }

        private bool _canCancelExecute = true;
        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new CommandHandler(Cancel, _canCancelExecute)); } }

        private void Cancel()
        {
            View.Cancel();
        }

        private void Login()
        {
            var userId = 0;
            using (
                var cmd =
                    new MySqlCommand($"Call CallCenter.LoginUser('{UserName}','{Password}','{AppSettings.SipInfo?.SipUser}')", AppSettings.DbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        userId = dataReader.GetInt32("Id");
                        CurrentUser = new UserDto()
                        {
                            Id = dataReader.GetInt32("Id"),
                            Login = dataReader.GetNullableString("Login"),
                            FirstName = dataReader.GetNullableString("FirstName"),
                            SurName = dataReader.GetNullableString("SurName"),
                            PatrName = dataReader.GetNullableString("PatrName")
                        };

                    }
                    dataReader.Close();
                }
            }
            if (userId > 0)
            {
                using (var cmd =
                    new MySqlCommand(
                        $@"SELECT r.* FROM CallCenter.UserRoles u
                            join CallCenter.Roles r on r.id = u.role_id
                            where u.user_id = {
                            userId}", AppSettings.DbConnection))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        CurrentUser.Roles = new List<RoleDto>();
                        while (dataReader.Read())
                        {
                            CurrentUser.Roles.Add(new RoleDto
                            {
                                Id = dataReader.GetInt32("Id"),
                                Name = dataReader.GetNullableString("Name")
                            });
                        }
                        dataReader.Close();
                    }
                }
                AppSettings.SetUser(CurrentUser);
                View.Done();
            }
            else
            {
                MessageBox.Show("������� �������� ����� ��� ������!", "������ �����������");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}