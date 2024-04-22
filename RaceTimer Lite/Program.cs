using RFIDReaderAPI.Models;
using RFIDReaderAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Security.Cryptography.Xml;
using RaceTimer_Lite;

namespace RaceTimer_Lite
{
    class Program : RFIDReaderAPI.Interface.IAsynchronousMessage
    {
        readonly Utilities myUtilities = new Utilities();
        static Message message = new Message();
        static int TimerTime = 6000;
        static Starter[] StarterArray = new Starter[400];
        public Starter CurrentStarter = new Starter();
        public List<Starter> starters = new List<Starter>();
        static List<Message> messages = new List<Message>();
        static readonly string ConnIDReader1 = "192.168.1.116:9090";
        static readonly string ConnIDReader2 = "192.168.1.115:9090";

        #region Set parameters


        //static string[,] StarterString = new string[300, 15];
        #endregion




        /*static void Main(string[] args)
        {
            
        }*/

        public void setStarterArray(Starter[] starters)
        {
            StarterArray = starters;
        }

        public void setStarter(Starter starter)
        {
            StarterArray[0] = starter;
        }

        public Starter[] getStarterArray()
        {
            return StarterArray;
        }

        public Starter getStarter(int StartNr)
        {
            return StarterArray[StartNr];
        }

        public List<Starter> getStarterList()
        {
            return starters;
        }


        public void CloseCon(string ID)
        {
            RFIDReader.CloseConn(ID);
        }

        public void StartRead(string ID)
        {
            RFIDReader._Tag6C.GetEPC(ID, eAntennaNo._1 | eAntennaNo._2 | eAntennaNo._3 | eAntennaNo._4, eReadType.Inventory);
        }

        public void StartRead1(string ID)
        {
            RFIDReader._Tag6C.GetEPC(ID, eAntennaNo._1 , eReadType.Inventory);
        }

        public void StopRead(string ID)
        {
            RFIDReader._RFIDConfig.Stop(ID);
        }

        public int ConnectReader(string ID)
        {
            RFIDReaderAPI.Interface.IAsynchronousMessage log = new Program();
            bool Decission = RFIDReader.CreateTcpConn(ID, log);
            int iReturn;

            if (Decission == true)
            {
                iReturn = 1;
            }
            else
            {
                iReturn = 0;
            }
            return iReturn;
        }

        public void SettingsReader(string ID)
        {
            message.Time = DateTime.Now.ToString("HH:mm:ss");

            //i1 = RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 100, 0);
            //i1 = RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 100, 20);
            //i1 = RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 100, 40);
            //i1 = RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 100, 50);
            //i1 = RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 100, 0);
            string stu = "SetTagUpdateParam: " + RFIDReader._RFIDConfig.SetTagUpdateParam(ID, 75, 0);



            //i2 = RFIDReader._RFIDConfig.SetReaderANT(ID, eAntennaNo._1 | eAntennaNo._2);
            string sra = "SetReaderANT: " + RFIDReader._RFIDConfig.SetReaderANT(ID, eAntennaNo._1 | eAntennaNo._2 | eAntennaNo._3| eAntennaNo._4);



            //i3 = RFIDReader._RFIDConfig.SetANTPowerParam(ID, new Dictionary<int, int>{ { 1, 5 }, { 2, 5 } });
            string sap = "SetANTPower: " + RFIDReader._RFIDConfig.SetANTPowerParam(ID, new Dictionary<int, int> { { 1, 30 }, { 2, 30 }, { 3, 30 }, { 4, 30 } });


            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.ETSI_866_to_868MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.FCC_902_to_928MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.GB_840_to_845MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.GB_920_to_925MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.GB_920_to_925MHz_and_GB_840_to_845MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.Korea_917_1_to_923_3MHz);
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.RUS_866_to_867MHz);

            string srr = "SetReaderRF: " + RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.ETSI_866_to_868MHz);

            //Nicht flashbar
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.LKA_920_to_924MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.MY_919_to_923MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.ID_923_to_925MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.BRA_902_to_907MHz_and_BRA_915_to928MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.GBT_920_to_925MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.JP_916_to_921MHz);//Fehler
            //i4 = RFIDReader._RFIDConfig.SetReaderRF(ID, eRF_Range.TW_922_to_927MHz);//Fehler

            string bbs = "BaseBandSettings: " + RFIDReader._RFIDConfig.SetEPCBaseBandParam(ID, 255, 0, 1, 2);
        }



        public int CheckConnection(string ID)
        {
            int iReturn = 0;

            try
            {
                if (RFIDReader.CheckConnect(ID))
                {
                    return iReturn = 1;
                }
            }


            catch (Exception e)
            {

            }

            return iReturn;
        }


        #region UseLess
        //Diese Funktionen werden von den Readern gefordert, ansonsten funktioniert das Programm nicht
        //In diesem Programm übernehmen diese Funktionen allerdings keine Aufgaben
        public void WriteDebugMsg(string msg)
        {
        }
        public void WriteLog(string msg)
        {
        }
        public void PortConnecting(string connID)
        {
        }
        public void PortClosing(string connID)
        {
        }
        public void OutPutTagsOver()
        {
        }
        public void GPIControlMsg(GPI_Model gpiModel)
        {
        }

        public void EventUpload(CallBackEnum type, object param)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void OutPutTags(Tag_Model tag)
        {
            int StarterNr = 0;
            try
            {
                StarterNr = Convert.ToInt32(tag.EPC);
            }
            catch (Exception ex)
            {
                ex.ToString();

            }
            if (StarterNr < 350)
            {
                messages.Add(new Message { Msg = tag.ReaderName.ToString(), Time = DateTime.Now.ToString("HH:mm:ss") });
                CurrentStarter = StarterArray[StarterNr];
                if (CurrentStarter != null && CurrentStarter.StartTime < DateTime.Now)
                {
                    if (tag.ReaderName == ConnIDReader1)
                    {
                        switch (CurrentStarter.LastRead)
                        {
                            case 0:
                                CurrentStarter.LastRead = 1;
                                CurrentStarter.SwimTime = DateTime.Now - CurrentStarter.StartTime;
                                message.Msg = "Last Read 0 " + CurrentStarter.SwimTime.ToString();
                                message.Time = DateTime.Now.ToString();
                                StarterArray[StarterNr] = CurrentStarter;
                                break;

                            case 1:
                                CurrentStarter.LastRead = 2;
                                CurrentStarter.BikeTime = DateTime.Now - CurrentStarter.StartTime - CurrentStarter.SwimTime;
                                message.Msg = "Last Read 1 " + CurrentStarter.BikeTime.ToString();
                                message.Time = DateTime.Now.ToString();
                                StarterArray[StarterNr] = CurrentStarter;
                                break;

                            case 2:
                                break;
                        }
                    }


                    if (tag.ReaderName == ConnIDReader2 && CurrentStarter.LastRead <= 2)
                    {
                        CurrentStarter.LastRead = 5;
                        CurrentStarter.EndTime = DateTime.Now - CurrentStarter.StartTime;
                        CurrentStarter.RunTime = DateTime.Now - CurrentStarter.StartTime - CurrentStarter.SwimTime - CurrentStarter.BikeTime;
                        message.Msg = "Last Read 2 " + CurrentStarter.RunTime.ToString();
                        message.Time = DateTime.Now.ToString();
                        StarterArray[StarterNr] = CurrentStarter;
                    }
                    messages.Add(new Message { Time = DateTime.Now.ToString(), Msg = tag.ReaderName});
                }
            }
        }
        
        public List<Message> getMessageList()
        {
            return messages;
        }

    }
}



