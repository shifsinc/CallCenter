﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Stimulsoft.Report;
using WebApi.Models;

namespace WebApi.Services
{
    public static class RequestService
    {
        private static string _connectionString;

        static RequestService()
        {
            _connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130",
                "asterisk", "mysqlasterisk", "CallCenter");
        }

        public static WebUserDto WebLogin(string userName, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.DispexLogin2('{userName}','{password}')", conn)
                )
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WebUserDto
                            {
                                UserId = dataReader.GetInt32("UserId"),
                                Login = dataReader.GetNullableString("Login"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                WorkerId = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                                SpecialityId = dataReader.GetInt32("speciality_id"),
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web"),
                                CanCloseRequest = dataReader.GetBoolean("can_close_request"),
                                CanSetRating = dataReader.GetBoolean("can_set_rating"),
                                AllowStatistics = dataReader.GetBoolean("allow_statistics"),
                                CanChangeExecutors = dataReader.GetBoolean("can_change_executors"),
                                ServiceCompanyFilter = dataReader.GetBoolean("show_all_request"),
                                EnableAdminPage = dataReader.GetBoolean("enable_admin_page"),
                                PushId = dataReader.GetString("guid"),
                            };
                        }
                        dataReader.Close();
                    }
                }
                return null;
            }
        }

        internal static WebUserDto FindUserByToken(Guid refreshToken,DateTime expireDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {

                conn.Open();
                using (var cmd = new MySqlCommand($"Call CallCenter.DispexGetUserByToken(@Token,@ExpireDate)", conn)
                )
                {
                    cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                    cmd.Parameters.AddWithValue("@Token", refreshToken);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WebUserDto
                            {
                                UserId = dataReader.GetInt32("UserId"),
                                Login = dataReader.GetNullableString("Login"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                WorkerId = dataReader.GetInt32("id"),
                                ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                                SpecialityId = dataReader.GetInt32("speciality_id"),
                                CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web"),
                                CanCloseRequest = dataReader.GetBoolean("can_close_request"),
                                CanSetRating = dataReader.GetBoolean("can_set_rating"),
                                AllowStatistics = dataReader.GetBoolean("allow_statistics"),
                                CanChangeExecutors = dataReader.GetBoolean("can_change_executors"),
                                ServiceCompanyFilter = dataReader.GetBoolean("show_all_request"),
                                EnableAdminPage = dataReader.GetBoolean("enable_admin_page"),
                                PushId = dataReader.GetString("guid"),
                            };
                        }
                        dataReader.Close();
                    }
                }
                return null;
            }
        }


        internal static void AddRefreshToken(int workerId, Guid refreshToken,DateTime expireDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd =
                            new MySqlCommand(@"CALL CallCenter.DispexAddToken(@WorkerId,@Token,@ExpireDate);", conn))
                    {
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@Token", refreshToken);
                        cmd.Parameters.AddWithValue("@ExpireDate", expireDate);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        public static byte[] GetRequestActs(int workerId, int[] requestIds)
        {
            var requests = WebRequestsByIds(workerId, requestIds);
            var stiReport = new StiReport();
            stiReport.Load("templates\\act.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;

            var acts = requests.Select(r => new { Id = r.Id, CreateTime = r.CreateTime, ContactPhones = r.ContactPhones, Address = r.FullAddress, Workers = r.Master?.FullName, ClientPhones = r.ContactPhones, Service = r.ParentService + ": " + r.Service, Description = r.Description }).ToArray();

            stiReport.RegBusinessObject("", "Acts", acts);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Pdf, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("C:\\1\\act.pdf",reportStream.ToArray());
            return reportStream.ToArray();

        }

        public static byte[] GetRequestExcel(int workerId, int[] requestIds)
        {
            //Переделать на шаблон
            var requests = WebRequestsByIds(workerId, requestIds);
            var stiReport = new StiReport();
            stiReport.Load("templates\\requests.mrt");
            StiOptions.Engine.HideRenderingProgress = true;
            StiOptions.Engine.HideExceptions = true;
            StiOptions.Engine.HideMessages = true;

            var requestObj = requests.Select(r => new { Id = r.Id,ExecuteDate = r.ExecuteTime, ContactPhones = r.ContactPhones, Address = r.FullAddress, Workers = r.Master?.FullName, ClientPhones = r.ContactPhones, Service = r.ParentService + ": " + r.Service, Description = r.Description }).ToArray();

            stiReport.RegBusinessObject("", "Requests", requestObj);
            stiReport.Render();
            var reportStream = new MemoryStream();
            stiReport.ExportDocument(StiExportFormat.Excel2007, reportStream);
            reportStream.Position = 0;
            //File.WriteAllBytes("C:\\1\\excel.xlsx",reportStream.ToArray());
            return reportStream.ToArray();

        }

        public static WorkerDto[] GetWorkersByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetWorkers(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                            });
                        }
                        dataReader.Close();
                    }
                    return workers.ToArray();
                }
            }
        }
        public static WorkerDto[] GetExecutersByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetExecuters(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var workers = new List<WorkerDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            workers.Add(new WorkerDto
                            {
                                Id = dataReader.GetInt32("id"),
                                SurName = dataReader.GetString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                                SpecialityId = dataReader.GetNullableInt("speciality_id"),
                            });
                        }
                        dataReader.Close();
                    }
                    return workers.ToArray();
                }
            }
        }
        public static int[] GetAddressesId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetAddresses(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<int>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                                list.Add(dataReader.GetInt32("id"));
                        }
                        dataReader.Close();
                    }
                    return list.ToArray();
                }
            }
        }
        public static int[] GetHousesId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.DispexGetAllHouses(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<int>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                                list.Add(dataReader.GetInt32("id"));
                        }
                        dataReader.Close();
                    }
                    return list.ToArray();
                }
            }
        }
        public static StatusDto[] GetStatusesAll(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = "CALL CallCenter.DispexGetStatuses(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var types = new List<StatusDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            types.Add(new StatusDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Description = dataReader.GetString("Description")
                            });
                        }
                        dataReader.Close();
                    }
                    return types.ToArray();
                }
            }
        }
        public static StatusDto[] GetStatusesAllowedInWeb(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = "CALL CallCenter.DispexGetStatusesForSet(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var types = new List<StatusDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            types.Add(new StatusDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Description = dataReader.GetString("Description")
                            });
                        }
                        dataReader.Close();
                    }
                    return types.ToArray();
                }
            }
        }

        public static WebCallsDto[] GetWebCallsByRequestId(int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = @"SELECT rc.id,c.CallerIdNum,c.CreateTime,c.MonitorFile FROM CallCenter.Requests r
join CallCenter.RequestCalls rc on rc.request_id = r.id
join asterisk.ChannelHistory c on c.UniqueID = rc.uniqueID where r.id = @reqId order by c.CreateTime";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    var states = new List<WebCallsDto>();
                    cmd.Parameters.AddWithValue("@reqId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            states.Add(new WebCallsDto
                            {
                                Id = dataReader.GetInt32("id"),
                                PhoneNumber = dataReader.GetNullableString("CallerIdNum"),
                                CreateTime = dataReader.GetDateTime("CreateTime"),
                            });
                        }
                        dataReader.Close();
                    }
                    return states.ToArray();
                }
            }
        }
        public static byte[] GetRecordById(int recordId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                    new MySqlCommand(@"SELECT MonitorFile FROM CallCenter.RequestCalls r
    join asterisk.ChannelHistory c on c.UniqueID = r.uniqueID
    where r.id = @reqId", conn))
                {
                    cmd.Parameters.AddWithValue("@reqId", recordId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            var fileName = dataReader.GetNullableString("MonitorFile");
                            var serverIpAddress = conn.DataSource;
                            var localFileName =
                                fileName.Replace("/raid/monitor/", $"\\\\{serverIpAddress}\\mixmonitor\\")
                                    .Replace("/", "\\");
                            if (File.Exists(localFileName))
                            {
                                return File.ReadAllBytes(localFileName);
                            }
                            var localFileNameMp3 = localFileName.Replace(".wav", ".mp3");
                            if (File.Exists(localFileNameMp3))
                            {
                                return File.ReadAllBytes(localFileNameMp3);
                            }
                            return null;
                        }
                    }
                }
            }
            return new byte[0];
        }
        public static byte[] DownloadFile(int requestId, string fileName,string rootDir)
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists($"{rootDir}\\{requestId}"))
            {
                return File.ReadAllBytes($"{rootDir}\\{requestId}\\{fileName}");
            }
            return null;
        }

        public static StreetDto[] GetStreetsByWorkerId(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "CALL CallCenter.DispexGetStreets(@CurWorker)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    var streets = new List<StreetDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            streets.Add(new StreetDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Prefix = new StreetPrefixDto
                                {
                                    Id = dataReader.GetInt32("Prefix_id"),
                                    Name = dataReader.GetString("Prefix_Name"),
                                    ShortName = dataReader.GetString("ShortName")
                                },
                                CityId = dataReader.GetInt32("city_id")
                            });
                        }
                        dataReader.Close();
                    }
                    return streets.ToArray();
                }
            }
        }
        public static WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = @"CALL CallCenter.DispexGetHouses(@StreetId,@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@StreetId", streetId);
                    var houses = new List<HouseDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            houses.Add(new HouseDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                StreetId = streetId
                            });
                        }
                        dataReader.Close();
                    }
                    return houses.Select(h => new WebHouseDto {Id = h.Id, Name = h.FullName}).ToArray();
                }
            }
        }
        public static IList<ServiceDto> GetParentServices()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"SELECT id,name,can_send_sms, null parent_id, null parent_name FROM CallCenter.RequestTypes R where parrent_id is null and enabled = 1 order by name";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var services = new List<ServiceDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            services.Add(new ServiceDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CanSendSms = dataReader.GetBoolean("can_send_sms"),
                                ParentId = dataReader.GetNullableInt("parent_id"),
                                ParentName = dataReader.GetNullableString("parent_name")
                            });
                        }
                        dataReader.Close();
                    }
                    return services;
                }
            }
        }
        public static List<ServiceCompanyDto> GetServicesCompanies()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var query = "SELECT id,name FROM CallCenter.ServiceCompanies S where Enabled = 1 order by S.Name";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var companies = new List<ServiceCompanyDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            companies.Add(new ServiceCompanyDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name")
                            });
                        }
                        dataReader.Close();
                    }
                    return companies.OrderBy(i => i.Name).ToList();
                }
            }
        }

        public static IList<ServiceDto> GetServices(int[] parentIds)
        {
            if (parentIds == null || parentIds.Length == 0)
                return new List<ServiceDto>(0);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var ids = parentIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j);
                var query =
                    $@"SELECT t1.id,t1.name,t1.can_send_sms,t2.id parent_id, t2.name parent_name FROM CallCenter.RequestTypes t1
                        left join CallCenter.RequestTypes t2 on t2.id = t1.parrent_id
                        where t1.parrent_id in ({ids}) and t1.enabled = 1 order by t2.name,t1.name";
                    
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var services = new List<ServiceDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            services.Add(new ServiceDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                CanSendSms = dataReader.GetBoolean("can_send_sms"),
                                ParentId = dataReader.GetNullableInt("parent_id"),
                                ParentName = dataReader.GetNullableString("parent_name")
                            });
                        }
                        dataReader.Close();
                    }
                    return services;
                }
            }
        }
        public static RequestForListDto[] WebRequestListArrayParam(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetIds, int[] houseIds, int[] addressIds, int[] parentServiceIds, int[] serviceIds, int[] statusIds, int[] workerIds, int[] executerIds, int[] ratingIds,int[] companies, int[] warrantyIds, int[] immediateIds, int[] regionIds, bool badWork = false, bool garanty = false,bool onlyRetry = false, bool chargeable = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequests(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@WarrantyIds,@BadWork,@Garanty,@ClientPhone,@RatingIds,@CompaniesIds,@OnlyRetry,@OnlyChargeable,@ImmediateIds,@RegionIds)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                    cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                    cmd.Parameters.AddWithValue("@OnlyRetry", onlyRetry);
                    cmd.Parameters.AddWithValue("@OnlyChargeable", chargeable);
                    cmd.Parameters.AddWithValue("@StreetIds",
                        streetIds != null && streetIds.Length > 0
                            ? streetIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@HouseIds",
                        houseIds != null && houseIds.Length > 0
                            ? houseIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@AddressIds",
                        addressIds != null && addressIds.Length > 0
                            ? addressIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ParentServiceIds",
                        parentServiceIds != null && parentServiceIds.Length > 0
                            ? parentServiceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ServiceIds",
                        serviceIds != null && serviceIds.Length > 0
                            ? serviceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@StatusIds",
                        statusIds != null && statusIds.Length > 0
                            ? statusIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@WorkerIds",
                        workerIds != null && workerIds.Length > 0
                            ? workerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ExecuterIds",
                        executerIds != null && executerIds.Length > 0
                            ? executerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@BadWork", badWork);
                    cmd.Parameters.AddWithValue("@Garanty", garanty);
                    cmd.Parameters.AddWithValue("@ClientPhone", clientPhone);
                    cmd.Parameters.AddWithValue("@RatingIds",
                        ratingIds != null && ratingIds.Length > 0
                            ? ratingIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@CompaniesIds",
                        companies != null && companies.Length > 0
                            ? companies.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@WarrantyIds",
                        warrantyIds != null && warrantyIds.Length > 0
                            ? warrantyIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@ImmediateIds",
                        immediateIds != null && immediateIds.Length > 0
                            ? immediateIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);
                    cmd.Parameters.AddWithValue("@RegionIds",
                        regionIds != null && regionIds.Length > 0
                            ? regionIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
                                Executer = dataReader.GetNullableInt("executer_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("exec_sur_name"),
                                        FirstName = dataReader.GetNullableString("exec_first_name"),
                                        PatrName = dataReader.GetNullableString("exec_patr_name"),
                                    }
                                    : null,
                                CreateUser = new UserDto
                                {
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                                ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                ClientName = dataReader.GetNullableString("client_name"),
                                CloseDate = dataReader.GetNullableDateTime("close_date"),
                                DoneDate = dataReader.GetNullableDateTime("done_date"),
                                GarantyId = dataReader.GetInt32("garanty"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }
        public static RequestForListDto[] WebRequestsByIds(int currentWorkerId, int[] requestIds)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.DispexGetRequestsByIds(@CurWorker,@RequestIds)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestIds",
                        requestIds != null && requestIds.Length > 0
                            ? requestIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)
                            : null);

                    var requests = new List<RequestForListDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var recordId = dataReader.GetNullableInt("recordId");
                            requests.Add(new RequestForListDto
                            {
                                Id = dataReader.GetInt32("id"),
                                StreetPrefix = dataReader.GetString("prefix_name"),
                                RegionId = dataReader.GetNullableInt("region_id"),
                                RegionName = dataReader.GetNullableString("region_name"),
                                StreetName = dataReader.GetString("street_name"),
                                AddressType = dataReader.GetString("address_type"),
                                CompanyId = dataReader.GetNullableInt("service_company_id"),
                                CompanyName = dataReader.GetNullableString("company_name"),
                                Flat = dataReader.GetString("flat"),
                                Building = dataReader.GetString("building"),
                                Corpus = dataReader.GetNullableString("corps"),
                                Entrance = dataReader.GetNullableString("entrance"),
                                FirstRecordId = recordId,
                                HasRecord = recordId.HasValue,
                                HasAttachment = dataReader.GetBoolean("has_attach"),
                                IsBadWork = dataReader.GetBoolean("bad_work"),
                                IsImmediate = dataReader.GetBoolean("is_immediate"),
                                Floor = dataReader.GetNullableString("floor"),
                                CreateTime = dataReader.GetDateTime("create_time"),
                                Description = dataReader.GetNullableString("description"),
                                ContactPhones = dataReader.GetNullableString("client_phones"),
                                ParentService = dataReader.GetNullableString("parent_name"),
                                Service = dataReader.GetNullableString("service_name"),
                                ParentServiceId = dataReader.GetNullableInt("parent_id"),
                                ServiceId = dataReader.GetNullableInt("service_id"),
                                Master = dataReader.GetNullableInt("worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                    }
                                    : null,
                                Executer = dataReader.GetNullableInt("executer_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("executer_id"),
                                        SurName = dataReader.GetNullableString("exec_sur_name"),
                                        FirstName = dataReader.GetNullableString("exec_first_name"),
                                        PatrName = dataReader.GetNullableString("exec_patr_name"),
                                    }
                                    : null,
                                CreateUser = new UserDto
                                {
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                                ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                                ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                                Rating = dataReader.GetNullableString("Rating"),
                                BadWork = dataReader.GetBoolean("bad_work"),
                                IsRetry = dataReader.GetBoolean("retry"),
                                Garanty = dataReader.GetBoolean("garanty"),
                                StatusId = dataReader.GetInt32("req_status_id"),
                                Status = dataReader.GetNullableString("Req_Status"),
                                TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                                RatingDescription = dataReader.GetNullableString("RatingDesc"),
                                LastNote = dataReader.GetNullableString("last_note"),
                                IsChargeable = dataReader.GetBoolean("is_chargeable"),
                                ClientName = dataReader.GetNullableString("client_name"),
                                TaskId = dataReader.GetNullableInt("task_id"),
                                TaskStart = dataReader.GetNullableDateTime("task_from"),
                                TaskEnd = dataReader.GetNullableDateTime("task_to"),
                                TaskWorker = dataReader.GetNullableInt("task_worker_id") != null
                                    ? new UserDto
                                    {
                                        Id = dataReader.GetInt32("task_worker_id"),
                                        SurName = dataReader.GetNullableString("task_sur_name"),
                                        FirstName = dataReader.GetNullableString("task_first_name"),
                                        PatrName = dataReader.GetNullableString("task_patr_name"),
                                    }
                                    : null,
                            });
                        }
                        dataReader.Close();
                    }
                    return requests.ToArray();
                }
            }
        }

        public static void AddNewNote(int requestId, string note, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestNoteHistory (request_id,operation_date,user_id,note,worker_id)
 values(@RequestId,sysdate(),0,@Note,@WorkerId);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }


        }
        public static void SetRating(int workerId, int requestId, int ratingId, string description)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"CALL CallCenter.DispexSetRating2(@WorkerId,@RequestId,@RatingId,@Desc);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@RatingId", ratingId);
                        cmd.Parameters.AddWithValue("@Desc", description);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        public static void SetExecuteDate(int workerId, int requestId, DateTime executeDate,string note )
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"CALL CallCenter.DispexSetExecuteDate2(@WorkerId,@RequestId,@ExecuteDate,@Note);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
                        cmd.Parameters.AddWithValue("@Note", note);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        public static FlatDto[] GetFlats(int houseId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"SELECT A.id,A.type_id,A.flat,T.Name FROM CallCenter.Addresses A
    join CallCenter.AddressesTypes T on T.id = A.type_id
    where A.enabled = true and A.house_id = @HouseId", conn))
                {
                    cmd.Parameters.AddWithValue("@HouseId", houseId);

                    var flats = new List<FlatDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            flats.Add(new FlatDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Flat = dataReader.GetString("flat"),
                                TypeId = dataReader.GetInt32("type_id"),
                                TypeName = dataReader.GetString("Name"),
                            });
                        }
                        dataReader.Close();
                    }
                    return flats.OrderBy(s => s.TypeId).ThenBy(s => s.Flat?.PadLeft(6, '0')).ToArray();
                }
            }
        }

        public static string CreateRequest(int workerId, string phone, string fio, int addressId, int typeId, int? masterId, int? executerId, string description, bool isChargeable = false, DateTime? executeDate = null,int warrantyId = 0)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query =
                    "call CallCenter.DispexCreateRequest(@WorkerId,@Phone,@Fio,@AddressId,@TypeId,@MasterId,@ExecuterId,@Desc,@IsChargeable,@ExecuteDate,@IsWarranty);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Fio", fio);
                    cmd.Parameters.AddWithValue("@AddressId", addressId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);
                    cmd.Parameters.AddWithValue("@ExecuterId", executerId);
                    cmd.Parameters.AddWithValue("@Desc", description);
                    cmd.Parameters.AddWithValue("@IsChargeable", isChargeable);
                    cmd.Parameters.AddWithValue("@ExecuteDate", executeDate);
                    cmd.Parameters.AddWithValue("@IsWarranty", warrantyId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("requestId");
                    }
                }
            }
        }

        public static void AttachFileToRequest(int workerId, int requestId, string fileName, string generatedFileName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =
                        new MySqlCommand(@"insert into CallCenter.RequestAttachments(request_id,name,file_name,create_date,user_id,worker_id)
 values(@RequestId,@Name,@FileName,sysdate(),0,@WorkerId);", conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@Name", fileName);
                    cmd.Parameters.AddWithValue("@FileName", generatedFileName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<AttachmentDto> GetAttachments(int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(@"SELECT a.id,a.request_id,a.name,a.file_name,a.create_date,u.id user_id,u.SurName,u.FirstName,u.PatrName FROM CallCenter.RequestAttachments a
 join CallCenter.Users u on u.id = a.user_id where a.deleted = 0 and a.request_id = @requestId", conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var attachments = new List<AttachmentDto>();
                        while (dataReader.Read())
                        {
                            attachments.Add(new AttachmentDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                FileName = dataReader.GetString("file_name"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                RequestId = dataReader.GetInt32("request_id"),
                                User = new UserDto()
                                {
                                    Id = dataReader.GetInt32("user_id"),
                                    SurName = dataReader.GetNullableString("SurName"),
                                    FirstName = dataReader.GetNullableString("FirstName"),
                                    PatrName = dataReader.GetNullableString("PatrName"),
                                }
                            });
                        }
                        dataReader.Close();
                        return attachments;
                    }
                }
            }
        }

        public static List<NoteDto> GetNotes(int workerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "call CallCenter.DispexGetNotes(@WorkerId,@RequestId);";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var noteList = new List<NoteDto>();
                        while (dataReader.Read())
                        {
                            noteList.Add(new NoteDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Date = dataReader.GetDateTime("operation_date"),
                                Note = dataReader.GetNullableString("note"),
                                User = new UserDto
                                {
                                    Id = dataReader.GetInt32("create_user_id"),
                                    SurName = dataReader.GetNullableString("surname"),
                                    FirstName = dataReader.GetNullableString("firstname"),
                                    PatrName = dataReader.GetNullableString("patrname"),
                                },
                            });
                        }
                        dataReader.Close();
                        return noteList;
                    }
                }

            }
        }
        public static List<CityRegionDto> GetRegions(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery = "call CallCenter.DispexGetCityRegions(@WorkerId);";
                using (
                    var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        var regions = new List<CityRegionDto>();
                        while (dataReader.Read())
                        {
                            regions.Add(new CityRegionDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("note")
                            });
                        }
                        dataReader.Close();
                        return regions;
                    }
                }

            }
        }

        public static void AddNewState(int requestId, int stateId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (
                        var cmd =
                            new MySqlCommand(@"insert into CallCenter.RequestStateHistory (request_id,operation_date,user_id,state_id,worker_id) 
    values(@RequestId,sysdate(),0,@StatusId,@WorkerId);", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@WorkerId", workerId);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd =
                        new MySqlCommand(
                            @"update CallCenter.Requests set state_id = @StatusId where id = @RequestId", conn))
                    {
                        cmd.Parameters.AddWithValue("@RequestId", requestId);
                        cmd.Parameters.AddWithValue("@StatusId", stateId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

        }
        public static void SetNewService(int requestId, int serviceId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetService(@WorkerId,@Id,@NewService);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewService", serviceId);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public static void SetNewMaster(int requestId, int masterId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetMaster(@WorkerId,@Id,@NewMaster);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewMaster", masterId);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public static void SetNewExecuter(int requestId, int executerId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.DispexSetExecutor(@WorkerId,@Id,@NewExecuter);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", requestId);
                    cmd.Parameters.AddWithValue("@NewExecuter", executerId);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        public static WarrantyDocDto[] WarrantyGetDocs(int currentWorkerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetDocs(@CurWorker,@RequestId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    var docs = new List<WarrantyDocDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            docs.Add(new WarrantyDocDto
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Extension = dataReader.GetString("extension"),
                                RequestId = dataReader.GetNullableInt("request_id"),
                                CreateDate = dataReader.GetDateTime("create_date"),
                                InsertDate = dataReader.GetDateTime("insert_date"),
                                Direction = dataReader.GetString("direction"),

                                CreateWorker = new WorkerDto()
                                    {
                                        Id = dataReader.GetInt32("worker_id"),
                                        SurName = dataReader.GetNullableString("sur_name"),
                                        FirstName = dataReader.GetNullableString("first_name"),
                                        PatrName = dataReader.GetNullableString("patr_name"),
                                        Phone = dataReader.GetNullableString("phone"),
                                    },
                                Organization = dataReader.GetNullableInt("org_id") != null
                                 ? new WarrantyOrganizationDto()
                                    {
                                        Id = dataReader.GetInt32("org_id"),
                                        Name = dataReader.GetNullableString("org_name"),
                                        Inn = dataReader.GetNullableString("org_inn"),
                                        DirectorFio = dataReader.GetNullableString("director_fio"),
                                    }
                                 : null,
                                Type = new WarrantyTypeDto()
                                {
                                    Id = dataReader.GetInt32("type_id"),
                                    Name = dataReader.GetNullableString("type_name"),
                                    IsAct = dataReader.GetBoolean("is_act"),
                                }

                            });
                        }
                        dataReader.Close();
                    }
                    return docs.ToArray();
                }
            }
        }
        public static WarrantyInfoDto WarrantyGetInfo(int currentWorkerId, int requestId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetInfo(@CurWorker,@RequestId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if(dataReader.Read())
                        {
                            return new WarrantyInfoDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                StartDate = dataReader.GetNullableDateTime("start_date"),
                                BeginDate = dataReader.GetNullableDateTime("begin_date"),
                                EndDate = dataReader.GetNullableDateTime("end_date"),
                                InsertDate = dataReader.GetDateTime("insert_date"),
                                ContactName = dataReader.GetNullableString("contact_name"),
                                ContactPhone = dataReader.GetNullableString("contact_phone"),
                                OrgId = dataReader.GetNullableInt("org_id"),
                                Organization = dataReader.GetNullableInt("org_id") != null
                                    ? new WarrantyOrganizationDto()
                                    {
                                        Id = dataReader.GetInt32("org_id"),
                                        Name = dataReader.GetNullableString("org_name"),
                                        Inn = dataReader.GetNullableString("org_inn"),
                                        DirectorFio = dataReader.GetNullableString("director_fio"),
                                    }
                                    : null,

                            };
                        }
                        dataReader.Close();
                    }
                    return null;
                }
            }
        }
        public static void WarrantySetInfo(int workerId, WarrantyInfoDto info)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyAddInfo(@WorkerId,@RequestId,@OrgId,@ContactName,@ContactPhone,@StartDate,@BeginDate,@EndDate);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", info.RequestId);
                    cmd.Parameters.AddWithValue("@OrgId", info.OrgId);
                    cmd.Parameters.AddWithValue("@ContactName", info.ContactName);
                    cmd.Parameters.AddWithValue("@ContactPhone", info.ContactPhone);
                    cmd.Parameters.AddWithValue("@StartDate", info.StartDate);
                    cmd.Parameters.AddWithValue("@BeginDate", info.BeginDate);
                    cmd.Parameters.AddWithValue("@EndDate", info.EndDate);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void WarrantyAddDoc(int id,int? orgId, int typeId, string name, DateTime docDate, string fileName, string direction, int workerId, string extension)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyAddDoc(@WorkerId,@Id,@OrgId,@TypeId,@Name,@FileName,@DocDate,@Direction,@Extension);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@OrgId", orgId);
                    cmd.Parameters.AddWithValue("@TypeId", typeId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@DocDate", docDate);
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@Direction", direction);
                    cmd.Parameters.AddWithValue("@Extension", extension);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void WarrantyDeleteDoc(int id, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyDeleteDoc(@WorkerId,@Id);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void WarrantySetState(int id, int stateId, int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantySetState(@WorkerId,@Id,@State);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@State", stateId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<WarrantyTypeDto> WarrantyGetDocTypes(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.WarrantyGetDocTypes(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<WarrantyTypeDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new WarrantyTypeDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                IsAct = dataReader.GetBoolean("is_act"),
                            };
                            list.Add(type);
                        }
                        dataReader.Close();
                    }
                    return list;
                }
            }
        }
        public static IEnumerable<WarrantyOrganizationDto> WarrantyGetOrganizations(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.WarrantyGetOrgs(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<WarrantyOrganizationDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new WarrantyOrganizationDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Inn = dataReader.GetNullableString("inn"),
                                DirectorFio = dataReader.GetNullableString("director_fio"),
                            };
                            list.Add(type);
                        }
                        dataReader.Close();
                    }
                    return list;
                }
            }
        }
        public static void WarrantyAddOrg(int workerId, WarrantyOrganizationDto organization)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantyCreateOrgs(@WorkerId,@Name,@Inn,@Fio);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Name", organization.Name);
                    cmd.Parameters.AddWithValue("@Inn", organization.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organization.DirectorFio);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void WarrantyEditOrg(int workerId, WarrantyOrganizationDto organization)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "call CallCenter.WarrantySetOrgs(@WorkerId,@Id,@Inn,@Fio);";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@Id", organization.Id);
                    cmd.Parameters.AddWithValue("@Inn", organization.Inn);
                    cmd.Parameters.AddWithValue("@Fio", organization.DirectorFio);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static WarrantyFileInfoDto WarrantyGetDocFileName(int workerId, int id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var sqlQuery =
                    "CALL CallCenter.WarrantyGetDocFileName(@CurWorker,@DocId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CurWorker", workerId);
                    cmd.Parameters.AddWithValue("@DocId", id);

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new WarrantyFileInfoDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                Name = dataReader.GetNullableString("name"),
                                FileName = dataReader.GetString("filename")
                            };
                        }
                        dataReader.Close();
                    }
                    return null;
                }
            }
        }


        public static ScheduleTaskDto[] GetScheduleTask(int currentWorkerId, int? workerId, DateTime fromDate,DateTime toDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.DispexGetScheduleTask(@CurrentWorkerId,@WorkerId,@FromDate,@ToDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));

                    var taskDtos = new List<ScheduleTaskDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            taskDtos.Add(new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                Worker = new ScheduleWorkerDto
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                    Phone = dataReader.GetNullableString("phone"),
                                }
                            });
                        }
                        dataReader.Close();
                    }
                    return taskDtos.ToArray();
                }
            }
        }
        public static ScheduleTaskDto[] GetAllScheduleTask(int currentWorkerId, int? workerId, DateTime fromDate,DateTime toDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"CALL CallCenter.DispexGetAllScheduleTask(@CurrentWorkerId,@WorkerId,@FromDate,@ToDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));

                    var taskDtos = new List<ScheduleTaskDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            taskDtos.Add(new ScheduleTaskDto
                            {
                                Id = dataReader.GetInt32("id"),
                                RequestId = dataReader.GetInt32("request_id"),
                                FromDate = dataReader.GetDateTime("from_date"),
                                ToDate = dataReader.GetDateTime("to_date"),
                                Worker = new ScheduleWorkerDto
                                {
                                    Id = dataReader.GetInt32("worker_id"),
                                    SurName = dataReader.GetNullableString("sur_name"),
                                    FirstName = dataReader.GetNullableString("first_name"),
                                    PatrName = dataReader.GetNullableString("patr_name"),
                                    Phone = dataReader.GetNullableString("phone"),
                                }
                            });
                        }
                        dataReader.Close();
                    }
                    return taskDtos.ToArray();
                }
            }
        }

//        public ScheduleTaskDto GetScheduleTaskByRequestId(int requestId)
//        {
//            ScheduleTaskDto result = null;
//            var query = @"SELECT s.id,w.id worker_id,w.sur_name,w.first_name,w.patr_name,s.request_id,s.from_date,s.to_date,s.event_description FROM CallCenter.ScheduleTasks s
//join CallCenter.Workers w on s.worker_id = w.id
//where s.request_id = @RequestId and deleted = 0;";
//            using (var cmd = new MySqlCommand(query, _dbConnection))
//            {
//                cmd.Parameters.AddWithValue("@RequestId", requestId);

//                using (var dataReader = cmd.ExecuteReader())
//                {
//                    if (dataReader.Read())
//                    {
//                        result = new ScheduleTaskDto
//                        {
//                            Id = dataReader.GetInt32("id"),
//                            RequestId = dataReader.GetNullableInt("request_id"),
//                            Worker = new WorkerDto()
//                            {
//                                Id = dataReader.GetInt32("worker_id"),
//                                SurName = dataReader.GetString("sur_name"),
//                                FirstName = dataReader.GetNullableString("first_name"),
//                                PatrName = dataReader.GetNullableString("patr_name"),
//                            },
//                            FromDate = dataReader.GetDateTime("from_date"),
//                            ToDate = dataReader.GetDateTime("to_date"),
//                            EventDescription = dataReader.GetNullableString("event_description")
//                        };
//                    }
//                    dataReader.Close();
//                }
//                return result;
//            }
//        }

        public static string AddScheduleTask(int currentWorkerId, int workerId, int? requestId, DateTime fromDate,
            DateTime toDate, string eventDescription)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =new MySqlCommand(@"CALL CallCenter.DispexScheduleTaskAdd(@CurrentWorkerId,@WorkerId,@RequestId,@FromDate,@ToDate)",conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataReader.Read();
                        return dataReader.GetNullableString("taskId");
                    }

                }
            }
        }
        public static void UpdateScheduleTask(int currentWorkerId, int taskId, int workerId, int? requestId, DateTime fromDate,
            DateTime toDate, string eventDescription)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd =new MySqlCommand(@"CALL CallCenter.DispexScheduleTaskUpdate(@CurrentWorkerId,@TaskId,@WorkerId,@RequestId,@FromDate,@ToDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@TaskId", taskId);
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteScheduleTask(int currentWorkerId, int sheduleId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (
                    var cmd =
                        new MySqlCommand(
                            @"CALL CallCenter.DispexScheduleTaskDrop(@CurrentWorkerId,@TaskId)",
                            conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentWorkerId", currentWorkerId);
                    cmd.Parameters.AddWithValue("@TaskId", sheduleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<ReportDto> ReportsGetAwailable(int workerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var sqlQuery = @"CALL CallCenter.ReportGetAwailable(@WorkerId)";
                using (var cmd = new MySqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkerId", workerId);
                    var list = new List<ReportDto>();
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var type = new ReportDto()
                            {
                                Id = dataReader.GetInt32("id"),
                                Name = dataReader.GetString("name"),
                                Url = dataReader.GetString("url"),
                            };
                            list.Add(type);
                        }
                        dataReader.Close();
                    }
                    return list;
                }
            }
        }
    }
}
