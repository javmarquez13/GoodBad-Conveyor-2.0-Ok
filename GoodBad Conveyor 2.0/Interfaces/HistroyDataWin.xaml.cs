using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GoodBad_Conveyor_2._0
{
    /// <summary>
    /// Interaction logic for HistroyDataWin.xaml
    /// </summary>
    public partial class HistroyDataWin : Window
    {
        DispatcherTimer RefreshMESData = new DispatcherTimer();
        DataTable _dt;
        int LaneDataWin = 0;


        public HistroyDataWin(int Lane)
        {
            InitializeComponent();
            InitDataGrid();
            LaneDataWin = Lane;

            if (LaneDataWin == 1) lblTitle.Content = "MES Data LANE 1";
            if (LaneDataWin == 2) lblTitle.Content = "MES Data LANE 2";

            RefreshMESData.Interval = new TimeSpan(0, 0, 1);
            RefreshMESData.Tick += RefreshMESData_Tick;
            RefreshMESData.Start();
        }

        public struct MESData
        {
            public string SERIAL_NUMBER { set; get; }
            public string MAPPING { set; get; }
            public string HISTORY { set; get; }
            public string STATUS { set; get; }
        }
        void InitDataGrid() 
        {
            DataGridTextColumn SERIAL_NUMBER = new DataGridTextColumn();
            SERIAL_NUMBER.Header = "SERIAL NUMBER";
            SERIAL_NUMBER.Binding = new Binding("SERIAL_NUMBER");
            SERIAL_NUMBER.Width = 125;
            SERIAL_NUMBER.IsReadOnly = true;

            DataGridTextColumn MAPPING = new DataGridTextColumn();
            MAPPING.Header = "MAPPING";
            MAPPING.Binding = new Binding("MAPPING");
            MAPPING.Width = 30;
            MAPPING.IsReadOnly = true;

            DataGridTextColumn HISTORY = new DataGridTextColumn();
            HISTORY.Header = "HISTORY";
            HISTORY.Binding = new Binding("HISTORY");
            HISTORY.Width = 200;
            HISTORY.IsReadOnly = true;

            DataGridTextColumn STATUS = new DataGridTextColumn();
            STATUS.Header = "STATUS";
            STATUS.Binding = new Binding("STATUS");
            STATUS.Width = 80;
            STATUS.IsReadOnly = true;

            DgMesData.Columns.Add(SERIAL_NUMBER);
            DgMesData.Columns.Add(MAPPING);
            DgMesData.Columns.Add(HISTORY);
            DgMesData.Columns.Add(STATUS);
        }

        void MainFunction() 
        {
            DgMesData.Items.Clear();
            DgMesData.DataContext = null;

            if(LaneDataWin == 1) _dt = Globals.DT_LANE1;
            if(LaneDataWin == 2) _dt = Globals.DT_LANE2;

            if (_dt == null) return;

            foreach (DataRow _dr in _dt.Rows)
            {               
                string SERIAL_NUMBER = _dr[3].ToString();
                string MAPPING = _dr[4].ToString();
                string HISTORY = _dr[7].ToString();
                string STATUS = _dr[8].ToString();

                WriteDgv(SERIAL_NUMBER, MAPPING, HISTORY, STATUS);
            }
        }

        private void RefreshMESData_Tick(object sender, EventArgs e)
        {
            try { MainFunction(); }
            catch(Exception ex) { }          
        }

        void WriteDgv(string _SERIAL_NUMBER, string _MAPPING, string _HISTROY, string _STATUS)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    DgMesData.Items.Add(new MESData { SERIAL_NUMBER = _SERIAL_NUMBER, MAPPING = _MAPPING, HISTORY = _HISTROY, STATUS = _STATUS });

                    if (DgMesData.Items.Count > 0)
                    {
                        var border = VisualTreeHelper.GetChild(DgMesData, 0) as Decorator;
                        if (border != null)
                        {
                            var scroll = border.Child as ScrollViewer;
                            if (scroll != null) scroll.ScrollToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDgv("APP ERROR", "0", ex.Message, "FAIL");
                }
            }));
        }

        private void DgMesData_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row;
            MESData _myData = new MESData();
            _myData = (MESData)row.DataContext;

            if (_myData.STATUS == "Pass")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5E20")); //DEEP GREEN 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "Fail" || _myData.STATUS == "QC / MRB")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B71C1C")); //DEEP RED 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }

            if (_myData.STATUS == "Missing Step")
            {
                row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")); //DEEP ORANGE 900
                row.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
    }
}
