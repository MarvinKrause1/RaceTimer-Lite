using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceTimer_Lite;

namespace RaceTimer_Lite
{
    public class Utilities
    {
        /*public void DataOutput(Starter[] StarterArray, string DataPath, int firstNr, int lastNr, string category)
        {
            StreamWriter streamWriter = new StreamWriter(DataPath);
            Starter[] SortedStarters = sortFunction.SortedStarter(StarterArray, firstNr, lastNr);
            int i = 1;
            string timeFormat = "HH:mm:ss.ff";

            streamWriter.WriteLine("Ergebnisliste " + category);
            streamWriter.Write("");
            streamWriter.WriteLine("{0, -6} {1,-8} {2,-30} {3,-5} {4,-5} {5,-30} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStarters)
            {
                streamWriter.WriteLine("{0,-6} {1,-8} {2,-30} {3,-5} {4,-5} {5,-30} {6,-15} {7,-15} {8,-15} {9,-15}", i, Starter.StartNr, Starter.FullName, Starter.AgeGroup,
                    Starter.Gender, Starter.Club, Starter.EndTime.ToString(timeFormat), Starter.SwimTime.ToString(timeFormat), Starter.BikeTime.ToString(timeFormat), Starter.RunTime.ToString(timeFormat));
                i++;
            }
            streamWriter.Close();

        }

        public void DataOutputGender(Starter[] StarterArray, string DataPath, int firstNr, int lastNr, string category, string gender)
        {
            Starter[] starterArray = StarterArray;
            SortFunction sortFunction = new SortFunction();
            StreamWriter streamWriter = new StreamWriter(DataPath);
            Starter[] SortedStarters = sortFunction.SortedStartersGender(starterArray, firstNr, lastNr, gender);
            int i = 1;
            string timeFormat = "HH:mm:ss.ff";

            streamWriter.WriteLine("Ergebnisliste " + category);
            streamWriter.Write("");
            streamWriter.WriteLine("{0, -6} {1,-8} {2,-30} {3,-5} {4,-5} {5,-30} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStarters)
            {
                streamWriter.WriteLine("{0,-6} {1,-8} {2,-30} {3,-5} {4,-5} {5,-30} {6,-15} {7,-15} {8,-15} {9,-15}", i, Starter.StartNr, Starter.FullName, Starter.AgeGroup,
                    Starter.Gender, Starter.Club, Starter.EndTime.ToString(timeFormat), Starter.SwimTime.ToString(timeFormat), Starter.BikeTime.ToString(timeFormat), Starter.RunTime.ToString(timeFormat));
                i++;
            }
            streamWriter.Close();

        }*/

        public Starter[] DataInput(string DataPath)
        {
            Starter[] StarterArray = new Starter[400];
            DateTime StartTimeC = new DateTime(2023,6,18,9,0,0);
            DateTime StartTimeB = new DateTime(2023, 6, 18, 9, 15, 0);
            DateTime StartTimeA = new DateTime(2023, 6, 18, 9, 30, 0);
            DateTime StartTime1 = new DateTime(2023, 6, 18, 10, 10, 0);
            DateTime StartTime2 = new DateTime(2023, 6, 18, 10, 30, 0);
            DateTime StartTime3 = new DateTime(2023, 6, 18, 10, 50, 0);


            string[] lines = File.ReadAllLines(DataPath).ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                Starter CurrentStarter = new Starter();
                string[] Eintraege = lines[i].Split(";");
                CurrentStarter.StartNr = Int32.Parse(Eintraege[0]);
                CurrentStarter.LastName = Eintraege[1];
                CurrentStarter.FirstName = Eintraege[2];
                CurrentStarter.FullName = CurrentStarter.FirstName + " " + CurrentStarter.LastName;
                CurrentStarter.AgeGroup = AgeToAgeGroup(Eintraege[3]);
                CurrentStarter.Gender = Eintraege[4];
                CurrentStarter.Club = Eintraege[5];
                CurrentStarter.EndTime = TimeSpan.Zero;
                CurrentStarter.EndTimeManual = TimeSpan.Zero;
                CurrentStarter.SwimTime = TimeSpan.Zero;
                CurrentStarter.BikeTime = TimeSpan.Zero;
                CurrentStarter.RunTime = TimeSpan.Zero;
                CurrentStarter.LastRead = 0;

                if (CurrentStarter.Gender == "s")
                {
                    CurrentStarter.SecondStarter = Eintraege[6] + " " + Eintraege[7];
                    CurrentStarter.ThirdStarter = Eintraege[8] + " " + Eintraege[9];
                }

                if(CurrentStarter.StartNr >= 1 && CurrentStarter.StartNr <= 19)
                {
                    CurrentStarter.StartTime = StartTimeC;
                } else if (CurrentStarter.StartNr >= 20 && CurrentStarter.StartNr <= 49)
                {
                    CurrentStarter.StartTime = StartTimeB;
                } else if (CurrentStarter.StartNr >= 50 && CurrentStarter.StartNr <= 99)
                {
                    CurrentStarter.StartTime = StartTimeA;
                } else if (CurrentStarter.StartNr >= 100 && CurrentStarter.StartNr <= 149)
                {
                    CurrentStarter.StartTime = StartTime1;
                } else if (CurrentStarter.StartNr >= 150 && CurrentStarter.StartNr <= 199)
                {
                    CurrentStarter.StartTime = StartTime2;
                } else if (CurrentStarter.StartNr >= 200 && CurrentStarter.StartNr <= 250)
                {
                    CurrentStarter.StartTime = StartTime3;
                }
                 
                StarterArray[CurrentStarter.StartNr] = CurrentStarter;
            }

            return StarterArray;
        }

        public string AgeToAgeGroup(string Age) 
        {
            int iBornYear = 0, iAge, iYear;
            string returnAge = "";
            try
            {
                iBornYear = int.Parse(Age);
            }
            catch (Exception ex){ }
            iYear = int.Parse(DateTime.Now.ToString("yyyy"));
            iAge = iYear - iBornYear;

            if (iAge > 0)
            {
                if (iAge < 20)
                {
                    if(iAge <= 9) {returnAge = "Schüler C";}
                    if(iAge > 9 && iAge <= 11) { returnAge = "Schüler B";}
                    if(iAge > 11 && iAge <= 13) { returnAge = "Schüler A"; }
                    if(iAge > 13 && iAge <= 15) { returnAge = "Jugend B"; }
                    if(iAge > 15 && iAge <= 17) { returnAge = "Jugend A"; }
                    if(iAge > 17 && iAge <= 19) { returnAge = "Junioren"; }
                } 

                if (iAge >= 20)
                {
                    if (iAge < 25) { returnAge = "AK20"; }
                    if (iAge >= 25 && iAge < 30) { returnAge = "AK25"; }
                    if (iAge >= 30 && iAge < 35) { returnAge = "AK30"; }
                    if (iAge >= 35 && iAge < 40) { returnAge = "AK35"; }
                    if (iAge >= 40 && iAge < 45) { returnAge = "AK40"; }
                    if (iAge >= 45 && iAge < 50) { returnAge = "AK45"; }
                    if (iAge >= 50 && iAge < 55) { returnAge = "AK50"; }
                    if (iAge >= 55 && iAge < 60) { returnAge = "AK55"; }
                    if (iAge >= 60 && iAge < 65) { returnAge = "AK60"; }
                    if (iAge >= 65 && iAge < 70) { returnAge = "AK65"; }
                    if (iAge >= 70 && iAge < 75) { returnAge = "AK70"; }
                    if (iAge >= 75 && iAge < 80) { returnAge = "AK75"; }
                    if (iAge >= 80 && iAge < 85) { returnAge = "AK80"; }
                    if (iAge >= 85 && iAge < 90) { returnAge = "AK85"; }
                    if (iAge >= 90 && iAge < 95) { returnAge = "AK90"; }
                    if (iAge >= 95 && iAge < 100) { returnAge = "AK95"; }
                }
            }
            


            return returnAge; 
        }
    }
}
