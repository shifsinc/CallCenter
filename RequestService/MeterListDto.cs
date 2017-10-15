﻿using System;

namespace RequestServiceImpl
{
    public class MeterListDto
    {
        public int Id { get; set; }
        public string ServiceCompany { get; set; }
        public string StreetName { get; set; }
        public int StreetId { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public int HouseId { get; set; }
        public string Flat { get; set; }
        public int AddressId { get; set; }
        public DateTime Date { get; set; }
        public double Electro1 { get; set; }
        public double Electro2 { get; set; }
        public double ColdWater1 { get; set; }
        public double HotWater1 { get; set; }
        public double ColdWater2 { get; set; }
        public double HotWater2 { get; set; }
        public double Heating { get; set; }
        public string FullAddress
        {
           get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                  : $"{StreetName}, {Building}/{Corpus}, {Flat}";
            }
        }
    }
}