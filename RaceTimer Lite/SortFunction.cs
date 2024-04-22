using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RaceTimer_Lite;

namespace RaceTimer_Lite
{
    /*public class SortFunction
    {
        public Starter[] SortedStarter(Starter[] startersArray, int firstNr, int lastNr) 
        {
            string[] strings = new string[150];
            int j = 0;
            Starter[] StarterArray = startersArray; 
            StreamWriter streamWriter = new StreamWriter(@"C:\RaceTimerLite\Random.txt");
            for (int i = firstNr; i <= lastNr; i++) 
            {
                if (StarterArray[i] != null)
                {
                    var starter = StarterArray[i];
                    streamWriter.WriteLine(starter.FullName);
                    strings[j] = starter.FullName; j++;
                }
            }

            streamWriter.Close();

            Starter[] SortedStarters = (from Starter starter in StarterArray
                                        let v = starter.StartNr <= lastNr
                                        where starter.StartNr >= firstNr && v
                                        orderby starter.EndTime
                                        select starter).ToArray();

            return SortedStarters;
        }

        public Starter[] SortedStartersGender(Starter[] startersArray, int firstNr, int lastNr, string gender)
        {
            Starter[] StarterArray = startersArray;
            Starter[] SortedStarters = (from Starter in StarterArray
                                        where Starter.StartNr >= firstNr && Starter.StartNr <= lastNr && Starter.Gender == gender
                                        orderby Starter.EndTime
                                        select Starter).ToArray();

            return SortedStarters;
        }

    }*/
}


