﻿using System;

namespace RequestServiceImpl
{
    public class MetersDto
    {
        public int Id { get; set; }
        public string PersonalAccount { get; set; }
        public DateTime Date { get; set; }
        public double Electro1 { get; set; }
        public double Electro2 { get; set; }
        public double ColdWater1 { get; set; }
        public double HotWater1 { get; set; }
        public double ColdWater2 { get; set; }
        public double HotWater2 { get; set; }
        public double Heating { get; set; }
        public double? Heating2 { get; set; }
        public double? Heating3 { get; set; }
    }
}