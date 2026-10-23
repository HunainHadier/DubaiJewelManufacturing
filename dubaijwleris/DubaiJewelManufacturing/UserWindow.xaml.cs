using ClosedXML.Excel;
using DubaiJewelManufacturing.Models;
using ExcelDataReader;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();

        private List<UserData> importedUsers = new List<UserData>();
        private string _userName;
        public UserWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;

            // Agar HeaderControl hai, to yaha bind karo
            HeaderCtrl.UserName = _userName;

            LoadUsers();
        }

        // ✅ Default constructor bhi rakho agar kahin use ho
        public UserWindow() : this("Guest") { }


        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainWindow second = new AdminMainWindow(_userName);
            second.Show();       // Show the next window
            this.Close();        // Optional: close current one
        }
        private void Edit_User(object sender, RoutedEventArgs e)
        {
            // Selected user
            var selectedUser = UsersDataGrid.SelectedItem as UserData;
            if (selectedUser == null)
            {
                MessageBox.Show("Please select a user to edit!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditUserWindow modal = new EditUserWindow(selectedUser);
            modal.Owner = this;
            modal.ShowDialog();

            // Refresh grid after close
            LoadUsers();
        }




        private void LoadUsers()
        {
            List<UserData> users = new List<UserData>();

            using (var conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT u.User_Id, u.Username, u.UserPassword, r.RoleName
                        FROM users u
                        INNER JOIN roles r ON u.Role_Id = r.Role_Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserData
                            {
                                UserID = reader["User_Id"].ToString(),
                                Username = reader["Username"].ToString(),
                                Password = reader["UserPassword"].ToString(),
                                Role = reader["RoleName"].ToString()
                            });
                        }
                    }

                    UsersDataGrid.ItemsSource = users;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error loading users: " + ex.Message);
                }
            }
        }



        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            // DataGrid ke ItemsSource ko list me cast karein
            var users = UsersDataGrid.ItemsSource as List<UserData>;
            if (users == null || users.Count == 0)
            {
                MessageBox.Show("No data to export!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "UsersData.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Users");

                        // Headers
                        worksheet.Cell(1, 1).Value = "User ID";
                        worksheet.Cell(1, 2).Value = "Username";
                        worksheet.Cell(1, 3).Value = "Password";
                        worksheet.Cell(1, 4).Value = "Role";

                        // Data
                        int row = 2;
                        foreach (var user in users)
                        {
                            worksheet.Cell(row, 1).Value = user.UserID;
                            worksheet.Cell(row, 2).Value = user.Username;
                            worksheet.Cell(row, 3).Value = user.Password;
                            worksheet.Cell(row, 4).Value = user.Role;
                            row++;
                        }

                        // Auto adjust columns
                        worksheet.Columns().AdjustToContents();

                        // Save
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Excel exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting Excel: " + ex.Message);
                }
            }
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            DataTable dt = result.Tables[0]; // First sheet

                            importedUsers.Clear(); // old data clear

                            foreach (DataRow row in dt.Rows)
                            {
                                importedUsers.Add(new UserData
                                {
                                    UserID = row["User_Id"].ToString(),
                                    Username = row["Username"].ToString(),
                                    Password = row["UserPassword"].ToString(),
                                    Role = row["Role_Id"].ToString()  // ya RoleName agar excel me ho
                                });
                            }

                            // DataGrid me show karein
                            UsersDataGrid.ItemsSource = null;
                            UsersDataGrid.ItemsSource = importedUsers;

                            MessageBox.Show("Excel Imported Successfully! Click 'Save' to store in Database.",
                                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing Excel: " + ex.Message);
                }
            }
        }
        private void SaveImportedData_Click(object sender, RoutedEventArgs e)
        {
            if (importedUsers.Count == 0)
            {
                MessageBox.Show("No data to save! Please import Excel first.",
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();

                    foreach (var user in importedUsers)
                    {
                        string query = @"INSERT INTO users (User_Id, Username, Role_Id, UserPassword)
                                 VALUES (@id, @username, @role, @pass)";

                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", user.UserID);
                            cmd.Parameters.AddWithValue("@username", user.Username);
                            cmd.Parameters.AddWithValue("@role", user.Role);
                            cmd.Parameters.AddWithValue("@pass", user.Password);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Data Saved to Database Successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                importedUsers.Clear(); // memory clear
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }


    }
}
