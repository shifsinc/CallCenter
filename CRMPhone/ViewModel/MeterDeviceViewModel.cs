﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CRMPhone.Annotations;
using CRMPhone.ViewModel;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone
{
    public class MeterDeviceViewModel : INotifyPropertyChanged
    {
        private Window _view;

        private readonly RequestService _requestService;
        private ObservableCollection<CityDto> _cityList;
        private CityDto _selectedCity;
        private ObservableCollection<StreetDto> _streetList;
        private StreetDto _selectedStreet;
        private ObservableCollection<HouseDto> _houseList;
        private HouseDto _selectedHouse;
        private ObservableCollection<FlatDto> _flatList;
        private FlatDto _selectedFlat;

        private string _phoneNumber;
        public string PhoneNumber {
            get { return _phoneNumber; }
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        public string PersonalAccount
        {
            get { return _personalAccount; }
            set { _personalAccount = value; OnPropertyChanged(nameof(PersonalAccount));}
        }

        public double Electro1
        {
            get { return _electro1; }
            set { _electro1 = value; OnPropertyChanged(nameof(Electro1)); }
        }

        public double Electro2
        {
            get { return _electro2; }
            set { _electro2 = value; OnPropertyChanged(nameof(Electro2));}
        }

        public double HotWater1
        {
            get { return _hotWater1; }
            set { _hotWater1 = value; OnPropertyChanged(nameof(HotWater1));}
        }

        public double HotWater2
        {
            get { return _hotWater2; }
            set { _hotWater2 = value; OnPropertyChanged(nameof(HotWater2));}
        }

        public double ColdWater1
        {
            get { return _coldWater1; }
            set { _coldWater1 = value; OnPropertyChanged(nameof(ColdWater1));}
        }

        public double ColdWater2
        {
            get { return _coldWater2; }
            set { _coldWater2 = value; OnPropertyChanged(nameof(ColdWater2));}
        }

        public double Heating2
        {
            get { return _heating2; }
            set { _heating2 = value; OnPropertyChanged(nameof(Heating2));}
        }

        public double Heating3
        {
            get { return _heating3; }
            set { _heating3 = value; OnPropertyChanged(nameof(Heating3));}
        }

        public double Heating
        {
            get { return _heating; }
            set { _heating = value; OnPropertyChanged(nameof(Heating));}
        }

        public ObservableCollection<CityDto> CityList
        {
            get { return _cityList; }
            set { _cityList = value; OnPropertyChanged(nameof(CityList)); }
        }

        public CityDto SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                ChangeCity(value?.Id);
                OnPropertyChanged(nameof(SelectedCity));
            }
        }

        private void ChangeCity(int? cityId)
        {
            StreetList.Clear();
            if (!cityId.HasValue)
                return;
            foreach (var street in _requestService.GetStreets(cityId.Value).OrderBy(s => s.Name))
            {
                StreetList.Add(street);
            }
            OnPropertyChanged(nameof(StreetList));
        }

        private void ChangeStreet(int? streetId)
        {
            HouseList.Clear();
            if (!streetId.HasValue)
                return;
            foreach (var house in _requestService.GetHouses(streetId.Value).OrderBy(s => s.Building?.PadLeft(6, '0')).ThenBy(s => s.Corpus?.PadLeft(6, '0')))
            {
                HouseList.Add(house);
            }
            OnPropertyChanged(nameof(HouseList));
        }

        private void ChangeHouse(int? houseId)
        {
            FlatList.Clear();
            if (!houseId.HasValue)
                return;
            foreach (var flat in _requestService.GetFlats(houseId.Value).OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')))
            {
                FlatList.Add(flat);
            }
            OnPropertyChanged(nameof(FlatList));
        }

        public ObservableCollection<StreetDto> StreetList
        {
            get { return _streetList; }
            set { _streetList = value; OnPropertyChanged(nameof(StreetList)); }
        }

        public StreetDto SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                _selectedStreet = value;
                ChangeStreet(value?.Id);
                OnPropertyChanged(nameof(SelectedStreet));
            }
        }

        public ObservableCollection<HouseDto> HouseList
        {
            get { return _houseList; }
            set { _houseList = value; OnPropertyChanged(nameof(HouseList)); }
        }

        public HouseDto SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                ChangeHouse(value?.Id);
                OnPropertyChanged(nameof(SelectedHouse));
            }
        }

        public ObservableCollection<FlatDto> FlatList
        {
            get { return _flatList; }
            set { _flatList = value; OnPropertyChanged(nameof(FlatList)); }
        }

        public FlatDto SelectedFlat
        {
            get { return _selectedFlat; }
            set
            {
                _selectedFlat = value;
                if (_selectedFlat != null)
                {
                    LoadRequestsBySelectedAddress(_selectedFlat.Id);
                }
                else
                {
                    MetersHistoryList.Clear();
                }
                OnPropertyChanged(nameof(SelectedFlat));
            }
        }

        private void LoadRequestsBySelectedAddress(int addressId)
        {
            MetersHistoryList = new ObservableCollection<MetersDto>(_requestService.GetMetersByAddressId(addressId));
            PersonalAccount = MetersHistoryList.LastOrDefault()?.PersonalAccount;
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(Save)); } }



        private void Save(object sender)
        {
            if (SelectedFlat == null)
            {
                MessageBox.Show("Необходимо выбрать правильный адрес!", "Ошибка");
                return;
            }
            _requestService.SaveMeterValues(PhoneNumber,SelectedFlat.Id,Electro1,Electro2,HotWater1,ColdWater1,HotWater2,ColdWater2,Heating, _meterId,PersonalAccount,Heating2,Heating3);
            LoadRequestsBySelectedAddress(SelectedFlat.Id);
            MessageBox.Show("Данные успешно сохранены!", "Приборы учёта");

        }

        private ICommand _closeCommand;
        private double _electro1;
        private double _electro2;
        private double _hotWater1;
        private double _hotWater2;
        private double _coldWater1;
        private double _coldWater2;
        private double _heating;
        private ObservableCollection<MetersDto> _metersHistoryList;
        private int? _meterId;
        private string _personalAccount;
        private double _heating2;
        private double _heating3;

        public ICommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new CommandHandler(Close, true)); } }

        private void Close()
        {
            _view.Close();
        }

        public ObservableCollection<MetersDto> MetersHistoryList
        {
            get { return _metersHistoryList; }
            set { _metersHistoryList = value; OnPropertyChanged(nameof(MetersHistoryList));}
        }

        public void SetView(Window view)
        {
            _view = view;
        }

        public bool CanEdit => !_meterId.HasValue;

        public MeterDeviceViewModel(MeterListDto meter = null)
        {
            _meterId = meter?.Id;
            _requestService = new RequestService(AppSettings.DbConnection);
            StreetList = new ObservableCollection<StreetDto>();
            HouseList = new ObservableCollection<HouseDto>();
            FlatList = new ObservableCollection<FlatDto>();
            CityList = new ObservableCollection<CityDto>(_requestService.GetCities());
            if (CityList.Count > 0)
            {
                SelectedCity = CityList.FirstOrDefault();
            }
            MetersHistoryList = new ObservableCollection<MetersDto>();
            //AppSettings.LastIncomingCall = "9829388873";
            if (!string.IsNullOrEmpty(AppSettings.LastIncomingCall))
            {
                var clientInfoDto = _requestService.GetLastAddressByClientPhone(AppSettings.LastIncomingCall);
                if (clientInfoDto != null)
                {
                    SelectedStreet = StreetList.FirstOrDefault(s => s.Id == clientInfoDto.StreetId);
                    SelectedHouse = HouseList.FirstOrDefault(h => h.Building == clientInfoDto.Building &&
                                                      h.Corpus == clientInfoDto.Corpus);
                    SelectedFlat = FlatList.FirstOrDefault(f => f.Flat == clientInfoDto.Flat);
                }
            }
            if (meter!=null)
            {
                SelectedStreet = StreetList.FirstOrDefault(s => s.Id == meter.StreetId);
                SelectedHouse = HouseList.FirstOrDefault(h => h.Id == meter.HouseId);
                SelectedFlat = FlatList.FirstOrDefault(f => f.Id == meter.AddressId);
                ColdWater1 = meter.ColdWater1;
                HotWater1 = meter.HotWater1;
                ColdWater2 = meter.ColdWater2;
                HotWater2 = meter.HotWater2;
                Electro1 = meter.Electro1;
                Electro2 = meter.Electro2;
                Heating = meter.Heating;
                Heating2 = meter.Heating2??0;
                Heating3 = meter.Heating3??0;
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