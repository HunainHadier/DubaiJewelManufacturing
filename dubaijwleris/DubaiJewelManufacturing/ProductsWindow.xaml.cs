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
    /// Interaction logic for ProductsWindow.xaml
    /// </summary>
    public partial class ProductsWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();

        // Top of your class
        private List<ProductModel> importedProducts = new List<ProductModel>();


        private string _userName;

        public ProductsWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;
            HeaderCtrl.UserName = _userName;
            All_Products();
        }

        private void All_Products()
        {
            try
            {
                List<ProductModel> products = new List<ProductModel>();

                using (var conn = db.GetConnection())
                {
                    conn.Open();

                    string query = @"SELECT ProductType, Carat FROM products";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new ProductModel
                            {
                                ProductType = reader["ProductType"].ToString(),
                                Carat = reader["Carat"].ToString()
                            });
                        }
                    }
                }

                // DataGrid bind
                ProductsDataGrid.ItemsSource = null;
                ProductsDataGrid.ItemsSource = products;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            AdminMainWindow second = new AdminMainWindow(_userName);
            second.Show();       // Show the next window
            this.Close();        // Optional: close current one
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var products = ProductsDataGrid.ItemsSource as List<ProductModel>;
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("No data to export!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "ProductsData.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Products");

                        // Headers
                        worksheet.Cell(1, 1).Value = "ProductType";
                        worksheet.Cell(1, 2).Value = "Carat";

                        // Data
                        int row = 2;
                        foreach (var product in products)
                        {
                            worksheet.Cell(row, 1).Value = product.ProductType;
                            worksheet.Cell(row, 2).Value = product.Carat;
                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
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
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        DataTable dt = result.Tables[0]; // First sheet

                        importedProducts.Clear(); // old data clear

                        foreach (DataRow row in dt.Rows)
                        {
                            importedProducts.Add(new ProductModel
                            {
                                ProductType = row["ProductType"].ToString(),
                                Carat = row["Carat"].ToString()
                            });
                        }

                        // DataGrid me show karein
                        ProductsDataGrid.ItemsSource = null;
                        ProductsDataGrid.ItemsSource = importedProducts;

                        MessageBox.Show("Excel Imported Successfully! Click 'Save' to store in Database.",
                                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (importedProducts.Count == 0)
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

                    foreach (var product in importedProducts)
                    {
                        string query = @"INSERT INTO products (ProductType, Carat) 
                                 VALUES (@type, @carat)";

                        using (var cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@type", product.ProductType);
                            cmd.Parameters.AddWithValue("@carat", product.Carat);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Data Saved to Database Successfully!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                importedProducts.Clear();
                All_Products(); // Refresh DataGrid from DB
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // ✅ Get the ProductModel of the clicked row
            var product = (sender as Button)?.DataContext as ProductModel;
            if (product == null) return;

            // ✅ Open modal and pass selected product
            var editWindow = new EditProductWindow(product)
            {
                Owner = this
            };

            // Show modal dialog
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // ✅ DataGrid ke liye UI refresh
                ProductsDataGrid.Items.Refresh();
            }
        }



    }
}
