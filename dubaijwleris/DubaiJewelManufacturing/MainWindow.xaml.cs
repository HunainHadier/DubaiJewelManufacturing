using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DubaiJewelManufacturing
{
    public partial class MainWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private bool ValidateLoginFields()
        {
            bool isValid = true;

            IdTextBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            PasswordBox.BorderBrush = new SolidColorBrush(Colors.Gray);

            if (string.IsNullOrWhiteSpace(IdTextBox.Text))
            {
                IdTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid = false;
            }

            if (!isValid)
            {
                MessageBox.Show("Please fill all required fields!", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateLoginFields()) return;

            string userId = IdTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            using (var conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                SELECT u.User_Id, u.UserName, u.Role_Id, r.RoleName 
                FROM users u
                INNER JOIN roles r ON u.Role_Id = r.Role_Id
                WHERE u.User_Id = @user AND u.UserPassword = @pass
                LIMIT 1";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", userId);
                        cmd.Parameters.AddWithValue("@pass", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userIdFromDb = reader["User_Id"].ToString();
                                string userName = reader["UserName"].ToString();  // ✅ Actual username
                                string roleId = reader["Role_Id"].ToString();
                                string roleName = reader["RoleName"].ToString();

                                MessageBox.Show($"Login Successful! Role: {roleName}",
                                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                if (roleId == "1") // Admin
                                {
                                    AdminMainWindow adminWindow = new AdminMainWindow(userName); // ✅ pass userName
                                    adminWindow.WindowState = WindowState.Maximized;
                                    adminWindow.HeaderCtrl.UserName = userName; // ✅ Show on header
                                    adminWindow.Show();
                                    this.Close();
                                }
                                else // Regular User
                                {
                                    BarCodeWindow userWindow = new BarCodeWindow(roleId, userName);
                                    userWindow.WindowState = WindowState.Maximized;
                                    // If you want to show name in BarCodeWindow header
                                    // userWindow.HeaderCtrl.UserName = userName;
                                    userWindow.Show();
                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid ID or Password!", "Error",
                                                MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
