using System;
using System.Collections.Generic;
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

namespace GoodBad_Conveyor_2._0
{
    /// <summary>
    /// Interaction logic for UsersLogin.xaml
    /// </summary>
    public partial class UsersLogin : Window
    {
        public UsersLogin()
        {
            InitializeComponent();
        }

    
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PwdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PwdBox.Password == "0000000000") PwdBox.Password = string.Empty;
        }

        private void PwdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PwdBox.Password)) PwdBox.Password = "00000000000";
        }

        private void PwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) 
            {
                if (xcBoxUsers.Text == "Administrator" && PwdBox.Password == "Jabil2022") DialogResult = true;
                if (xcBoxUsers.Text == "Operator" && PwdBox.Password == "1234") DialogResult = false;
            }

        }
    }
}
