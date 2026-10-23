using ClosedXML.Excel;
using DubaiJewelManufacturing.Models;
using Microsoft.Win32;
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
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        private string _userName;
        public DataWindow(string userName)
        {
            InitializeComponent();
            _userName = userName;
            HeaderCtrl.UserName = _userName;
            LoadProductsData();
        }



        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            // ✅ DataGrid ke ItemsSource ko List<DataModel> me cast karein
            var productData = ProductsDataGrid.ItemsSource as List<DataModel>;
            if (productData == null || productData.Count == 0)
            {
                MessageBox.Show("No data to export!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // ✅ Save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "ProductData.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("ProductData");

                        // ✅ Headers
                        worksheet.Cell(1, 1).Value = "ProductType";
                        worksheet.Cell(1, 2).Value = "Carat";
                        worksheet.Cell(1, 3).Value = "Weight";
                        worksheet.Cell(1, 4).Value = "BarcodeFormat";
                        worksheet.Cell(1, 5).Value = "SerialNumber";
                        worksheet.Cell(1, 6).Value = "Date";
                        worksheet.Cell(1, 7).Value = "Time";

                        // ✅ Data Fill
                        int row = 2;
                        foreach (var item in productData)
                        {
                            worksheet.Cell(row, 1).Value = item.ProductType;
                            worksheet.Cell(row, 2).Value = item.Carat;
                            worksheet.Cell(row, 3).Value = item.Weight;
                            worksheet.Cell(row, 4).Value = item.Barcodeformat;
                            worksheet.Cell(row, 5).Value = item.SerialNumber;
                            worksheet.Cell(row, 6).Value = item.Date;
                            worksheet.Cell(row, 7).Value = item.Time;
                            row++;
                        }

                        // ✅ Auto adjust columns
                        worksheet.Columns().AdjustToContents();

                        // ✅ Save file
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

        private void LoadProductsData()
        {
            List<DataModel> productsData = new List<DataModel>();

            using (var conn = db.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                SELECT ProductType, Carat, Weight, BarcodeFormat, SerialNumber, Date, Time
                FROM product_data";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productsData.Add(new DataModel
                            {
                                ProductType = reader["ProductType"].ToString(),
                                Carat = reader["Carat"].ToString(),
                                Weight = reader["Weight"].ToString(),
                                Barcodeformat = reader["BarcodeFormat"].ToString(),
                                SerialNumber = reader["SerialNumber"].ToString(),
                                Date = Convert.ToDateTime(reader["Date"]).ToString("yyyy-MM-dd"),
                                Time = reader["Time"].ToString()
                            });
                        }
                    }

                    ProductsDataGrid.ItemsSource = productsData;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error loading products: " + ex.Message);
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
