using DubaiJewelManufacturing.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;

namespace DubaiJewelManufacturing
{
    public partial class BarcodeSecondWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        private bool isReadingEnabled = false;
        private string _userName;

        private string selectedProductType = "";
        private string selectedCarat = "";
        private WriteableBitmap currentBarcodeBitmap;

        public BarcodeSecondWindow(string userName = "")
        {
            InitializeComponent();
            LoadProductsToComboBox();

            _userName = userName;               // ✅ Assign karo
            BarcodeHeader.UserName = _userName;
            WeightTextBox.TextChanged += (s, e) => UpdatePreviewAndBarcode();
        }

        private void LoadProductsToComboBox()
        {
            try
            {
                var products = new List<ProductModel>();

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

                ManualSelectionComboBox.ItemsSource = products;
                ManualSelectionComboBox.DisplayMemberPath = "ProductType";
                ManualSelectionComboBox.SelectedIndex = -1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdatePreviewAndBarcode()
        {
            // Collect values
            string weight = WeightTextBox.Text.Trim();
            string length = LengthTextBox.Text.Trim();
            string quantity = QuantityTextBox.Text.Trim();
            string carat = CaratTextBox.Text.Trim();
            string productType = ProductTypeTextBox.Text.Trim();

            // Update Arabic preview labels
            WeightText.Text = string.IsNullOrEmpty(weight) ? "-" : weight + "g";
            CaratText.Text = string.IsNullOrEmpty(carat) ? "-" : carat;
            LengthText.Text = string.IsNullOrEmpty(length) ? "-" : length + "cm";
            QuantityText.Text = string.IsNullOrEmpty(quantity) ? "-" : quantity;

            // Combine for barcode
            string barcodeData = $"{productType}-{carat}-{weight}-{length}-{quantity}";
            GenerateBarcode(barcodeData);
        }
        private void GenerateBarcode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 200,  // Match karo XAML se
                    Width = 180,
                    Margin = 2
                }
            };

            var pixelData = writer.Write(text);

            var bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, 96, 96, PixelFormats.Pbgra32, null);
            bitmap.WritePixels(
                new Int32Rect(0, 0, pixelData.Width, pixelData.Height),
                pixelData.Pixels, pixelData.Width * 4, 0
            );

            BarcodeImage.Source = bitmap;
            currentBarcodeBitmap = bitmap;
        }

        private void PrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (BarcodePreviewCanvas == null)
            {
                MessageBox.Show("No barcode to print.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                // Fixed print size
                double printWidth = 446;
                double printHeight = 100;

                // Save original scale
                Transform originalScale = BarcodePreviewCanvas.LayoutTransform;

                // Calculate scale factor to fit 446x100
                double scaleX = printWidth / BarcodePreviewCanvas.ActualWidth;
                double scaleY = printHeight / BarcodePreviewCanvas.ActualHeight;

                // Apply scaling
                BarcodePreviewCanvas.LayoutTransform = new ScaleTransform(scaleX, scaleY);

                // Arrange canvas for printing
                Size printSize = new Size(printWidth, printHeight);
                BarcodePreviewCanvas.Measure(printSize);
                BarcodePreviewCanvas.Arrange(new Rect(new Point(0, 0), printSize));

                // Print the visual
                printDlg.PrintVisual(BarcodePreviewCanvas, "Printing Barcode");

                // Reset scale
                BarcodePreviewCanvas.LayoutTransform = originalScale;
            }
        }

        private void CheckManualSelectionComplete()
        {
            bool isComplete = ManualSelectionComboBox.SelectedItem != null
                              && !string.IsNullOrWhiteSpace(WeightTextBox.Text)
                              && !string.IsNullOrWhiteSpace(LengthTextBox.Text)
                              && !string.IsNullOrWhiteSpace(QuantityTextBox.Text);

            ReadingEnabledButton.IsEnabled = isComplete;
        }


        private void returntomainmenu(object sender, RoutedEventArgs e)
        {
            AdminMainWindow second = new AdminMainWindow(_userName);
            second.Show();
            this.Close();
        }


        private void ManualSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ManualSelectionComboBox.SelectedItem is ProductModel selectedProduct)
            {
                ProductTypeTextBox.Text = selectedProduct.ProductType;
                CaratTextBox.Text = selectedProduct.Carat + "K";
                UpdatePreviewAndBarcode();
                CheckManualSelectionComplete();
            }
        }

        private void WeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewAndBarcode();
            CheckManualSelectionComplete();
        }

        private void LengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewAndBarcode();
            CheckManualSelectionComplete();
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewAndBarcode();
            CheckManualSelectionComplete();
        }

    }
}
