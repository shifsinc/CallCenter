﻿using System;

namespace WebApi.Models
{
    public class RequestForListDto
    {
        public int Id { get; set; }
        public bool HasAttachment { get; set; }
        public bool HasRecord { get; set; }
        public bool IsBadWork { get; set; }
        public bool IsImmediate { get; set; }
        public bool IsChargeable { get; set; }
        public bool IsRetry { get; set; }
        public string IsRetryText => IsRetry ? "да" : "нет";
        public int? FirstRecordId { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ClientName { get; set; }
        public DateTime CreateTime { get; set; }
        public string StreetPrefix { get; set; }
        public int? RegionId { get; set; }
        public string RegionName { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string Floor { get; set; }
        public string Entrance { get; set; }
        public string AddressType { get; set; }
        public UserDto Master { get; set; }
        public UserDto Executer { get; set; }
        public UserDto CreateUser { get; set; }
        public DateTime? ExecuteTime { get; set; }
        public DateTime? TermOfExecution { get; set; }
        public string ExecutePeriod { get; set; }
        public string Description { get; set; }
        public string ParentService { get; set; }
        public string Service { get; set; }
        public int? ParentServiceId { get; set; }
        public int? ServiceId { get; set; }
        public string ContactPhones { get; set; }
        public string MainFio { get; set; }
        public string Rating { get; set; }
        public string RatingDescription { get; set; }
        public bool BadWork { get; set; }
        public bool Garanty { get; set; }
        public int GarantyId { get; set; }
        public string GarantyTest => Garanty ? "Да" : "Нет";
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string SpendTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public DateTime? AlertTime { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public string LastNote { get; set; }
        public int? TaskId { get; set; }
        public DateTime? TaskStart { get; set; }
        public DateTime? TaskEnd { get; set; }
        public UserDto TaskWorker { get; set; }


        public string FullAddress
        {
            //get { return string.IsNullOrEmpty(Corpus)?$"{StreetPrefix} {StreetName}, {Building}, {AddressType} {Flat}"
            //        : $"{StreetPrefix} {StreetName}, {Building}/{Corpus}, {AddressType} {Flat}"; }
            get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                  : $"{StreetName}, {Building} к.{Corpus}, {Flat}";
            }
        }

    }
}