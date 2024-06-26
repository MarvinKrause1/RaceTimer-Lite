﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RaceTimer_Lite
{
    public class Starter
    {
        public int StartNr { get; set; }
        public string ?LastName { get; set; }
        public string ?FirstName { get; set; }
        public string? FullName { get; set; } 
        public string ?Club { get; set; }
        public string ?Gender { get; set; }
        public DateTime StartTime { get; set; }
        public string ?AgeGroup { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan EndTimeManual { get; set; }
        public TimeSpan SwimTime { get; set; }
        public TimeSpan BikeTime { get; set; } 
        public TimeSpan RunTime { get; set; }
        //Gibt über Zahlenwert wieder an welchem Reader der Starter zulätzt einglesen wurde
        public int LastRead { get; set; }
        public string ?SecondStarter { get; set; }
        public string ?ThirdStarter { get; set;}


    }
}
