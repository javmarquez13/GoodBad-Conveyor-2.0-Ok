using System;
using System.Data;
using System.Deployment.Application;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GoodBad_Conveyor_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    /// 
    /// DEVELOPED BY JAVIER MARQUEZ
    /// GOOD BAD CONVEYOR  2.0
    /// 
    /// 
    /// 
    /// 1.0.0.51
    /// FIRST RELEASE TO PRD 
    /// 22 SEP 2022
    /// 
    ///
    /// 1.0.0.62
    /// Add FVT / PBTS  STATUS FAIL AS A GOOD BOARD
    /// 12 OCTOBER 2022
    /// 
    /// 
    /// 1.0.0.63
    /// 1.0.0.64
    /// 1.0.0.65
    /// FIX A MISTYPED LABEL ON GUI
    /// 12 OCTOBER 2022
    /// 
    ///
    /// 1.0.0.66
    /// Added variable CHECK_PREVIOUS to improve the logic on GOOD BAD CONVEYOR FOR NEPTUNE C+
    /// 28 OCTOBER 2022
    /// 
    /// 
    /// 1.0.0.67
    /// 1.0.0.68
    /// 1.0.0.69
    /// 1.0.0.70
    /// 1.0.0.71
    /// 1.0.0.72
    /// 1.0.0.73
    /// 1.0.0.74
    /// 1.0.0.75
    /// Debug new functionality for Shutter
    /// 1.0.0.76
    /// Added functionality for Shutter and Release to production
    /// 31 JAN 2023
    /// 
    public partial class MainWindow : Window
    {
        SerialPort _Keyence1 = new SerialPort();
        SerialPort _Keyence2 = new SerialPort();
        SerialPort _SingleKeyence = new SerialPort();
        DispatcherTimer _TimerDAQ = new DispatcherTimer();
        NI Ni = new NI();

        bool Busy_1 = false;
        bool Busy_2 = false;
        bool CheckPointLane1_OK = false;
        bool CheckPointLane2_OK = false;

        bool[] DAQDefault =
                        {
                           false,
                           false,
                           false,
                           false,
                           false,
                           false,
                           false,
                           false
                        };

        bool[] DAQPassThrue =
                  {
                           false,
                           true,
                           true,
                           false,
                           false,
                           true,
                           false,
                           false
                        };

        bool[] outLane1_OK =
                        {
                           false,
                           true,
                           true,
                           false,
                           false,
                           false,
                           false,
                           false
                        };

        bool[] outLane1_NG =
                        {
                           true,
                           true,
                           false,
                           false,
                           false,
                           false,
                           false,
                           false
                        };

        bool[] outLane2_OK =
                        {
                           false,
                           true,
                           false,
                           false,
                           false,
                           true,
                           false,
                           false
                        };


        bool[] ShuttlerOutLane2_OK =
                        {
                           false,
                           true,
                           false,
                           false,
                           true,
                           false,
                           false,
                           false
                        };



        bool[] outLane2_NG =
                        {
                           false,
                           true,
                           false,
                           true,
                           false,
                           false,
                           false,
                           false
                        };

        public struct DataLane1
        {
            public DateTime DATE { set; get; }
            public string SERIAL_NUMBER { set; get; }
            public string PROCESS { set; get; }
            public string CRD { set; get; }
            public string DEFECT { set; get; }
            public string STATUS { set; get; }
        }

        public struct DataLane2
        {
            public DateTime DATE { set; get; }
            public string SERIAL_NUMBER { set; get; }
            public string PROCESS { set; get; }
            public string CRD { set; get; }
            public string DEFECT { set; get; }
            public string STATUS { set; get; }
        }

        public MainWindow()
        {
            InitializeComponent();

            btnOnOff_Lane1.Visibility = Visibility.Hidden;
            btnOnOff_Lane2.Visibility = Visibility.Hidden;
            btnDebug.Visibility = Visibility.Hidden;
            btnPassThru.Visibility = Visibility.Hidden;
            DockMenu.Width = 0;
            InitializeDgv();
            GetVersion();

            LogEvents.RegisterEvent(13, "Initializing application GOOD BAD CONVEYOR");

            Ni.WriteDAQ(DAQDefault);

            _TimerDAQ.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _TimerDAQ.Tick += _TimerReadDAQ;
            _TimerDAQ.Start();

            InitLanes();
        }

        void GetVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version myVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                lblVersion.Content = "PRD v" + myVersion;
            }
            else lblVersion.Content = "UAT v" + "1.0.0.0";
        }

        void InitializeDgv()
        {
            DataGridTextColumn DATE = new DataGridTextColumn();
            DATE.Header = "DATE";
            DATE.Binding = new Binding("DATE");
            DATE.Width = 150;
            DATE.IsReadOnly = true;

            DataGridTextColumn SERIAL_NUMBER = new DataGridTextColumn();
            SERIAL_NUMBER.Header = "SERIAL NUMBER";
            SERIAL_NUMBER.Binding = new Binding("SERIAL_NUMBER");
            SERIAL_NUMBER.Width = 130;
            SERIAL_NUMBER.IsReadOnly = true;

            DataGridTextColumn PROCESS = new DataGridTextColumn();
            PROCESS.Header = "PROCESS";
            PROCESS.Binding = new Binding("PROCESS");
            PROCESS.Width = 680;
            PROCESS.IsReadOnly = true;

            DataGridTextColumn STATUS = new DataGridTextColumn();
            STATUS.Header = "STATUS";
            STATUS.Binding = new Binding("STATUS");
            STATUS.Width = 100;
            STATUS.IsReadOnly = true;


            DgLane1.Columns.Add(DATE);
            DgLane1.Columns.Add(SERIAL_NUMBER);
            DgLane1.Columns.Add(PROCESS);
            DgLane1.Columns.Add(STATUS);



            DataGridTextColumn DATE2 = new DataGridTextColumn();
            DATE2.Header = "DATE";
            DATE2.Binding = new Binding("DATE");
            DATE2.Width = 150;
            DATE2.IsReadOnly = true;

            DataGridTextColumn SERIAL_NUMBER2 = new DataGridTextColumn();
            SERIAL_NUMBER2.Header = "SERIAL NUMBER";
            SERIAL_NUMBER2.Binding = new Binding("SERIAL_NUMBER");
            SERIAL_NUMBER2.Width = 130;
            SERIAL_NUMBER2.IsReadOnly = true;

            DataGridTextColumn PROCESS2 = new DataGridTextColumn();
            PROCESS2.Header = "PROCESS";
            PROCESS2.Binding = new Binding("PROCESS");
            PROCESS2.Width = 300;
            PROCESS2.IsReadOnly = true;

            DataGridTextColumn STATUS2 = new DataGridTextColumn();
            STATUS2.Header = "STATUS";
            STATUS2.Binding = new Binding("STATUS");
            STATUS2.Width = 100;
            STATUS2.IsReadOnly = true;


            DgLane2.Columns.Add(DATE2);
            DgLane2.Columns.Add(SERIAL_NUMBER2);
            DgLane2.Columns.Add(PROCESS2);
            DgLane2.Columns.Add(STATUS2);
        }

        void InitLanes() 
        {
            if (Globals.IS_ACTIVE_LANE_1) 
            {
                lblLane1Disable.Visibility = Visibility.Hidden;
                lblLane1Disable.Width = 0;

                if(Globals.IS_SHUTTLE) OnScanner();
                if(!Globals.IS_SHUTTLE) OnScanners(); 
            }
                
            if (!Globals.IS_ACTIVE_LANE_1) 
            {
                lblLane1Disable.Visibility = Visibility.Visible;
                lblLane1Disable.Width = 1270;

                if (Globals.IS_SHUTTLE) OffScanner();
                if (!Globals.IS_SHUTTLE) OffScanners();
            }

            if (Globals.IS_ACTIVE_LANE_2) 
            {
                lblLane2Disable.Visibility = Visibility.Hidden;
                lblLane2Disable.Width = 0;

                if (Globals.IS_SHUTTLE) OnScanner();
                if (!Globals.IS_SHUTTLE) OnScanners();
            }
               
            if (!Globals.IS_ACTIVE_LANE_2) 
            {
                lblLane2Disable.Visibility = Visibility.Visible;
                lblLane2Disable.Width = 1270;
                
                if (Globals.IS_SHUTTLE) OffScanner();
                if (!Globals.IS_SHUTTLE) OffScanners();
            }               
        }

        void OnScanners() 
        {
            if (Globals.IS_ACTIVE_LANE_1) 
            {
                try
                {
                    if (_Keyence1.IsOpen) return;
                    _Keyence1.PortName = Globals.COM_SCANNER1;
                    _Keyence1.BaudRate = Globals.BAUD_RATE;
                    _Keyence1.DataBits = Globals.DATA_BITS;
                    _Keyence1.Parity = Parity.None;
                    _Keyence1.Handshake = Handshake.None;
                    _Keyence1.DataReceived += _Keyence1_DataReceived;
                    _Keyence1.ErrorReceived += _Keyence1_ErrorReceived;
                    _Keyence1.Open();
                }
                catch (Exception ex)
                {
                    WriteDgv(1, DateTime.Now, "SCANNER 1", "ERROR: " + ex.Message, "FAIL");
                    LogEvents.RegisterEvent(1, "OnScanners: " + ex.Message);
                }
            }

            if (Globals.IS_ACTIVE_LANE_2)
            {
                try
                {
                    if (_Keyence2.IsOpen) return;
                    _Keyence2.PortName = Globals.COM_SCANNER2;
                    _Keyence2.BaudRate = Globals.BAUD_RATE;
                    _Keyence2.DataBits = Globals.DATA_BITS;
                    _Keyence2.Parity = Parity.None;
                    _Keyence2.Handshake = Handshake.None;
                    _Keyence2.DataReceived += _Keyence2_DataReceived;
                    _Keyence2.Open();
                }
                catch (Exception ex)
                {
                    WriteDgv(2, DateTime.Now, "SCANNER 2", "ERROR: " + ex.Message, "FAIL");
                    LogEvents.RegisterEvent(2, "OnScanners: " + ex.Message);
                }
            }
        }

        void OnScanner()
        {
                try
                {
                    if (_SingleKeyence.IsOpen) return;
                    _SingleKeyence.PortName = Globals.SINGLE_SCANNER;
                    _SingleKeyence.BaudRate = Globals.BAUD_RATE;
                    _SingleKeyence.DataBits = Globals.DATA_BITS;
                    _SingleKeyence.Parity = Parity.None;
                    _SingleKeyence.Handshake = Handshake.None;
                    _SingleKeyence.DataReceived += _SingleKeyence_DataReceived;
                    _SingleKeyence.ErrorReceived += _SingleKeyence_ErrorReceived;
                    _SingleKeyence.Open();
                }
                catch (Exception ex)
                {
                    WriteDgv(1, DateTime.Now, "SINGLE_SCANNER ", "ERROR: " + ex.Message, "FAIL");
                    WriteDgv(2, DateTime.Now, "SINGLE_SCANNER ", "ERROR: " + ex.Message, "FAIL");
                    LogEvents.RegisterEvent(1, "OnScanner: " + ex.Message);
                    LogEvents.RegisterEvent(2, "OnScanner: " + ex.Message);
                }                     
        }

        void OffScanners()
        {
            if (!Globals.IS_ACTIVE_LANE_1)
            {
                try
                {
                    if (_Keyence1.IsOpen) _Keyence1.Close();
                }
                catch (Exception ex)
                {
                    WriteDgv(1, DateTime.Now, "SCANNER 1", "ERROR: " + ex.Message, "FAIL");
                    LogEvents.RegisterEvent(1, "OffScanners: " + ex.Message);
                }
            }

            if (!Globals.IS_ACTIVE_LANE_2)
            {
                try
                {
                    if (_Keyence2.IsOpen) _Keyence2.Close();

                }
                catch (Exception ex)
                {
                    WriteDgv(2, DateTime.Now, "SCANNER 2", "ERROR: " + ex.Message, "FAIL");
                    LogEvents.RegisterEvent(2, "OffScanners: " + ex.Message);
                }
            }
        }

        void OffScanner()
        {
            try
            {
                if (_SingleKeyence.IsOpen) _SingleKeyence.Close();
            }
            catch (Exception ex)
            {
                WriteDgv(1, DateTime.Now, "SINGLE_SCANNER", "ERROR: " + ex.Message, "FAIL");
                WriteDgv(2, DateTime.Now, "SINGLE_SCANNER", "ERROR: " + ex.Message, "FAIL");
                LogEvents.RegisterEvent(1, "OffScanner: " + ex.Message);
                LogEvents.RegisterEvent(2, "OffScanner: " + ex.Message);
            }
        }
  
        private void _Keyence1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string Result = String.Empty;

            try 
            {
                if (Globals.SCANNER_BASE == "USB")
                    Result = _Keyence1.ReadExisting();

                if (Globals.SCANNER_BASE == "SERIAL")
                    Result = _Keyence1.ReadLine();

                try { Result = Result.TrimEnd('\r'); }
                catch (Exception ex) { LogEvents.RegisterEvent(1, "Keyence1_DataReceived Delete r: " + ex.Message); }

                if (Result.Length == Globals.SN_LENGH || Result.Length == Globals.SMOTHER_LENGH)
                {
                    Globals.SERIAL_NUMBER1 = Result;
                    WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "DATA RECEIVED", "PASS");
                    if (!Busy_1) Task.Factory.StartNew(() => VerifyProcess1());

                }
            }
            catch(Exception ex) 
            {
                LogEvents.RegisterEvent(1, "Keyence1_DataReceived: " + ex.Message);
            }        
        }

        private void _Keyence1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void _Keyence2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string Result = String.Empty;

            try 
            {
                if (Globals.SCANNER_BASE == "USB")
                    Result = _Keyence2.ReadExisting();

                if (Globals.SCANNER_BASE == "SERIAL")
                    Result = _Keyence2.ReadLine();

                try { Result = Result.TrimEnd('\r'); }
                catch (Exception ex) { }

                if (Result.Length == Globals.SN_LENGH || Result.Length == Globals.SMOTHER_LENGH)
                {
                    Globals.SERIAL_NUMBER2 = Result;
                    WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "DATA RECEIVED", "PASS");
                    if (!Busy_2) Task.Factory.StartNew(() => VerifyProcess2());
                }
            }
            catch(Exception ex) 
            {
                LogEvents.RegisterEvent(2, "Keyence2_DataReceived: " + ex.Message);
            }      
        }

        private void _SingleKeyence_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string Result = String.Empty;

            try
            {
                if (Globals.SCANNER_BASE == "USB")
                    Result = _SingleKeyence.ReadExisting();

                if (Globals.SCANNER_BASE == "SERIAL")
                    Result = _SingleKeyence.ReadLine();

                try { Result = Result.TrimEnd('\r'); }
                catch (Exception ex) { LogEvents.RegisterEvent(1, "_SingleKeyence_ErrorReceived Delete r: " + ex.Message); }

                if (Result.Length == Globals.SN_LENGH || Result.Length == Globals.SMOTHER_LENGH)
                {
                    if (Globals._SHUTTLE_LANE_ACTIVE == "LANE_1")
                    {
                        Globals.SERIAL_NUMBER1 = Result;
                        WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "DATA RECEIVED", "PASS");
                        if (!Busy_1) Task.Factory.StartNew(() => VerifyProcess1());
                    }

                    if (Globals._SHUTTLE_LANE_ACTIVE == "LANE_2")
                    {
                        Globals.SERIAL_NUMBER2 = Result;
                        WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "DATA RECEIVED", "PASS");
                        if (!Busy_2) Task.Factory.StartNew(() => VerifyProcess2());
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvents.RegisterEvent(1, "_SingleKeyence_ErrorReceived: " + ex.Message);
            }
        }

        private void _SingleKeyence_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void _TimerReadDAQ(object sender, EventArgs e)
        {
            if (!Globals.DAQ_OK) 
            {
                Ni.WriteDAQ(DAQDefault);
                return;
            } 

            Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();

            if (!Globals.IS_SHUTTLE) 
            {
                if (Globals.IS_ACTIVE_LANE_1 || Globals.PASS_THRUE) VerifyLane1();
                if (Globals.IS_ACTIVE_LANE_2 || Globals.PASS_THRUE) VerifyLane2();
            }

            if (Globals.IS_SHUTTLE) 
            {
                if (Globals.IS_ACTIVE_LANE_1 || Globals.PASS_THRUE) 
                {
                    VerifyLane1Shuttle();                    
                }

                if (Globals.IS_ACTIVE_LANE_2 || Globals.PASS_THRUE) 
                {
                    VerifyLane2Shuttle();
                }
            }  
        }

        void VerifyLane1()
        {
            try 
            {
                if (Globals.DAQ_OUT_PUTS[1])
                {
                    Globals._SHUTTLE_LANE_ACTIVE = "LANE_1";

                    lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); //DEEP GREEN 900
                    lblLane1IO.Content = Globals.SERIAL_NUMBER1;

                    if (!Busy_1 && !Globals.PASS_THRUE)
                    {
                        _Keyence1.Write("LON\r");

                        Globals.COUNT_RETRY_SCANNING1++;

                        if (Globals.COUNT_RETRY_SCANNING1 > 1)
                            WriteDgv(1, DateTime.Now, "NULL", "SCANNING RETRY: " + Globals.COUNT_RETRY_SCANNING1.ToString(), "FAIL");
                    }

                    if (Busy_1 && CheckPointLane1_OK || Globals.PASS_THRUE)
                    {
                        _TimerDAQ.Stop(); //Validacion
                        Ni.WriteDAQ(outLane1_OK);

                        while (Globals.DAQ_OUT_PUTS[1])
                        {
                            Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                    new Action(delegate { }));
                        }

                        Ni.WriteDAQ(DAQDefault);
                        _TimerDAQ.Start(); //Validacion
                        CheckPointLane1_OK = false;
                        CleanUP1();
                    }
                }

                else               
                {
                    Globals.COUNT_RETRY1 = 0;

                    if(!Globals.PASS_THRUE) _Keyence1.Write("LOFF\r");
                      
                    Busy_1 = false;

                    if(Globals.PASS_THRUE) 
                        lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    else                       
                        lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                    lblLane1IO.Content = "";
                    CleanUP1();
                }

                if (Globals.COUNT_RETRY_SCANNING1 == Globals.RETRIES_SCANNING)
                {
                    _Keyence1.Write("LOFF\r");
                    _TimerDAQ.Stop(); //Validacion

                    Ni.WriteDAQ(outLane1_NG);
                    WriteDgv(1, DateTime.Now, "NULL", "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING1.ToString(), "FAIL");
                    LogEvents.RegisterEvent(1, "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING1);
                   
                    while (Globals.DAQ_OUT_PUTS[1])
                    {           
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }

                if (Globals.COUNT_RETRY1 == Globals.RETRIES_CHECKPROCESS)
                {
                    _Keyence1.Write("LOFF\r");
                    _TimerDAQ.Stop(); //Validacion

                    Ni.WriteDAQ(outLane1_NG);
                    WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY1.ToString(), "FAIL");
                    LogEvents.RegisterEvent(1, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY1);

                    while (Globals.DAQ_OUT_PUTS[1])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    Ni.WriteDAQ(DAQDefault);
             
                }
            }
            catch(Exception ex)
            {
                LogEvents.RegisterEvent(1, "VERIFY LANE: " + ex.Message);
            }           
        }

        void VerifyLane2() 
        {
            try 
            {
                if (Globals.DAQ_OUT_PUTS[6] || Globals.DAQ_OUT_PUTS[7])
                {
                    Globals._SHUTTLE_LANE_ACTIVE = "LANE_2";

                    lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); //DEEP BLACK
                    lblLane2IO.Content = Globals.SERIAL_NUMBER2;

                    if (!Busy_2 && !Globals.PASS_THRUE)
                    {
                        _Keyence2.Write("LON\r");

                        Globals.COUNT_RETRY_SCANNING2++;

                        if (Globals.COUNT_RETRY_SCANNING2 > 1)
                            WriteDgv(2, DateTime.Now, "NULL", "SCANNING RETRY: " + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    }


                    if (Busy_2 && CheckPointLane2_OK || Globals.PASS_THRUE)
                    {
                        _TimerDAQ.Stop(); //Validacion
                        Ni.WriteDAQ(outLane2_OK);

                        while (Globals.DAQ_OUT_PUTS[6])
                        {
                            Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                    new Action(delegate { }));                         
                        }

                        Ni.WriteDAQ(DAQDefault);
                        _TimerDAQ.Start(); //Validacion
                        CheckPointLane2_OK = false;
                        CleanUP2();
                    }
                }
                
                if (!Globals.DAQ_OUT_PUTS[6])
                {
                    Globals.COUNT_RETRY2 = 0;

                    if (!Globals.PASS_THRUE) _Keyence2.Write("LOFF\r");

                    Busy_2 = false;

                    if (Globals.PASS_THRUE)
                        lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    else
                        lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                    lblLane2IO.Content = "";
                    CleanUP2();
                }
                
                if (Globals.COUNT_RETRY_SCANNING2 == Globals.RETRIES_SCANNING)
                {
                    _Keyence2.Write("LOFF\r");
                    _TimerDAQ.Stop(); //Validacion

                    Ni.WriteDAQ(outLane2_NG);
                    WriteDgv(2, DateTime.Now, "NULL", "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    LogEvents.RegisterEvent(2, "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2);

                    while (Globals.DAQ_OUT_PUTS[6])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }

                if (Globals.COUNT_RETRY2 == Globals.RETRIES_CHECKPROCESS)
                {
                    _Keyence2.Write("LOFF\r");
                    _TimerDAQ.Stop(); //Validacion

                    Ni.WriteDAQ(outLane2_NG);
                    WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    LogEvents.RegisterEvent(2, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY2);
                    
                    while (Globals.DAQ_OUT_PUTS[6])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }
            }
            catch(Exception ex) 
            {
                LogEvents.RegisterEvent(2, "VERIFY LANE: " + ex.Message);
            }    
        }

        void VerifyLane1Shuttle()
        {
            try
            {
                if (Globals.DAQ_OUT_PUTS[1])
                {
                    Globals._SHUTTLE_LANE_ACTIVE = "LANE_1";

                    lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); //DEEP GREEN 900
                    lblLane1IO.Content = Globals.SERIAL_NUMBER1;

                    if (!Busy_1 && !Globals.PASS_THRUE)
                    {
                        _SingleKeyence.Write("LON\r");

                        Globals.COUNT_RETRY_SCANNING1++;

                        if (Globals.COUNT_RETRY_SCANNING1 > 1)
                            WriteDgv(1, DateTime.Now, "NULL", "SCANNING RETRY: " + Globals.COUNT_RETRY_SCANNING1.ToString(), "FAIL");
                    }

                    if (Busy_1 && CheckPointLane1_OK || Globals.PASS_THRUE)
                    {
                        _TimerDAQ.Stop(); //Validacion
                        Ni.WriteDAQ(outLane1_OK);

                        while (Globals.DAQ_OUT_PUTS[1])
                        {
                            Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                    new Action(delegate { }));
                        }

                        Ni.WriteDAQ(DAQDefault);

                        _TimerDAQ.Start(); //Validacion

                        CheckPointLane1_OK = false;
                        CleanUP1();
                    }
                }

                else
                {
                    Globals.COUNT_RETRY1 = 0;

                    if (!Globals.PASS_THRUE && Globals._SHUTTLE_LANE_ACTIVE == "LANE_1")
                        _SingleKeyence.Write("LOFF\r");

                    Busy_1 = false;

                    if (Globals.PASS_THRUE)
                        lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    else
                        lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                    lblLane1IO.Content = "";
                    CleanUP1();
                }

                if (Globals.COUNT_RETRY_SCANNING1 == Globals.RETRIES_SCANNING)
                {
                    _SingleKeyence.Write("LOFF\r");

                    _TimerDAQ.Stop(); //Validacion

                    Ni.WriteDAQ(outLane1_NG);
                    WriteDgv(1, DateTime.Now, "NULL", "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING1.ToString(), "FAIL");
                    LogEvents.RegisterEvent(1, "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING1);

                    while (Globals.DAQ_OUT_PUTS[1])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    WaitNSeconds(10);
                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }

                if (Globals.COUNT_RETRY1 == Globals.RETRIES_CHECKPROCESS)
                {
                    _SingleKeyence.Write("LOFF\r");

                    _TimerDAQ.Stop(); //Validacion
                    Ni.WriteDAQ(outLane1_NG);
                    WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY1.ToString(), "FAIL");
                    LogEvents.RegisterEvent(1, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY1);

                    while (Globals.DAQ_OUT_PUTS[1])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    WaitNSeconds(10);
                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }
            }
            catch (Exception ex)
            {
                LogEvents.RegisterEvent(1, "VERIFY LANE: " + ex.Message);
            }
        }

        void VerifyLane2Shuttle()
        {
            try
            {
                if (Globals.DAQ_OUT_PUTS[6])
                {
                    Globals._SHUTTLE_LANE_ACTIVE = "LANE_2";

                    lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); //DEEP BLACK
                    lblLane2IO.Content = Globals.SERIAL_NUMBER2;

                    if (!Busy_2 && !Globals.PASS_THRUE)
                    {
                        _SingleKeyence.Write("LON\r");

                        Globals.COUNT_RETRY_SCANNING2++;

                        if (Globals.COUNT_RETRY_SCANNING2 > 1)
                            WriteDgv(2, DateTime.Now, "NULL", "SCANNING RETRY: " + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    }


                    if (Busy_2 && CheckPointLane2_OK || Globals.PASS_THRUE)
                    {
                        _TimerDAQ.Stop(); //Validacion
                        Ni.WriteDAQ(ShuttlerOutLane2_OK);

                        while (Globals.DAQ_OUT_PUTS[6])
                        {
                            Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                    new Action(delegate { }));
                        }

                        Ni.WriteDAQ(DAQDefault);
                        _TimerDAQ.Start(); //Validacion

                        CheckPointLane2_OK = false;
                        CleanUP2();
                    }
                }

                else
                {
                    Globals.COUNT_RETRY2 = 0;

                    if (!Globals.PASS_THRUE && Globals._SHUTTLE_LANE_ACTIVE == "LANE_2") 
                        _SingleKeyence.Write("LOFF\r");

                    Busy_2 = false;

                    if (Globals.PASS_THRUE)
                        lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    else
                        lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                    lblLane2IO.Content = "";
                    CleanUP2();
                }

                if (Globals.COUNT_RETRY_SCANNING2 == Globals.RETRIES_SCANNING)
                {
                    _SingleKeyence.Write("LOFF\r");

                    _TimerDAQ.Stop(); //Validacion
                    Ni.WriteDAQ(outLane2_NG);
                    WriteDgv(2, DateTime.Now, "NULL", "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    LogEvents.RegisterEvent(2, "VERIFY LANE: RETRIES LIMIT SCANNING EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2);

                    while (Globals.DAQ_OUT_PUTS[6])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    WaitNSeconds(10);
                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }

                if (Globals.COUNT_RETRY2 == Globals.RETRIES_CHECKPROCESS)
                {
                    _SingleKeyence.Write("LOFF\r");

                    _TimerDAQ.Stop(); //Validacion
                    Ni.WriteDAQ(outLane2_NG);
                    WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY_SCANNING2.ToString(), "FAIL");
                    LogEvents.RegisterEvent(2, "VERIFY LANE: RETRIES LIMIT VERIFY PROCESS EXCEEDED, SENDING AS NG BOARD, RETRIES:" + Globals.COUNT_RETRY2);

                    while (Globals.DAQ_OUT_PUTS[6])
                    {
                        Globals.DAQ_OUT_PUTS = Ni.ReadDAQ();
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                new Action(delegate { }));
                    }

                    WaitNSeconds(10);
                    Ni.WriteDAQ(DAQDefault);
                    _TimerDAQ.Start(); //Validacion
                }
            }
            catch (Exception ex)
            {
                LogEvents.RegisterEvent(2, "VERIFY LANE: " + ex.Message);
            }
        }

        void VerifyProcess1() 
        {
            if (string.IsNullOrEmpty(Globals.SERIAL_NUMBER1) || Busy_1) return;
           
          RetryCheckPoint:

            Busy_1 = true;
            Globals.COUNT_MATRIX1 = 0;
            Globals.DT_LANE1 = StaticFunctions.VerifyCheckPointNew(Globals.SERIAL_NUMBER1);
            
            foreach (DataRow _dr in Globals.DT_LANE1.Rows)
            {
                string _TempStep = _dr[7].ToString();
                string _TempStatus = _dr[8].ToString();

                //if (_TempStep == "MRB" || _TempStep == Globals.STEP_TO_CHECK) 
                if (_TempStep == "QC / MRB" || _TempStep == Globals.STEP_TO_CHECK && _TempStatus == "Pass" || _TempStep == "FVT / PBTS" && _TempStatus == "Fail") 
                {
                    Globals.COUNT_MATRIX1++; 
                }
                else {  }
            }


            if (Globals.COUNT_MATRIX1 != 15)
            {
                Globals.COUNT_RETRY1++;
                CheckPointLane1_OK = false;
                WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "VERIFY PROCESS RETRY: " + Globals.COUNT_RETRY1.ToString(), "FAIL");

                if (Globals.COUNT_RETRY1 < Globals.RETRIES_CHECKPROCESS) goto RetryCheckPoint;              
                Busy_1 = false;

                LogEvents.RegisterEvent(1, "VERIFY PROCESS: FAIL " + Globals.SERIAL_NUMBER1);

                DataTable _dtReport = Globals.DT_LANE1.Copy();
                foreach (DataRow _drReport in _dtReport.Rows) 
                {
                    string SN = _drReport[3].ToString();
                    string Array = _drReport[4].ToString();
                    string StepName = _drReport[7].ToString();
                    string Status = _drReport[8].ToString();

                    string LogString = "SN: " + SN + " " +
                                        "POS: " + Array + " " +
                                        "STEP: " + StepName + " " +
                                        "STATUS: " + Status;

                    LogEvents.RegisterEvent(1, "VERIFY PROCESS: DETAIL " + LogString);
                }              
            }

            if (Globals.COUNT_MATRIX1 == 15)
            {
                CheckPointLane1_OK = true;
                WriteDgv(1, DateTime.Now, Globals.SERIAL_NUMBER1, "VERIFY PROCESS: OK SENDING AS A GOOD BOARD", "PASS");
                LogEvents.RegisterEvent(1, "VERIFY PROCESS: OK SENDING AS A GOOD BOARD " + Globals.SERIAL_NUMBER1);
                CleanUP1();
            }
        }

        void VerifyProcess2()
        {
            if (string.IsNullOrEmpty(Globals.SERIAL_NUMBER2) || Busy_2) return;

          RetryCheckPoint:

            Busy_2 = true;
            Globals.COUNT_MATRIX2 = 0;
            Globals.DT_LANE2 = StaticFunctions.VerifyCheckPointNew(Globals.SERIAL_NUMBER2);

            foreach (DataRow _dr in Globals.DT_LANE2.Rows)
            {
                string _TempStep = _dr[7].ToString();
                string _TempStatus = _dr[8].ToString();

                if (_TempStep == "QC / MRB" || _TempStep == Globals.STEP_TO_CHECK && _TempStatus == "Pass" || _TempStep == "FVT / PBTS" && _TempStatus == "Fail") 
                {
                    Globals.COUNT_MATRIX2++;
                }
                else { }
            }


            if (Globals.COUNT_MATRIX2 != 15)
            {
                Globals.COUNT_RETRY2++;
                CheckPointLane2_OK = false;
                WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "VERIFY PROCESS RETRY: " + Globals.COUNT_RETRY2.ToString(), "FAIL");

                if (Globals.COUNT_RETRY2 < Globals.RETRIES_CHECKPROCESS) goto RetryCheckPoint;
                Busy_2 = false;

                LogEvents.RegisterEvent(2, "VERIFY PROCESS: FAIL " + Globals.SERIAL_NUMBER2);

                DataTable _dtReport = Globals.DT_LANE2.Copy();
                foreach (DataRow _drReport in _dtReport.Rows)
                {
                    string SN = _drReport[3].ToString();
                    string Array = _drReport[4].ToString();
                    string StepName = _drReport[7].ToString();
                    string Status = _drReport[8].ToString();

                    string LogString = "SN: " + SN + " " +
                                        "POS: " + Array + " " +
                                        "STEP: " + StepName + " " +
                                        "STATUS: " + Status;

                    LogEvents.RegisterEvent(2, "VERIFY PROCESS: DETAIL " + LogString);
                }
            }

            if (Globals.COUNT_MATRIX2 == 15)
            {
                CheckPointLane2_OK = true;
                WriteDgv(2, DateTime.Now, Globals.SERIAL_NUMBER2, "VERIFY PROCESS : OK SENDING AS A GOOD BOARD", "PASS");
                LogEvents.RegisterEvent(2, "VERIFY PROCESS: OK SENDING AS A GOOD BOARD " + Globals.SERIAL_NUMBER2);
                CleanUP2();             
            }
        }

        private void WaitNSeconds(int segundos)
        {
            if (segundos < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(segundos);
            while (DateTime.Now < _desired)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                      new Action(delegate { }));
            }
        }

        void CleanUP1()
        {
            Globals.SERIAL_NUMBER1 = string.Empty;
            Globals.STEPS_MISSING1 = string.Empty;
            Globals.COUNT_MATRIX1 = 0;
            Globals.COUNT_RETRY_SCANNING1 = 0;
            Globals.COUNT_RETRY1 = 0;
        }

        void CleanUP2()
        {
            Globals.SERIAL_NUMBER2 = string.Empty;
            Globals.STEPS_MISSING2 = string.Empty;
            Globals.COUNT_MATRIX2 = 0;
            Globals.COUNT_RETRY_SCANNING2 = 0;
        }

        void WriteDgv(int _LANE, DateTime _DATE, string _SERIAL_NUMBER, string _PROCESS, string _STATUS)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {     
                if(_LANE == 1) 
                {
                    try 
                    {
                        if (DgLane1.Items.Count > 100) DgLane1.Items.Clear();

                        DgLane1.Items.Add(new DataLane1 { DATE = _DATE, SERIAL_NUMBER = _SERIAL_NUMBER, PROCESS = _PROCESS, STATUS = _STATUS });

                        if (DgLane1.Items.Count > 0)
                        {
                            var border = VisualTreeHelper.GetChild(DgLane1, 0) as Decorator;
                            if (border != null)
                            {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) scroll.ScrollToEnd();
                            }
                        }
                    }
                    catch(Exception ex) 
                    {
                        WriteDgv(1, DateTime.Now, "APP ERROR", ex.Message, "FAIL");
                        LogEvents.RegisterEvent(1, "WRITEDGV: " + ex.Message);
                    }
                   
                }

                if (_LANE == 2) 
                { 
                    try 
                    {
                        if (DgLane2.Items.Count > 100) DgLane2.Items.Clear();

                        DgLane2.Items.Add(new DataLane2 { DATE = _DATE, SERIAL_NUMBER = _SERIAL_NUMBER, PROCESS = _PROCESS, STATUS = _STATUS });

                        if (DgLane2.Items.Count > 0)
                        {
                            var border = VisualTreeHelper.GetChild(DgLane2, 0) as Decorator;
                            if (border != null)
                            {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) scroll.ScrollToEnd();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteDgv(2, DateTime.Now, "APP ERROR", ex.Message, "FAIL");
                        LogEvents.RegisterEvent(2, "WRITEDGV: " + ex.Message);
                    }
                }

                return;
            }));
        }

        private void lblLane1IO_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try { if (!string.IsNullOrEmpty(lblLane1IO.Content.ToString())) new HistroyDataWin(1).Show(); }
            catch(Exception ex) { }        
        }

        private void lblLane2IO_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try { if (!string.IsNullOrEmpty(lblLane2IO.Content.ToString())) new HistroyDataWin(2).Show(); }
            catch (Exception ex) { }
        }

        #region Events Handler DAQ
        NationalInstruments.DAQmx.Task myTask;
        private NationalInstruments.DAQmx.DigitalSingleChannelReader myDigitalReader;
        void DAQEvents() 
        {
            myTask = new NationalInstruments.DAQmx.Task();

            myTask.DIChannels.CreateChannel("DIO/port1/line0:0", "", NationalInstruments.DAQmx.ChannelLineGrouping.OneChannelForAllLines);

            myTask.Timing.ConfigureChangeDetection("DIO/port1/line0:7", "DIO/port1/line0:7", NationalInstruments.DAQmx.SampleQuantityMode.ContinuousSamples);

            myTask.SynchronizeCallbacks = true;

            //myTask.DigitalChangeDetection += new NationalInstruments.DAQmx.DigitalChangeDetectionEventHandler(MyTask_DigitalChangeDetection);
            myTask.DigitalChangeDetection += new NationalInstruments.DAQmx.DigitalChangeDetectionEventHandler(MyTask_DigitalChangeDetection);

            myDigitalReader = new NationalInstruments.DAQmx.DigitalSingleChannelReader(myTask.Stream);

            myTask.Start();
        }

        private void MyTask_DigitalChangeDetection(object sender, NationalInstruments.DAQmx.DigitalChangeDetectionEventArgs e)
        {
            
        }

        #endregion

        #region Generic Events application
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.DOCK_MENU)
            {
                //Margin="10,120,10,440"
                //Thickness _temp = lblEventosLane1.Margin;
                //_temp.Left = 200f;

                btnOnOff_Lane1.Visibility = Visibility.Visible;
                btnOnOff_Lane2.Visibility = Visibility.Visible;
                btnDebug.Visibility = Visibility.Visible;
                btnPassThru.Visibility = Visibility.Visible;
                //btnMenu.Content = "MENU";
                //btnMenu.Width = 168;
                //btnMenu.Height = 75;
                Globals.DOCK_MENU = false;
                DockMenu.Width = 180;
                return;
            }

            if (!Globals.DOCK_MENU)
            {
                //Thickness _temp = lblEventosLane1.Margin;
                //_temp.Left = 10f;

                btnOnOff_Lane1.Visibility = Visibility.Hidden;
                btnOnOff_Lane2.Visibility = Visibility.Hidden;
                btnDebug.Visibility = Visibility.Hidden;
                btnPassThru.Visibility = Visibility.Hidden;
                //btnMenu.Content = "M";
                //btnMenu.Width = 45;
                //btnMenu.Height = 45;
                Globals.DOCK_MENU = true;
                DockMenu.Width = 0;
                return;
            }
        }

        private void btnOnOff_Lane1_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PASS_THRUE) return;

            Button button = (Button)sender;
            
            if (Globals.IS_ACTIVE_LANE_1)
            {
                LogEvents.RegisterEvent(1, "DISABLE");
                WriteDgv(1, DateTime.Now, "LANE 1 DISABLE", "NULL", "PASS");
                Globals.IS_ACTIVE_LANE_1 = false;
                InitLanes();
                OffScanners();
                Ni.WriteDAQ(DAQDefault);
                return;
            }

            if (!Globals.IS_ACTIVE_LANE_1)
            {
                LogEvents.RegisterEvent(1, "ENABLE");
                WriteDgv(1, DateTime.Now, "LANE 1 ACTIVE", "NULL", "PASS");           
                Globals.IS_ACTIVE_LANE_1 = true;
                InitLanes();
                OnScanners();
                Ni.WriteDAQ(DAQDefault);
                return;
            }
        }

        private void btnOnOff_Lane2_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PASS_THRUE) return;

            Button button = (Button)sender;

            if (Globals.IS_ACTIVE_LANE_2)
            {
                LogEvents.RegisterEvent(2, "DISABLE");
                WriteDgv(2, DateTime.Now, "LANE 2 DISABLE", "NULL", "PASS");
                Globals.IS_ACTIVE_LANE_2 = false;
                InitLanes();
                Ni.WriteDAQ(DAQDefault);
                return;
            }

            if (!Globals.IS_ACTIVE_LANE_2)
            {
                LogEvents.RegisterEvent(2, "ENABLE");
                WriteDgv(2, DateTime.Now, "LANE 2 ACTIVE", "NULL", "PASS");                
                Globals.IS_ACTIVE_LANE_2 = true;
                InitLanes();
                Ni.WriteDAQ(DAQDefault);
                return;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) 
            {
                _TimerDAQ.Stop(); //Validacion
                DragMove();
                _TimerDAQ.Start(); //Validacion
            }
           
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DgLane1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;
            DataLane1 _myData = new DataLane1();
            _myData = (DataLane1)row.DataContext;

            if (_myData.STATUS == "PASS")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5E20")); //DEEP GREEN 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "FAIL")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B71C1C")); //DEEP RED 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "UNKOWN")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF360C")); //DEEP ORANGE 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }
        }

        private void DgLane2_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;
            DataLane2 _myData = new DataLane2();
            _myData = (DataLane2)row.DataContext;

            if (_myData.STATUS == "PASS")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5E20")); //DEEP GREEN 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "FAIL")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B71C1C")); //DEEP RED 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "UNKOWN")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF360C")); //DEEP ORANGE 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

        }

        private void btnPassThru_Click(object sender, RoutedEventArgs e)
        {           
            if (Globals.PASS_THRUE)
            {
                Globals.IS_ACTIVE_LANE_1 = false;
                Globals.IS_ACTIVE_LANE_2 = false;
                InitLanes();

                lblLane1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
                lblLane1.Content = "LANE 1";
                lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                lblLane2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));
                lblLane2.Content = "LANE 2";
                lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242"));

                WriteDgv(1, DateTime.Now, "NULL", "PASS THRUE DISABLE", "PASS");
                WriteDgv(2, DateTime.Now, "NULL", "PASS THRUE DISABLE", "PASS");
                LogEvents.RegisterEvent(13, "DISABLE PASS THROUGH MODE");
                Globals.PASS_THRUE = false;
                return;
            }

            if (!Globals.PASS_THRUE)
            {
                bool? result = new UsersLogin().ShowDialog();
                if (result == true) 
                {
                    lblLane1Disable.Visibility = Visibility.Hidden;
                    lblLane1Disable.Width = 0;
                    lblLane2Disable.Visibility = Visibility.Hidden;
                    lblLane2Disable.Width = 0;

                    lblLane1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    lblLane1.Content = "LANE 1 PASS THRUE >>>";
                    lblLane1IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));

                    lblLane2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));
                    lblLane2.Content = "LANE 2 PASS THRUE >>>";
                    lblLane2IO.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00897B"));

                    WriteDgv(1, DateTime.Now, "NULL", "PASS THRUE ENABLE", "PASS");
                    WriteDgv(2, DateTime.Now, "NULL", "PASS THRUE ENABLE", "PASS");
                    LogEvents.RegisterEvent(13, "ENABLE PASS THROUGH MODE");
                    Globals.PASS_THRUE = true;

                    btnMenu_Click(sender, e);
                    return;
                }  
            }
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PASS_THRUE) return;

            if (Globals.IS_ACTIVE_LANE_1) new HistroyDataWin(1).Show();
            if (Globals.IS_ACTIVE_LANE_2) new HistroyDataWin(2).Show();

            btnMenu_Click(sender, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_Keyence1.IsOpen) _Keyence1.Close();
            if (_Keyence2.IsOpen) _Keyence1.Close();
            if (_SingleKeyence.IsOpen) _SingleKeyence.Close();
            //myTask.Dispose();

            Ni.WriteDAQ(DAQDefault);
            LogEvents.RegisterEvent(13, "Finalizing application GOOD BAD CONVEYOR");
        }
        #endregion

  
    }
}
