using System.Windows;
using System.Windows.Controls;

namespace DubaiJewelManufacturing
{
    public partial class HeaderControl : UserControl
    {
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(string), typeof(HeaderControl), new PropertyMetadata("User"));

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public HeaderControl()
        {
            InitializeComponent();
            DataContext = this; // ✅ DataContext apne upar set karo
        }

        private void UserMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserMenuButton.ContextMenu != null)
            {
                UserMenuButton.ContextMenu.PlacementTarget = UserMenuButton;
                UserMenuButton.ContextMenu.IsOpen = true;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logging out...");

            foreach (Window window in Application.Current.Windows)
            {
                if (!(window is MainWindow))
                    window.Close();
            }

            MainWindow login = new MainWindow();
            login.Show();
        }
    }
}
