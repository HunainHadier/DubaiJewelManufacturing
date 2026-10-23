using DubaiJewelManufacturing.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using System.Windows.Documents;
using System.IO.Ports;
using System.Windows.Threading;


namespace DubaiJewelManufacturing
{
    public partial class BarCodeWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        private bool isReadingEnabled = false;

        private string selectedProductType = "";
        private string selectedCarat = "";
        private SerialPort weightPort;
        private string incomingWeightData = "";

        private string _roleId;
        private string _userName;

        private WriteableBitmap currentBarcodeBitmap; // store last barcode for print

        //public BarCodeWindow(string roleId = "", string userName = "")
        //{
        //    InitializeComponent();
        //    LoadProductsToComboBox();

        //    _roleId = roleId;
        //    _userName = userName;

        //    // ✅ Set karte hi header me propagate hoga
        //    BarcodeHeader.UserName = _userName;

        //    WeightTextBox.TextChanged += WeightTextBox_TextChanged;

        //    ReturnToMainMenuButton.Visibility = (_roleId == "1")
        //        ? Visibility.Visible
        //        : Visibility.Collapsed;
        //}

        public BarCodeWindow(string roleId = "", string userName = "")
        {
            InitializeComponent();
            LoadProductsToComboBox();

            _roleId = roleId;
            _userName = userName;

            BarcodeHeader.UserName = _userName;

            WeightTextBox.TextChanged += WeightTextBox_TextChanged;

            ReturnToMainMenuButton.Visibility = (_roleId == "1")
                ? Visibility.Visible
                : Visibility.Collapsed;

            StartWeightReading(); // ✅ Start reading from machine
        }
        private void StartWeightReading()
        {
            try
            {
                weightPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One); // 🔁 Adjust COM port & baud rate
                weightPort.DataReceived += WeightPort_DataReceived;
                weightPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Weight machine not connected: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WeightPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string rawData = weightPort.ReadLine(); // Or use ReadExisting()

                // Optional: Parse data if needed
                string cleanData = rawData.Trim();

                Dispatcher.Invoke(() =>
                {
                    if (isReadingEnabled)
                    {
                        WeightTextBox.Text = cleanData;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error reading weight: " + ex.Message);
                });
            }
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (weightPort != null && weightPort.IsOpen)
            {
                weightPort.Close();
            }

            base.OnClosing(e);
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

        private void ManualSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ManualSelectionComboBox.SelectedItem is ProductModel selectedProduct)
            {
                selectedProductType = selectedProduct.ProductType;
                selectedCarat = selectedProduct.Carat;

                ProductTypeTextBox.Text = selectedProduct.ProductType;
                CaratTextBox.Text = selectedProduct.Carat + "K";

                UpdatePreviewAndBarcode();
            }
        }

        private void WeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewAndBarcode();
        }

        private void UpdatePreviewAndBarcode()
        {
            if (!string.IsNullOrWhiteSpace(selectedProductType) &&
                !string.IsNullOrWhiteSpace(selectedCarat) &&
                !string.IsNullOrWhiteSpace(WeightTextBox.Text))
            {
                // Update UI
                CaratText.Text = $"{selectedCarat}K";
                BarcodeCodeText.Text = selectedProductType;
                WeightText.Text = $"{WeightTextBox.Text}g";

                // Generate barcode value
                string barcodeValue = $"{selectedCarat}K-{selectedProductType}-{WeightTextBox.Text}g";
                GenerateBarcode(barcodeValue);
            }
        }

        private void GenerateBarcode(string text)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 45,
                    Width = 220,
                    Margin = 2
                }
            };

            var pixelData = writer.Write(text);

            var bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(
                new Int32Rect(0, 0, pixelData.Width, pixelData.Height),
                pixelData.Pixels, pixelData.Width * 4, 0);

            BarcodeImage.Source = bitmap;
            currentBarcodeBitmap = bitmap; // save for printing
        }

        private void PrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (currentBarcodeBitmap == null)
            {
                MessageBox.Show("Please generate a barcode first.", "No Barcode", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FlowDocument doc = new FlowDocument
            {
                PageWidth = 300,
                PageHeight = 150,
                PagePadding = new Thickness(10)
            };

            BlockUIContainer container = new BlockUIContainer();

            StackPanel panel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };

            panel.Children.Add(new TextBlock
            {
                Text = "Dubai Jewel | جوهرة دبي",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            panel.Children.Add(new Image
            {
                Source = currentBarcodeBitmap,
                Width = 220,
                Height = 45,
                Margin = new Thickness(0, 5, 0, 5)
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"{CaratText.Text}   {BarcodeCodeText.Text}   {WeightText.Text}",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            container.Child = panel;
            doc.Blocks.Add(container);

            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                doc.Name = "BarcodeDoc";
                printDlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Printing Barcode");
            }
        }


       
        private void ReadingEnabledButton_Click(object sender, RoutedEventArgs e)
        {
            isReadingEnabled = !isReadingEnabled;

            if (isReadingEnabled)
            {
                ReadingEnabledButton.Content = "🔍 Reading is Enabled";
                ReadingEnabledButton.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ReadingEnabledButton.Content = "🔍 Reading is Disabled";
                ReadingEnabledButton.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void returntomainmenu(object sender, RoutedEventArgs e)
        {
            AdminMainWindow main = new AdminMainWindow(_userName);
            main.Show();
            this.Close();
        }
    }
}
