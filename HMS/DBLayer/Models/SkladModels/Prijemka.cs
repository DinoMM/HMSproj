﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Prijemka : PohSkup
    {
        public string? Objednavka { get; set; } = default!;
        public string? DodaciID { get; set; } = default!;
        public string? FakturaID { get; set; } = default!;
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }
        public DateTime Obdobie { get; set; } = SkladObdobie.GetSeassonFromToday();

        [ForeignKey("DruhPohybuX")]
        public string? DruhPohybu { get; set; } = default!;
        public DruhPohybu? DruhPohybuX { get; set; }


        public override string GetDisplayName()
        {
            return "Príjemka";
        }
    }
    
}