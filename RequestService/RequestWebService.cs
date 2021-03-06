using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using RequestServiceImpl.Dto;

namespace RequestServiceImpl
{
    public partial class RequestService //Web
    {
        public WebUserDto WebLogin(string userName, string password)
        {
            using (var cmd = new MySqlCommand($"Call CallCenter.WebLogin('{userName}','{password}')", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new WebUserDto
                        {
                            UserId = dataReader.GetInt32("UserId"),
                            SurName = dataReader.GetString("SurName"),
                            FirstName = dataReader.GetNullableString("FirstName"),
                            PatrName = dataReader.GetNullableString("PatrName"),
                            WorkerId = dataReader.GetInt32("worker_id"),
                            ServiceCompanyId = dataReader.GetInt32("service_company_id"),
                            SpecialityId = dataReader.GetInt32("speciality_id"),
                            CanCreateRequestInWeb = dataReader.GetBoolean("can_create_in_web")
                        };
                    }
                    dataReader.Close();
                }
            }
            return null;
        }
        public AttachmentDto[] GetAttachmentsWeb(int requestId)
        {
            return GetAttachmentsCore(requestId,_dbConnection).ToArray();
        }

        public StatInfoDto[] GetRequestByUsersInfo()
        {
            var result = new List<StatInfoDto>();
            using (var cmd = new MySqlCommand(@"SELECT dat stat_date,U.id user_id,concat(U.SurName,' ',U.FirstName,' ',U.PatrName) user_name,count(R.id) request_count FROM
                (select DATE(sysdate() - interval b.val day) dat from(select 1 val union select 2 union select 3 union select 4 union select 5 union select 6 union select 7) b) calendar
                left join CallCenter.Requests R on DATE(R.create_time) = dat
            left join CallCenter.Users U on U.id = R.create_user_id
            group by dat, U.id, concat(U.SurName, ' ', U.FirstName, ' ', U.PatrName) order by dat;", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new StatInfoDto
                        {
                            StatDate = dataReader.GetDateTime("stat_date"),
                            Name = dataReader.GetNullableString("user_name"),
                            Count = dataReader.GetNullableInt("request_count")
                        });
                    }
                    dataReader.Close();
                }
            }
            return result.ToArray();
        }

        public List<RequestRatingListDto> GetRequestRatings(int requestId)
        {
            var result = new List<RequestRatingListDto>();
            using (var cmd = new MySqlCommand(@"SELECT r.id,request_id,create_date,rating_id,Description,t.name rating_name,
 u.Id user_id, u.SurName, u.FirstName,u.PatrName FROM CallCenter.RequestRating r
 join CallCenter.Users u on u.id = r.user_id
 join CallCenter.RatingTypes t on t.id = rating_id
 where request_id = @RequestId;", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new RequestRatingListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            CreateDate = dataReader.GetDateTime("create_date"),
                            Description = dataReader.GetNullableString("Description"),
                            Rating = dataReader.GetNullableString("rating_name"),
                            CreateUser = new RequestUserDto()
                            {
                                Id = dataReader.GetInt32("user_id"),
                                SurName = dataReader.GetNullableString("SurName"),
                                FirstName = dataReader.GetNullableString("FirstName"),
                                PatrName = dataReader.GetNullableString("PatrName"),
                            }
                        });
                    }
                    dataReader.Close();
                }
            }
            return result;
        }

        public StatInfoDto[] GetRequestByWorkersInfo()
        {
            var result = new List<StatInfoDto>();
            using (var cmd = new MySqlCommand(@"SELECT dat stat_date,W.id user_id,trim(concat(W.sur_name,' ',IFNULL(W.first_name,''),' ',IFNULL(W.patr_name,''))) user_name,count(R.id) request_count FROM
 (select DATE(sysdate() - interval b.val day) dat from (select 1 val union select 2 union select 3 union select 4 union select 5 union select 6 union select 7) b) calendar
 left join CallCenter.Requests R on DATE(R.create_time) = dat
 left join CallCenter.Workers W on W.id = R.worker_id
 group by dat,W.id order by dat;", _dbConnection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new StatInfoDto
                        {
                            StatDate = dataReader.GetDateTime("stat_date"),
                            Name = dataReader.GetNullableString("user_name"),
                            Count = dataReader.GetNullableInt("request_count")
                        });
                    }
                    dataReader.Close();
                }
            }
            return result.ToArray();
        }

        //Replased by  WebRequestListArrayParam
        public RequestForListDto[] WebRequestList2(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int? streetId, int? houseId, int? addressId, int? parentServiceId, int? serviceId, int? statusId, int? workerId, bool badWork = false, bool garanty = false, string clientPhone = null, int? rating = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery = "CALL CallCenter.WebGetRequests2(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetId,@HouseId,@AddressId,@ParentServiceId,@ServiceId,@StatusId,@WorkerId,@BadWork,@Warranty,@ClientPhone,null,@Rating)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                cmd.Parameters.AddWithValue("@StreetId", streetId);
                cmd.Parameters.AddWithValue("@HouseId", houseId);
                cmd.Parameters.AddWithValue("@AddressId", addressId);
                cmd.Parameters.AddWithValue("@ParentServiceId", parentServiceId);
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@BadWork", badWork);
                cmd.Parameters.AddWithValue("@Warranty", garanty);
                cmd.Parameters.AddWithValue("@ClientPhone", clientPhone);
                cmd.Parameters.AddWithValue("@Rating", rating);

                var requests = new List<RequestForListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new RequestForListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetPrefix = dataReader.GetString("prefix_name"),
                            StreetName = dataReader.GetString("street_name"),
                            AddressType = dataReader.GetString("address_type"),
                            Flat = dataReader.GetString("flat"),
                            Building = dataReader.GetString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Entrance = dataReader.GetNullableString("entrance"),
                            Floor = dataReader.GetNullableString("floor"),
                            CreateTime = dataReader.GetDateTime("create_time"),
                            Description = dataReader.GetNullableString("description"),
                            ContactPhones = dataReader.GetNullableString("client_phones"),
                            ParentService = dataReader.GetNullableString("parent_name"),
                            Service = dataReader.GetNullableString("service_name"),
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
                            CreateUser = new RequestUserDto
                            {
                                Id = dataReader.GetInt32("create_user_id"),
                                SurName = dataReader.GetNullableString("surname"),
                                FirstName = dataReader.GetNullableString("firstname"),
                                PatrName = dataReader.GetNullableString("patrname"),
                            },
                            ExecuteTime = dataReader.GetNullableDateTime("execute_date"),
                            ExecutePeriod = dataReader.GetNullableString("Period_Name"),
                            TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                            Rating = dataReader.GetNullableString("Rating"),
                            RatingDescription = dataReader.GetNullableString("RatingDesc"),
                            BadWork = dataReader.GetBoolean("bad_work"),
                            IsRetry = dataReader.GetBoolean("retry"),
                            Warranty = dataReader.GetInt32("garanty"),
                            Immediate = dataReader.GetBoolean("is_immediate"),
                            StatusId = dataReader.GetInt32("req_status_id"),
                            Status = dataReader.GetNullableString("Req_Status"),
                        });
                    }
                    dataReader.Close();
                }
                return requests.ToArray();
            }
        }
        public RequestForListDto[] WebRequestListArrayParam(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int[] streetIds, int[] houseIds, int[] addressIds, int[] parentServiceIds, int[] serviceIds, int[] statusIds, int[] workerIds,int[] executerIds, int[] ratingIds, bool badWork = false, bool garanty = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery = "CALL CallCenter.WebGetRequestsArrayParam(@CurWorker,@RequestId,@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@StreetIds,@HouseIds,@AddressIds,@ParentServiceIds,@ServiceIds,@StatusIds,@WorkerIds,@ExecuterIds,@BadWork,@Warranty,@ClientPhone,@RatingIds)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
                cmd.Parameters.AddWithValue("@StreetIds", streetIds!=null && streetIds.Length> 0 ? streetIds.Select(i=>i.ToString()).Aggregate((i,j)=>i + "," + j) : null);
                cmd.Parameters.AddWithValue("@HouseIds", houseIds != null && houseIds.Length > 0 ? houseIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@AddressIds", addressIds != null && addressIds.Length > 0 ? addressIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@ParentServiceIds", parentServiceIds != null && parentServiceIds.Length > 0 ? parentServiceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@ServiceIds", serviceIds != null && serviceIds.Length > 0 ? serviceIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@StatusIds", statusIds != null && statusIds.Length > 0 ? statusIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@WorkerIds", workerIds != null && workerIds.Length > 0 ? workerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@ExecuterIds", executerIds != null && executerIds.Length > 0 ? executerIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);
                cmd.Parameters.AddWithValue("@BadWork", badWork);
                cmd.Parameters.AddWithValue("@Warranty", garanty);
                cmd.Parameters.AddWithValue("@ClientPhone", clientPhone);
                cmd.Parameters.AddWithValue("@RatingIds", ratingIds != null && ratingIds.Length > 0 ? ratingIds.Select(i => i.ToString()).Aggregate((i, j) => i + "," + j) : null);

                var requests = new List<RequestForListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new RequestForListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetPrefix = dataReader.GetString("prefix_name"),
                            StreetName = dataReader.GetString("street_name"),
                            AddressType = dataReader.GetString("address_type"),
                            Flat = dataReader.GetString("flat"),
                            Building = dataReader.GetString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Entrance = dataReader.GetNullableString("entrance"),
                            Floor = dataReader.GetNullableString("floor"),
                            CreateTime = dataReader.GetDateTime("create_time"),
                            Description = dataReader.GetNullableString("description"),
                            ContactPhones = dataReader.GetNullableString("client_phones"),
                            ParentService = dataReader.GetNullableString("parent_name"),
                            Service = dataReader.GetNullableString("service_name"),
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
                            Executer = dataReader.GetNullableInt("executer_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("executer_id"),
                                SurName = dataReader.GetNullableString("exec_sur_name"),
                                FirstName = dataReader.GetNullableString("exec_first_name"),
                                PatrName = dataReader.GetNullableString("exec_patr_name"),
                            } : null,
                            CreateUser = new RequestUserDto
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
                            Warranty = dataReader.GetInt32("garanty"),
                            StatusId = dataReader.GetInt32("req_status_id"),
                            Status = dataReader.GetNullableString("Req_Status"),
                            TermOfExecution = dataReader.GetNullableDateTime("term_of_execution"),
                        });
                    }
                    dataReader.Close();
                }
                return requests.ToArray();
            }
        }
        public RequestForListDto[] WebRequestList(int currentWorkerId, int? requestId, bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate, int? streetId, int? houseId, int? addressId, int? parentServiceId, int? serviceId, int? statusId, int? workerId, bool badWork = false, string clientPhone = null)
        {
            var findFromDate = fromDate.Date;
            var findToDate = toDate.Date.AddDays(1).AddSeconds(-1);
            var sqlQuery =
                @"SELECT R.id,R.create_time,sp.name as prefix_name,s.name as street_name,h.building,h.corps,at.Name address_type, a.flat,
    R.worker_id, w.sur_name,w.first_name,w.patr_name, create_user_id,u.surname,u.firstname,u.patrname,
    R.execute_date,p.Name Period_Name, R.description,rt.name service_name, rt2.name parent_name, group_concat(distinct cp.Number order by rc.IsMain desc separator ', ') client_phones,
    rating.Name Rating,R.bad_work,R.floor,R.entrance,
    RS.id req_status_id,
    RS.Description Req_Status,R.term_of_execution
    FROM CallCenter.Requests R
    join CallCenter.RequestState RS on RS.id = R.state_id
    join CallCenter.WebStateToState WS on WS.state_id = R.state_id
    join CallCenter.Addresses a on a.id = R.address_id
    join CallCenter.AddressesTypes at on at.id = a.type_id
    join CallCenter.Houses h on h.id = house_id

    join CallCenter.Workers lw on lw.service_company_id = h.service_company_id

    join CallCenter.Streets s on s.id = street_id
    join CallCenter.StreetPrefixes sp on sp.id = s.prefix_id
    join CallCenter.RequestTypes rt on rt.id = R.type_id
    join CallCenter.RequestTypes rt2 on rt2.id = rt.parrent_id
    left join CallCenter.Workers w on w.id = R.worker_id
    left join CallCenter.RequestContacts rc on rc.request_id = R.id
    left join CallCenter.ClientPhones cp on cp.id = rc.clientPhone_id
    join CallCenter.Users u on u.id = create_user_id
    left join CallCenter.PeriodTimes p on p.id = R.period_time_id
    left join CallCenter.RequestRating rtype on rtype.request_id = R.id
    left join CallCenter.RatingTypes rating on rtype.rating_id = rating.id";
            if (!requestId.HasValue)
            {
                sqlQuery +=
                    " where lw.id = @CurWorker";
                    //" where R.worker_id in (select id from CallCenter.Workers where id = @CurWorker union SELECT dependent_worker_id FROM CallCenter.WorkersRelations W where parent_worker_id =  @CurWorker and can_view = 1)";
                if (filterByCreateDate)
                {
                    sqlQuery += " and R.create_time between @FromDate and @ToDate";
                }
                else
                {
                    findFromDate = executeFromDate.Date;
                    findToDate = executeToDate.Date.AddDays(1).AddSeconds(-1);

                    sqlQuery += " and R.execute_date between @FromDate and @ToDate";
                }
                if (streetId.HasValue)
                    sqlQuery += $" and s.id = {streetId.Value}";
                if (houseId.HasValue)
                    sqlQuery += $" and h.id = {houseId.Value}";
                if (addressId.HasValue)
                    sqlQuery += $" and a.id = {addressId.Value}";
                if (serviceId.HasValue)
                    sqlQuery += $" and rt.id = {serviceId.Value}";
                if (parentServiceId.HasValue)
                    sqlQuery += $" and rt2.id = {parentServiceId.Value}";
                if (statusId.HasValue)
                    sqlQuery += $" and WS.Web_State_Id = {statusId.Value}";
                if (workerId.HasValue)
                    sqlQuery += $" and w.id = {workerId.Value}";
                if (badWork)
                    sqlQuery += " and R.bad_work = 1";
                if(!string.IsNullOrEmpty(clientPhone))
                    sqlQuery += $" and cp.Number = '{clientPhone}'";
            }
            else
            {
                sqlQuery += " where R.id = @RequestId";
            }
            sqlQuery += " group by R.id order by id desc";
            using (var cmd =
                new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CurWorker", currentWorkerId);
                if (!requestId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@FromDate", findFromDate);
                    cmd.Parameters.AddWithValue("@ToDate", findToDate);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@RequestId", requestId);
                }
                var requests = new List<RequestForListDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        requests.Add(new RequestForListDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetPrefix = dataReader.GetString("prefix_name"),
                            StreetName = dataReader.GetString("street_name"),
                            AddressType = dataReader.GetString("address_type"),
                            Flat = dataReader.GetString("flat"),
                            Building = dataReader.GetString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Entrance = dataReader.GetNullableString("entrance"),
                            Floor = dataReader.GetNullableString("floor"),
                            CreateTime = dataReader.GetDateTime("create_time"),
                            Description = dataReader.GetNullableString("description"),
                            ContactPhones = dataReader.GetNullableString("client_phones"),
                            ParentService = dataReader.GetNullableString("parent_name"),
                            Service = dataReader.GetNullableString("service_name"),
                            Master = dataReader.GetNullableInt("worker_id") != null ? new RequestUserDto
                            {
                                Id = dataReader.GetInt32("worker_id"),
                                SurName = dataReader.GetNullableString("sur_name"),
                                FirstName = dataReader.GetNullableString("first_name"),
                                PatrName = dataReader.GetNullableString("patr_name"),
                            } : null,
                            CreateUser = new RequestUserDto
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
                            StatusId = dataReader.GetInt32("req_status_id"),
                            Status = dataReader.GetNullableString("Req_Status"),
                        });
                    }
                    dataReader.Close();
                }
                return requests.ToArray();
            }
        }

        public string GetServiceCompanyInfo(int id)
        {
            var info = string.Empty;
            var sqlQuery = @"SELECT info from CallCenter.ServiceCompanies where id = @Id";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        info = dataReader.GetNullableString("info");
                    }
                }
                return info;
            };
        }

        public string GetCurrentChannel(string sipNumber)
        {
            var info = string.Empty;
            var sqlQuery = @"CALL CallCenter.GetChannelsToBridge(@Sip)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Sip", sipNumber);
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        info = dataReader.GetNullableString("Channel");
                    }
                }
                return info;
            };
        }

        public WorkerDto[] GetWorkersByWorkerId(int workerId)
        {
            var sqlQuery = @"CALL CallCenter.WebGetWorkers(@WorkerId)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public WorkerDto[] GetWorkersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate,int workerId)
        {
            var sqlQuery = @"CALL CallCenter.WebGetWorkersByPeriod(@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@WorkerId)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
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
        public WorkerDto[] GetExecutersByPeriod(bool filterByCreateDate, DateTime fromDate, DateTime toDate, DateTime executeFromDate, DateTime executeToDate,int workerId)
        {
            var sqlQuery = @"CALL CallCenter.WebGetExecutersByPeriod(@ByCreateDate,@FromDate,@ToDate,@ExecuteFromDate,@ExecuteToDate,@WorkerId)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@ByCreateDate", filterByCreateDate);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@ExecuteFromDate", executeFromDate);
                cmd.Parameters.AddWithValue("@ExecuteToDate", executeToDate);
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
        public StreetDto[] GetStreetsByWorkerId(int workerId)
        {
            var sqlQuery = "CALL CallCenter.WebGetStreets(@CurWorker)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
        public StreetDto GetStreetById(int streetId)
        {
            var sqlQuery = @"SELECT s.id,s.name,s.city_id,p.id as Prefix_id,p.Name as Prefix_Name,p.ShortName FROM CallCenter.Streets s
    join CallCenter.StreetPrefixes p on p.id = s.prefix_id
    where s.id = @StreetId and s.enabled = 1";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@StreetId", streetId);
                StreetDto street = null;
                using (var dataReader = cmd.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        street = new StreetDto
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
                        };
                    }
                    dataReader.Close();
                }
                return street;
            }
        }

        public WebHouseDto[] GetHousesByStreetAndWorkerId(int streetId, int workerId)
        {
            var sqlQuery = @"CALL CallCenter.WebGetHouses(@StreetId,@WorkerId)";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
                return houses.Select(h => new WebHouseDto { Id = h.Id, Name = h.FullName }).ToArray();
            }
        }
        public WebStatusDto[] GetWebStatuses()
        {

            var sqlQuery = @"SELECT id,name FROM CallCenter.WebState w order by id";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
            {
                var states = new List<WebStatusDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var status_id = dataReader.GetInt32("id");
                        states.Add(new WebStatusDto
                        {
                            Id = status_id,
                            Name = dataReader.GetString("name"),
                            //IsDefault = status_id == 2 ? true : false
                        });
                    }
                    dataReader.Close();
                }
                return states.ToArray();
            }
        }
        public WebCallsDto[] GetWebCallsByRequestId(int requestId)
        {

            var sqlQuery = @"SELECT rc.id,c.CallerIdNum,c.CreateTime,c.MonitorFile FROM CallCenter.Requests r
join CallCenter.RequestCalls rc on rc.request_id = r.id
join asterisk.ChannelHistory c on c.UniqueID = rc.uniqueID where r.id = @reqId order by c.CreateTime";
            using (var cmd = new MySqlCommand(sqlQuery, _dbConnection))
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
                            MonitorFile = dataReader.GetNullableString("MonitorFile"),
                        });
                    }
                    dataReader.Close();
                }
                return states.ToArray();
            }
        }
        public StatusDto[] GetStatusesAllowedInWeb()
        {
            var query = "SELECT id, name, Description FROM CallCenter.RequestState R where R.allow_in_web = 1 order by id";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
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

        public AppRequestDto[] GetRequestsByPhone(string phone, string code)
        {
            var query = "call CallCenter.GetRequestsByPhone(@Phone,@Code)";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Code", code);

                var result = new List<AppRequestDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new AppRequestDto
                        {
                            Id = dataReader.GetInt32("id"),
                            CreateTime = dataReader.GetDateTime("create_time"),
                            StreetName = dataReader.GetNullableString("street_name"),
                            Description = dataReader.GetNullableString("Description"),
                            State = dataReader.GetNullableString("state"),
                            PrimaryType = dataReader.GetNullableString("primary_type"),
                            ServiceType = dataReader.GetNullableString("service_type"),
                            Building = dataReader.GetNullableString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Flat = dataReader.GetNullableString("flat"),
                        });
                    }
                    dataReader.Close();
                }
                return result.ToArray();
            }
        }
        public AppAddressDto[] GetAddressesByPhone(string phone, string code)
        {
            var query = "call CallCenter.GetAddressesByPhone(@Phone,@Code)";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Code", code);

                var result = new List<AppAddressDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new AppAddressDto
                        {
                            Id = dataReader.GetInt32("id"),
                            StreetName = dataReader.GetNullableString("street_name"),
                            Building = dataReader.GetNullableString("building"),
                            Corpus = dataReader.GetNullableString("corps"),
                            Flat = dataReader.GetNullableString("flat"),
                        });
                    }
                    dataReader.Close();
                }
                return result.ToArray();
            }
        }
        public AppTypeDto[] GetTypesByPhone(string phone, string code)
        {
            var query = "call CallCenter.GetTypesByPhone(@Phone,@Code)";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Code", code);

                var result = new List<AppTypeDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result.Add(new AppTypeDto
                        {
                            Id = dataReader.GetInt32("id"),
                            ParrentId = dataReader.GetNullableInt("parrent_id"),
                            Name = dataReader.GetNullableString("name"),
                        });
                    }
                    dataReader.Close();
                }
                return result.ToArray();
            }
        }

        public void CreateRequestFromPhone(string phone, string code, int addressId, int typeId, string description)
        {
            var query = "call CallCenter.CreateRequestFromPhone(@Phone,@Code,@AddressId,@TypeId,@Desc)";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@AddressId", addressId);
                cmd.Parameters.AddWithValue("@TypeId", typeId);
                cmd.Parameters.AddWithValue("@Desc", description);
                cmd.ExecuteNonQuery();
            }

        }
        public string CreateRequestFromWeb(int workerId, string phone, string fio, int addressId, int typeId,int? masterId,int? executerId, string description)
        {
            var query = "call CallCenter.WebCreateRequest(@WorkerId,@Phone,@Fio,@AddressId,@TypeId,@MasterId,@ExecuterId,@Desc);";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@WorkerId", workerId);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Fio", fio);
                cmd.Parameters.AddWithValue("@AddressId", addressId);
                cmd.Parameters.AddWithValue("@TypeId", typeId);
                cmd.Parameters.AddWithValue("@MasterId", masterId);
                cmd.Parameters.AddWithValue("@ExecuterId", executerId);
                cmd.Parameters.AddWithValue("@Desc", description);
                using (var dataReader = cmd.ExecuteReader())
                {
                    dataReader.Read();
                    return dataReader.GetNullableString("requestId");
                }
            }
        }

        public StatInfoDto[] GetWorkerStat(int currentWorkerId, DateTime fromDate, DateTime toDate)
        {
        
                    var result = new List<StatInfoDto>();
            using (var cmd = new MySqlCommand("CALL CallCenter.WebGetWorkerStat(@fromDate,@toDate,@userId)", _dbConnection))
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate);
                cmd.Parameters.AddWithValue("@toDate", toDate);
                cmd.Parameters.AddWithValue("@userId", currentWorkerId);
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var surName = dataReader.GetNullableString("sur_name");
                        var firstName = dataReader.GetNullableString("first_name");
                        var patrName = dataReader.GetNullableString("patr_name");
                        var operType = dataReader.GetNullableString("oper_type");
                        var name = surName + " " + (string.IsNullOrEmpty(firstName)
                            ? ""
                            : firstName.Substring(0, 1)+".")
                            + " " + (string.IsNullOrEmpty(patrName)
                            ? ""
                            : patrName.Substring(0, 1) + ".")
                            + " " + (operType== "Altered" ? "Назначено": "Выполнено");

                        result.Add(new StatInfoDto
                        {
                            StatDate = dataReader.GetDateTime("rep_date"),
                            Name = name,
                            Count = dataReader.GetInt32("item_count")
                        });
                    }
                    dataReader.Close();
                }
            }
            return result.ToArray();
        }

        public List<TransferIntoDto> GetTransferList()
        {
            var query = "SELECT id,name,sip_number FROM CallCenter.TransferInfo T where enabled = 1 order by order_id,id;";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                var transferList = new List<TransferIntoDto>();
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        transferList.Add(new TransferIntoDto
                        {
                            Id = dataReader.GetInt32("id"),
                            Name = dataReader.GetString("name"),
                            SipNumber = dataReader.GetString("sip_number")
                        });
                    }
                    dataReader.Close();
                }
                return transferList;
            }
        }
        public void DeleteRequestRatingById(int itemId)
        {
            var query = "delete from CallCenter.RequestRating where id = @Id;";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", itemId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteCallFromNotAnsweredList(string callerId)
        {
            var query = "delete from asterisk.NotAnsweredQueue where CallerIDNum  = @CallerId;";
            using (var cmd = new MySqlCommand(query, _dbConnection))
            {
                cmd.Parameters.AddWithValue("@CallerId", callerId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}