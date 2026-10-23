using System;
using System.Windows;
using System.Windows.Controls;

namespace DubaiJewelManufacturing
{
    public partial class BarcodeformateControl : UserControl
    {
        public BarcodeformateControl()
        {
            InitializeComponent();
        }

        // ✅ DependencyProperty for UserName
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(
                nameof(UserName),
                typeof(string),
                typeof(BarcodeformateControl),
                new PropertyMetadata("User")
            );

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // optional initialization
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation Message
            var result = MessageBox.Show("Are you sure you want to logout?",
                                         "Logout",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // ✅ Sab windows close karo except MainWindow
                foreach (Window window in Application.Current.Windows)
                {
                    if (!(window is MainWindow))
                        window.Close();
                }

                // ✅ New Login window show karo
                MainWindow login = new MainWindow();
                login.Show();
            }
        }
    

        private void BarcodeFormatComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (BarcodeFormatComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selected = selectedItem.Content.ToString();
                Window parentWindow = Window.GetWindow(this);

                if (selected == "Normal")
                {
                    var normalWindow = new BarCodeWindow("1", UserName);
                    normalWindow.Show();
                    parentWindow?.Close();
                }
                else if (selected == "Chain")
                {
                    var chainWindow = new BarcodeSecondWindow(UserName);
                    chainWindow.Show();
                    parentWindow?.Close();
                }
            }
        }
    }
}
