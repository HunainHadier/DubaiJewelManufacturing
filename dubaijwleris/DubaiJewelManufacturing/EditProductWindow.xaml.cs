using System;
using System.Windows;
using MySql.Data.MySqlClient;
using DubaiJewelManufacturing.Models;

namespace DubaiJewelManufacturing
{
    public partial class EditProductWindow : Window
    {
        private ProductModel _product;
        private DatabaseHelper db = new DatabaseHelper();

        public EditProductWindow(ProductModel product)
        {
            InitializeComponent();
            _product = product;

            // Current values textbox me set karo
            txtCurrentProductType.Text = _product.ProductType;
            txtCurrentCarat.Text = _product.Carat;
        }

        // ✅ Save button click
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newProductType = string.IsNullOrWhiteSpace(txtNewProductType.Text)
                                        ? _product.ProductType
                                        : txtNewProductType.Text;
            string newCarat = string.IsNullOrWhiteSpace(txtNewCarat.Text)
                                        ? _product.Carat
                                        : txtNewCarat.Text;

            using (var conn = db.GetConnection())
            {
                conn.Open();

                string updateQuery = @"UPDATE products 
                               SET ProductType=@type, Carat=@carat 
                               WHERE ProductType=@oldType";

                using (var cmd = new MySqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@type", newProductType);
                    cmd.Parameters.AddWithValue("@carat", newCarat);
                    cmd.Parameters.AddWithValue("@oldType", _product.ProductType);

                    cmd.ExecuteNonQuery();
                }
            }

            // ✅ Update the in-memory object
            _product.ProductType = newProductType;
            _product.Carat = newCarat;

            // ✅ DialogResult true set karo
            this.DialogResult = true;

            MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        // ✅ Delete button click
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var conn = db.GetConnection())
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM products WHERE ProductType=@type";
                    using (var cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@type", _product.ProductType);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product deleted successfully!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        // ✅ Cancel button click
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
