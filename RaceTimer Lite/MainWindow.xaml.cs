using RFIDReaderAPI.Models;
using RFIDReaderAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RaceTimer_Lite;
using System.Threading.Tasks.Dataflow;
using System.Security.Cryptography;

namespace RaceTimer_Lite
{
    public partial class MainWindow : Window 
    {
        Utilities myUtilites = new Utilities();
        readonly Program myProgram = new Program();
        public Starter[] StarterArray = new Starter[400];
        public static string[] DataPath = SetDirectory();

        static int StatusStartStop = 0;
        public int Evaluation = 0;

        static readonly string MessageView = "ListViewMessages";
        static readonly string StarterView = "ListViewStarter";
        public List<Message> messages { get; set; }
        public List<Message> LogList { get; set; }
        public List<Starter> starters { get; set; }
        

        static readonly string ConnIDReader1 = "192.168.1.116:9090";
        static readonly string ConnIDReader2 = "192.168.1.115:9090";
        static int ConnectionStatus = 0;
        private int Connection1 = 0;
        public int ConnectionStatus1;
        public int ConnectionStatus2;
        
        

        static int TimerTime = 6000;
        static System.Timers.Timer timer = new System.Timers.Timer(TimerTime);

        public int CurrentfirstNr;
        public int CurrentlastNr;
        public string? CurrentDataPath;
        public string? CurrentCategory;
        public string? CurrentGender;
        public MainWindow()
        {
            InitializeComponent();
            messages = new List<Message>();
            starters = new List<Starter>();


            StarterArray = myUtilites.DataInput(DataPath[4]);
            myProgram.setStarterArray(StarterArray);
            

            UpdateListView(StarterView);

            ConnectionStatus1 = 0;
            ConnectionStatus2 = 0;
            


        }

        private void UpdateListView(string Listview)
        {
            if(Listview == MessageView)
            {
                ListViewMessages.Items.Refresh();

                ListViewMessages.SelectedIndex = ListViewMessages.Items.Count - 1;
                ListViewMessages.ScrollIntoView(ListViewMessages.SelectedItem);
                DataContext = this;
            }
            if (Listview == StarterView)
            {
                ListViewStarter.Items.Refresh();

                ListViewStarter.SelectedIndex = ListViewStarter.Items.Count - 1;
                ListViewStarter.ScrollIntoView(ListViewStarter.SelectedItem);
                DataContext = this;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Message message = new Message();

            if (StatusStartStop == 0)
            {
                myProgram.SettingsReader(ConnIDReader1);
                myProgram.SettingsReader(ConnIDReader2);
                myProgram.StartRead(ConnIDReader1);
                myProgram.StartRead1(ConnIDReader2);
                messages.Add(new Message { Msg="Zeitmessung gestartet", Time=DateTime.Now.ToString("HH:mm:ss")});
                timer.Elapsed += Timer_Elapsed;
                timer.Start();

                ListViewStarter.Items.Refresh();

                UpdateListView(MessageView);

                StartButton.Content = "Stop";

                StatusStartStop = 1;
            }
            else
            {
                timer.Stop();

                messages.Add(new Message { Msg = "Zeitmessung gestoppt", Time = DateTime.Now.ToString("HH:mm:ss") });
                myProgram.StopRead(ConnIDReader1);
                myProgram.StopRead(ConnIDReader2);

                UpdateListView(MessageView);

                LogList = myProgram.getMessageList();

                StreamWriter streamLog = new StreamWriter(DataPath[0]);
                foreach (Message log in LogList )
                {
                    streamLog.WriteLine("{0} {1}", log.Time, log.Msg);
                }
                streamLog.Close();  

                StartButton.Content = "Start";

                StatusStartStop = 0;
            }
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if(myProgram.CheckConnection(ConnIDReader1) != 1)
            {
                
                ConnectionStatus1 = 0;
                myProgram.CloseCon(ConnIDReader1);
                myProgram.ConnectReader(ConnIDReader1);
                myProgram.StartRead(ConnIDReader1);
            } else if (myProgram.CheckConnection(ConnIDReader1) == 1)
            {
                ConnectionStatus1 = 1;
            }

            if (myProgram.CheckConnection(ConnIDReader2) != 1)
            {
                ConnectionStatus2 = 0;  
                myProgram.CloseCon(ConnIDReader2);
                myProgram.ConnectReader(ConnIDReader2);
                myProgram.StartRead(ConnIDReader2);
            }
            else if (myProgram.CheckConnection(ConnIDReader2) == 1)
            {
                ConnectionStatus2 = 1;
            }
        }

        private void BoxStartNr_KeyDown(object sender, KeyEventArgs e)
        {
            Message message = new Message();

            if (e.Key == Key.Enter)
            {
                int StartNr = 0;
                try
                {
                    StartNr = Convert.ToInt32(BoxStartNr.Text);
                } 
                catch (Exception ex)
                { 
                    message.Msg = ex.Message;
                    message.Time = DateTime.Now.ToString("HH:mm:ss");
                    messages.Add(message);
                }
                if (StartNr > 0 && StartNr < 351)
                {
                    Starter CurrentStarter = myProgram.getStarter(StartNr);
                    if( CurrentStarter != null ) 
                    {
                        if (StatusStartStop == 0)
                        {
                            message.Msg = "Zeitmessung inaktiv";
                            message.Time = DateTime.Now.ToString("HH:mm:ss");
                            messages.Add(message);
                        }
                        else
                        {
                            message.Time = DateTime.Now.ToString("HH:m:ss");
                            if (CurrentStarter.EndTimeManual != TimeSpan.Zero)
                            {
                                message.Msg = "Doppelte Eingabe bei Startnummereingabe";

                                messages.Add(message);
                            }
                            else
                            {
                                    CurrentStarter.EndTimeManual = DateTime.Now - CurrentStarter.StartTime;

                                    if (CurrentStarter.EndTime == TimeSpan.Zero)
                                    {
                                        CurrentStarter.EndTime = CurrentStarter.EndTimeManual;
                                        CurrentStarter.RunTime = DateTime.Now - CurrentStarter.StartTime - CurrentStarter.SwimTime - CurrentStarter.BikeTime;
                                    }
                                    message.Msg = CurrentStarter.FullName + " ist im Ziel";


                                starters.Add(CurrentStarter);

                                StarterArray[StartNr] = CurrentStarter;

                                messages.Add(message);
                            }
                            
                        }
                    }
                }

                UpdateListView("ListViewMessages");
                UpdateListView("ListViewStarter");

                BoxStartNr.Text = ""; 
            }
        }

        private void BtnClearMsgs_Click(object sender, RoutedEventArgs e)
        {
            messages.Clear();

            UpdateListView(MessageView);
        }


        public void AddToListViewStarter(Starter starter)
        {
            starters.Add(starter);
            UpdateListView(StarterView);

        }

        public void AddToListView(Message message)
        {
            messages.Add(message);
            UpdateListView(MessageView);
        }

        public Starter[] UpdateArrayMW()
        {
            return StarterArray;
        }

        static string[] SetDirectory()
        {

            string DirectoryUtilities = System.IO.Path.Combine(@"C:\RaceTimerLite", "Utilities");
            string FileLog = System.IO.Path.Combine(DirectoryUtilities, "Log.txt");
            string ReadLog = System.IO.Path.Combine(DirectoryUtilities, "ReadLog.csv");
            string DirectoryLists = System.IO.Path.Combine(DirectoryUtilities, "Lists");
            string StarterListFile = System.IO.Path.Combine(DirectoryLists, "StarterList.txt");
            string DirectoryResults = System.IO.Path.Combine(DirectoryUtilities, "Results");
            string ResultsSchuelerC = System.IO.Path.Combine(DirectoryResults, "SchuelerC.txt");
            string ResultsSchuelerB = System.IO.Path.Combine(DirectoryResults, "SchuelerB.txt");
            string ResultsSchuelerA = System.IO.Path.Combine(DirectoryResults, "SchuelerA_JugendB.txt");
            string ResultsVolkW = System.IO.Path.Combine(DirectoryResults, "VolkstriathlonW.txt");
            string ResultsVolkM = System.IO.Path.Combine(DirectoryResults, "VolkstriathlonM.txt");
            string ResultsVolk = System.IO.Path.Combine(DirectoryResults, "Volkstriathlon.txt");
            string ResultsStaffel = System.IO.Path.Combine(DirectoryResults, "Staffeltriathlon.txt");
            string ResultsAgeGroup = System.IO.Path.Combine(DirectoryResults, "Altersklassen.txt");

            string[] EventMsgs = new string[5];


            if (!Directory.Exists(DirectoryUtilities))
            {
                Directory.CreateDirectory(DirectoryUtilities);
                Directory.CreateDirectory(DirectoryLists);
                Directory.CreateDirectory(DirectoryResults);
                File.Create(FileLog);
                File.Create(ReadLog);
            }
            else
            {
                if (!Directory.Exists(DirectoryResults))
                {
                    Directory.CreateDirectory(DirectoryResults);
                }
                if (!Directory.Exists(DirectoryLists))
                {
                    Directory.CreateDirectory(DirectoryLists);
                }
                if (!File.Exists(FileLog))
                {
                    File.Create(FileLog);
                }
                if (!File.Exists(ReadLog))
                {
                    File.Create(ReadLog);
                }

            }


            string[] returnArray = new string[14];
            returnArray[0] = FileLog;
            returnArray[1] = DirectoryUtilities;
            returnArray[2] = DirectoryLists;
            returnArray[3] = DirectoryResults;
            returnArray[4] = StarterListFile;
            returnArray[5] = ResultsSchuelerC;
            returnArray[6] = ResultsSchuelerB;
            returnArray[7] = ResultsSchuelerA; 
            returnArray[8] = ResultsVolk;
            returnArray[9] = ResultsVolkM;
            returnArray[10] = ResultsVolkW;
            returnArray[11] = ResultsStaffel;
            returnArray[12] = ResultsAgeGroup;
            returnArray[13] = ReadLog;


            return returnArray;

        }

        private void BtnAuswertung_Click(object sender, RoutedEventArgs e)
        {
            switch (Evaluation)
            {
                case 1:
                    CurrentDataPath = DataPath[5];
                    CurrentCategory = "Schüler C    0,1 - 2,5 - 0,4";
                    CurrentfirstNr = 1;
                    CurrentlastNr = 19;
                    //DataOutput();
                    CurrentCategory = "Schüler C";
                    myUtilites.DataOutput(StarterArray, DataPath[5], 1, 19);
                    break;

                case 2:
                    CurrentDataPath = DataPath[6];
                    CurrentCategory = "Schüler B    0,2 - 5 - 1";
                    CurrentfirstNr = 20;
                    CurrentlastNr = 49;
                    //DataOutput();
                    CurrentCategory = "Scüler B";
                    myUtilites.DataOutput(StarterArray, DataPath[6], 20, 49);
                    break;

                case 3:
                    CurrentDataPath = DataPath[7];
                    CurrentCategory = "Schüler A/Jugend B    0,4 - 10 - 2,5";
                    CurrentfirstNr = 50;
                    CurrentlastNr = 99;
                    //DataOutput();
                    //DataOutputSchuelerA();
                    myUtilites.DataOutput(StarterArray, DataPath[7], 50, 99);
                    break;

                case 4:
                    CurrentDataPath = DataPath[8];
                    CurrentCategory = "Volkstriathlon Gesamt    0,5 - 20 - 5";
                    CurrentfirstNr = 100;
                    CurrentlastNr = 250;
                    myUtilites.DataOutput(StarterArray, DataPath[8], 100, 200);
                    CurrentDataPath = DataPath[9];
                    CurrentCategory = "Volkstriathlon Männer    0,5 - 20 - 5";
                    CurrentGender = "m";
                    myUtilites.DataOutputGender(StarterArray, DataPath[9], 100, 200, "m");
                    CurrentDataPath = DataPath[10];
                    CurrentCategory = "Volkstriathlon Frauen    0,5 - 20 - 5";
                    CurrentGender = "w";
                    myUtilites.DataOutputGender(StarterArray, DataPath[10], 100, 200, "w");
                    /*
                    CurrentDataPath = DataPath[12];
                    CurrentCategory = "Volkstriathlon Altersklassen    0,5 - 20 - 5";
                    DataOutputAgeGroup();
                    CurrentCategory = "Volkstriathlon";*/

                    break;

                case 5:
                    CurrentDataPath = DataPath[11];
                    CurrentCategory = "Staffeltriatlon    0,5 - 20 - 5";
                    CurrentfirstNr = 200;
                    CurrentlastNr = 250;
                    CurrentGender = "s";
                    myUtilites.DataOutputGender(StarterArray, DataPath[11], 200, 250, "s");
                    CurrentCategory = "Staffeltriathlon";
                    break;

                default:
                    break;
            }
            Evaluation = 0;
            Message message = new Message();
            message.Msg = "Wettkampf " + CurrentCategory + " ausgewertet";
            message.Time = DateTime.Now.ToString("HH:mm:ss");
            AddToListView(message);
        }

        private void BtnAuswertungSchulerC_Click(object sender, RoutedEventArgs e)
        {
            Evaluation = 1;
        }

        private void BtnAuswertungSchulerB_Click(object sender, RoutedEventArgs e)
        {
            Evaluation = 2;
        }

        private void BtnAuswertungSchulerA_Click(object sender, RoutedEventArgs e)
        {
            Evaluation = 3;
        }

        private void BtnAuswertungVolk_Click(object sender, RoutedEventArgs e)
        {
            Evaluation = 4;
        }

        private void BtnAuswertungStaffel_Click(object sender, RoutedEventArgs e)
        {
            Evaluation = 5;
            
        }

        public void DataOutput()
        {
            StreamWriter streamWriter = new StreamWriter(CurrentDataPath);
            Starter[] SortedStarters = SortedStarter(CurrentfirstNr, CurrentlastNr);
            int i = 1;

            streamWriter.WriteLine("Ergebnisliste " + CurrentCategory);
            streamWriter.Write("\n");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStarters)
            {   
                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
                if (i == 49)
                {
                    streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
                    streamWriter.WriteLine("Ergebnisse online unter lipperlandtriathlon.tglage.de");
                } else if (i == 101)
                {
                    streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
                    streamWriter.WriteLine("Ergebnisse online unter lipperlandtriathlon.tglage.de");
                }
            }
            if (SortedStarters.Length > 50)
            {
                int space = 50 - (SortedStarters.Length % 50) + 2;
                for (int y = 0; y < space; y++)
                {
                    streamWriter.WriteLine("");
                }
                streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
                streamWriter.WriteLine("Ergebnisse online unter lipperlandtriathlon.tglage.de");
            }
            if (SortedStarters.Length < 50)
            {
                int space = 48 - SortedStarters.Length;
                for(int j  = 0; j < space; j++)
                {
                    streamWriter.WriteLine("");
                }
                streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
                streamWriter.WriteLine("Ergebnisse online unter lipperlandtriathlon.tglage.de");
            } 
            streamWriter.Close();
            
        }

        public void DataOutputGender()
        {
            StreamWriter streamWriter = new StreamWriter(CurrentDataPath);
            Starter[] SortedStarters = SortedStartersGender(CurrentfirstNr, CurrentlastNr, CurrentGender);
            int i = 1;

            streamWriter.WriteLine("Ergebnisliste " + CurrentCategory);
            streamWriter.Write("\n");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStarters)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }
            streamWriter.Close();

        }
        public void DataOutputSchuelerA() 
        {
            StreamWriter streamWriter = new StreamWriter(CurrentDataPath);
            Starter[] SortedStarters = SortedStarter(CurrentfirstNr, CurrentlastNr);
            int i = 1;

            streamWriter.WriteLine("Ergebnisliste " + CurrentCategory);
            streamWriter.Write("\n");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStarters)
            {
                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            Starter[] SortedStartersSA = SortedStarterAgeGroupLite(CurrentfirstNr, CurrentlastNr, "Schüler A");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("Schüler A:");
            foreach (Starter Starter in SortedStartersSA)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("Jugend B:");
            i = 1;
            Starter[] SortedStartersJB = SortedStarterAgeGroupLite(CurrentfirstNr, CurrentlastNr, "Jugend B");
            foreach (Starter Starter in SortedStartersJB)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }


            streamWriter.Close();

        }

        public void DataOutputAgeGroup() 
        {
            StreamWriter streamWriter = new StreamWriter(CurrentDataPath);
            Starter[] SortedStartersA = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "Jugend A");
            int i = 1;

            streamWriter.WriteLine("Ergebnisliste " + CurrentCategory);
            streamWriter.Write("\n");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            streamWriter.Write('\n');
            streamWriter.WriteLine("Frauen:");
            streamWriter.WriteLine("-------");
            streamWriter.WriteLine("Jugend A:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStartersA)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersJ = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "Junioren");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("Junioren:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersJ)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW20 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK20");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK20:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW20)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW25 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK25");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK25:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW25)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW30 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK30");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK30:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW30)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW35 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK35");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK35:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW35)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW40 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK40");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK40:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW40)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW45 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK45");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK45:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW45)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW50 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK50");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK50:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW50)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
            }

            Starter[] SortedStartersW55 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK55");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK55:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW55)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW60 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK60");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK60:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW60)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW65 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK65");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK65:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW65)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW70 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK70");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK70:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW70)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersW75= SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "w", "AK75");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK75:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersW75)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            Starter[] SortedStartersMA = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "Jugend A");
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("Männer");
            streamWriter.WriteLine("-------");
            streamWriter.WriteLine("Jugend A:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");

            foreach (Starter Starter in SortedStartersMA)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersMJ = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "Junioren");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("Junioren:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersMJ)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM20 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK20");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK20:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM20)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM25 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK25");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK25:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM25)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM30 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK30");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK30:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM30)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM35 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK35");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK35:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM35)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM40 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK40");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK40:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM40)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM45 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK45");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK45:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM45)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM50 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK50");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK50:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM50)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM55 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK55");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK55:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM55)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM60 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK60");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK60:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM60)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM65 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK65");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK65:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM65)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM70 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK70");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK70:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM70)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }

            Starter[] SortedStartersM75 = SortedStarterAgeGroup(CurrentfirstNr, CurrentlastNr, "m", "AK75");
            i = 1;
            streamWriter.WriteLine("\n");
            streamWriter.WriteLine("AK75:");
            streamWriter.WriteLine("{0, -3} {1,-3} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15} {7,-15} {8,-15} {9,-15}", "Platz", "StartNr.", "Name", "AK", "m/w", "Verein", "Gesamt", "Swim", "Bike", "Run");
            streamWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (Starter Starter in SortedStartersM75)
            {

                streamWriter.WriteLine("{0,-5} {1,-8} {2,-25} {3,-10} {4,-5} {5,-33} {6,-15:@} {7,-15} {8,-15} {9,-15}", i + ".", Starter.StartNr, Starter.FullName, Starter.AgeGroup, Starter.Gender, Starter.Club, Convert.ToDateTime(Starter.EndTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.SwimTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.BikeTime.ToString()).ToString("HH:mm:ss.ff"), Convert.ToDateTime(Starter.RunTime.ToString()).ToString("HH:mm:ss.ff"));
                i++;
            }
            streamWriter.Close();
        }


        public Starter[] SortedStarter(int firstNr, int lastNr)
        {

            Starter[] SortedStarters = (from Starters in StarterArray
                                        where Starters != null && Starters.StartNr >= CurrentfirstNr && Starters.StartNr <= CurrentlastNr && Starters.EndTime != TimeSpan.Zero
                                        orderby Starters.EndTime
                                        select Starters).ToArray();

            return SortedStarters;
        }

        public Starter[] SortedStarterAgeGroup(int firstNr, int lastNr, string gender, string AgeGroup) 
        {
            Starter[] SortedStarters = (from Starter in StarterArray
                                        where Starter != null && Starter.AgeGroup == AgeGroup && Starter.Gender == gender && Starter.EndTime != TimeSpan.Zero
                                        orderby Starter.EndTime
                                        select Starter).ToArray();
            return SortedStarters;
        }

        public Starter[] SortedStarterAgeGroupLite(int firstNr, int lastNr, string AgeGroup)
        {
            Starter[] SortedStarters = (from Starter in StarterArray
                                        where Starter != null && Starter.AgeGroup == AgeGroup && Starter.EndTime != TimeSpan.Zero
                                        orderby Starter.EndTime
                                        select Starter).ToArray();
            return SortedStarters;
        }

        public Starter[] SortedStartersGender(int firstNr, int lastNr, string gender)
        {

            Starter[] SortedStarters = (from Starter in StarterArray
                                        where Starter != null && Starter.StartNr >= CurrentfirstNr && Starter.StartNr <= CurrentlastNr && Starter.Gender == CurrentGender && Starter.EndTime != TimeSpan.Zero
                                        orderby Starter.EndTime
                                        select Starter).ToArray();

            return SortedStarters;
        }

        private void CreateConn_Click(object sender, RoutedEventArgs e)
        {
            switch (ConnectionStatus)
            {
                case 0:
                    int i = myProgram.ConnectReader(ConnIDReader1);
                    int j = myProgram.ConnectReader(ConnIDReader2);

                    if (j == 1 && j == 1)
                    {
                        messages.Add(new Message { Msg = "Connected to Readers", Time = DateTime.Now.ToString("HH:mm:ss") });
                        ConnectionStatus = 1;
                        CreateConn.Content = "Close Conn";

                    }
                    else
                    {
                        messages.Add(new Message { Msg = "Connection to Readers failed", Time = DateTime.Now.ToString("HH:mm:ss") });
                        ConnectionStatus = 0;
                        CreateConn.Content = "Create Conn";
                    }
                    break;

                 case 1:
                    myProgram.StopRead(ConnIDReader1);
                    myProgram.StopRead(ConnIDReader2);
                    myProgram.CloseCon(ConnIDReader1);
                    myProgram.CloseCon(ConnIDReader2);
                    ConnectionStatus = 0;
                    CreateConn.Content = "Create Conn";

                    messages.Add(new Message { Msg = "Connection to Readers closed", Time = DateTime.Now.ToString("HH:mm:ss") });
                    break;
            }

            UpdateListView(MessageView);
        }

        private void CheckConn_Click(object sender, RoutedEventArgs e)
        {
            if (myProgram.CheckConnection(ConnIDReader1) == 1)
            {
                messages.Add(new Message { Msg = "Connection " + ConnIDReader1 + " i.O.", Time = DateTime.Now.ToString("HH:mm:ss") });
            } else
            {
                messages.Add(new Message { Msg = "Connection " + ConnIDReader1 + " n.i.O.", Time = DateTime.Now.ToString("HH:mm:ss") });
            }

            if (myProgram.CheckConnection(ConnIDReader2) == 1)
            {
                messages.Add(new Message { Msg = "Connection " + ConnIDReader2 + " i.O.", Time = DateTime.Now.ToString("HH:mm:ss") });
            }
            else
            {
                messages.Add(new Message { Msg = "Connection " + ConnIDReader2 + " n.i.O.", Time = DateTime.Now.ToString("HH:mm:ss") });
            }

            UpdateListView(MessageView);
        }

    }

}


