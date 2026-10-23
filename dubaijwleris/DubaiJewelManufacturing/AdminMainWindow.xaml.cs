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

namespace DubaiJewelManufacturing
{
    /// <summary>
    /// Interaction logic for AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        private string _userName;

        public AdminMainWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;
            HeaderCtrl.UserName = _userName;
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            SerialnumberWindow second = new SerialnumberWindow(_userName);
            second.Show();
            this.Close();
        }

        private void productpagee(object sender, RoutedEventArgs e)
        {
            ProductsWindow second = new ProductsWindow(_userName);
            second.Show();
            this.Close();
        }

        private void Barcode(object sender, RoutedEventArgs e)
        {
            BarCodeWindow second = new BarCodeWindow("1", _userName);
            second.Show();
            this.Close();
        }

        private void UserPage(object sender, RoutedEventArgs e)
        {
            UserWindow second = new UserWindow(_userName);
            second.Show();
            this.Close();
        }

        private void btnData_Click(object sender, RoutedEventArgs e)
        {
            DataWindow second = new DataWindow(_userName);
            second.Show();
            this.Close();
        }
    }

}
