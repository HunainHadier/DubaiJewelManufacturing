using DubaiJewelManufacturing.Models;
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
    /// Interaction logic for EditUserWindow.xaml
    /// </summary>
    public partial class EditUserWindow : Window
    {

        private UserData _user;
        private DatabaseHelper db = new DatabaseHelper();

     
        public EditUserWindow(UserData user)
        {
            InitializeComponent();
            _user = user;

            // Fill current fields
            txtCurrentUsername.Text = user.Username;
            txtCurrentRole.Text = user.Role;
            txtCurrentUserID.Text = user.UserID;
            txtCurrentPassword.Text = user.Password;
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = string.IsNullOrWhiteSpace(txtNewUsername.Text) ? _user.Username : txtNewUsername.Text;
            string newPassword = string.IsNullOrWhiteSpace(txtNewPassword.Text) ? _user.Password : txtNewPassword.Text;
            string newUserId = string.IsNullOrWhiteSpace(txtNewUserID.Text) ? _user.UserID : txtNewUserID.Text;
            string newRoleName = string.IsNullOrWhiteSpace(txtNewRole.Text) ? _user.Role : txtNewRole.Text;

            using (var conn = db.GetConnection())
            {
                conn.Open();

                // 1️⃣ Role_Name se Role_Id nikalna
                string roleIdQuery = "SELECT Role_Id FROM roles WHERE RoleName=@role LIMIT 1";
                string roleId = _user.Role; // default

                using (var roleCmd = new MySqlCommand(roleIdQuery, conn))
                {
                    roleCmd.Parameters.AddWithValue("@role", newRoleName);
                    object result = roleCmd.ExecuteScalar();
                    if (result != null)
                        roleId = result.ToString();
                    else
                    {
                        MessageBox.Show("Role not found in database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // 2️⃣ Update users table
                string updateQuery = @"UPDATE users 
                               SET User_Id=@newId, Username=@newName, UserPassword=@newPass, Role_Id=@roleId
                               WHERE User_Id=@oldId";

                using (var cmd = new MySqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@newId", newUserId);
                    cmd.Parameters.AddWithValue("@newName", newUsername);
                    cmd.Parameters.AddWithValue("@newPass", newPassword);
                    cmd.Parameters.AddWithValue("@roleId", roleId);
                    cmd.Parameters.AddWithValue("@oldId", _user.UserID);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM users WHERE User_Id=@id";
                    using (var cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _user.UserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User deleted successfully!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Bas close kar do, DialogResult mat use karo
        }


        
    }
}
