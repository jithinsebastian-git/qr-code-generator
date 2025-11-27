using Microsoft.Maui.Controls;
using QRCoder;
using System;
using System.IO;
#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace QR_Code_Generator
{
    public partial class MainPage : ContentPage
    {
        // Holds the last generated PNG bytes for saving/downloading
        private byte[]? _lastQrPng;

        public MainPage()
        {
            InitializeComponent();
        }

        private void GenerateBtn_Clicked(object sender, EventArgs e)
        {
            string qrText = InputText?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(qrText))
            {
                // Use MAUI alert instead of MessageBox
                _ = DisplayAlert("Input required", "Please enter text to generate QR code.", "OK");
                return;
            }

            try
            {
#if WINDOWS
                // Generate QR code bitmap
                using QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                using QRCode qrCode = new QRCode(qrCodeData);
                using Bitmap qrBitmap = qrCode.GetGraphic(20);

                // Convert bitmap to PNG bytes
                using MemoryStream ms = new MemoryStream();
                qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                _lastQrPng = ms.ToArray();

                // Set the Image control source from the PNG bytes
                QrImage.Source = ImageSource.FromStream(() => new MemoryStream(_lastQrPng));
                ResultPane.IsVisible = true;
#else
                // Not supported on this platform
                _ = DisplayAlert("Unsupported", "QR code generation is only supported on Windows in this version.", "OK");
                _lastQrPng = null;
                QrImage.Source = null;
#endif
            }
            catch (Exception ex)
            {
                // Show failure to user
                _ = DisplayAlert("Error", $"Failed to generate QR code: {ex.Message}", "OK");
            }
        }

        // Hook this to your Save button (e.g., SaveBtn Clicked)
        private async void SaveBtn_Clicked(object sender, EventArgs e)
        {
            if (_lastQrPng == null || _lastQrPng.Length == 0)
            {
                await DisplayAlert("No image", "Please generate a QR code before saving.", "OK");
                return;
            }

            try
            {
                string fileName = $"qrcode_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                // Save to user's Documents folder (cross-platform reasonable default)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrWhiteSpace(documentsPath))
                {
                    // Fallback to app data directory if Documents isn't available
                    documentsPath = FileSystem.AppDataDirectory;
                }

                string fullPath = Path.Combine(documentsPath, fileName);

                File.WriteAllBytes(fullPath, _lastQrPng);

                await DisplayAlert("Saved", $"QR code saved to:\n{fullPath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Save failed", $"Unable to save QR code: {ex.Message}", "OK");
            }
        }

        private void ClearBtn_Clicked(object sender, EventArgs e)
        {           
            ResultPane.IsVisible = false;
            InputText.Text = "";
            _lastQrPng = null;
        }
    }
}
