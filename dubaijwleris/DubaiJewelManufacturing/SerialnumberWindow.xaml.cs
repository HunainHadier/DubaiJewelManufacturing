using MySql.Data.MySqlClient;
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
    /// Interaction logic for SerialnumberWindow.xaml
    /// </summary>
    public partial class SerialnumberWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        private string _userName;

        // ✅ Naya constructor jo username accept kare
        public SerialnumberWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;

            // Agar HeaderControl hai, waha user name show karo
            HeaderCtrl.UserName = _userName;

            int lastSerial = GetLastSerialFromDatabase();
            CurrentSerialNumberTextBox.Text = lastSerial.ToString();
        }

        // ✅ Purana default constructor optional hai agar kahin use ho raha ho
        public SerialnumberWindow() : this("Guest") { }
        private int GetLastSerialFromDatabase()
        {
            using (var conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT MAX(SerialNumber) FROM product_data";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newSerial = NewSerialNumberTextBox.Text.Trim();
            if (string.IsNullOrEmpty(newSerial))
            {
                MessageBox.Show("Please enter a new serial number!");
                return;
            }

            using (var conn = db.GetConnection())
            {
                conn.Open();

                // ✅ Update all rows with old serial to new serial
                string query = "UPDATE product_data SET SerialNumber=@newSerial WHERE SerialNumber=@oldSerial";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@newSerial", newSerial);
                    cmd.Parameters.AddWithValue("@oldSerial", CurrentSerialNumberTextBox.Text);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    MessageBox.Show($"{rowsAffected} rows updated successfully!", "Success");

                    // ✅ Frontend update
                    CurrentSerialNumberTextBox.Text = newSerial;
                    NewSerialNumberTextBox.Clear();
                }
            }
        }


        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainWindow second = new AdminMainWindow(_userName);
            second.Show();       // Show the next window
            this.Close();        // Optional: close current one
        }

    }
}
