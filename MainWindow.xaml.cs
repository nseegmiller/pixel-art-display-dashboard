using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pixel_Art_Display_Dashboard
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly Divoom divoom = new();
        public MainWindow()
        {
            this.InitializeComponent();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // Set the desired size using Resize()
            appWindow.Resize(new SizeInt32(800, 600));
        }

        private async Task ShowException(Exception ex)
        {
            // Handle any errors (e.g., network issues)
            ContentDialog dialog = new()
            {
                Title = "Error",
                Content = $"An error occurred: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void SearchPanels_Click(object sender, RoutedEventArgs e)
        {
            try
            {          
                // Disable the button to prevent multiple clicks
                ((Button)sender).IsEnabled = false;

                List<Device> devices = await Divoom.GetPanels();
                DevicesBox.Items.Clear();
                foreach (Device device in devices)
                {
                    if (device == null) continue;
                    DevicesBox.Items.Add(device.DevicePrivateIP);
                }
                DevicesBox.IsEnabled = true;
                // Select the first item
                if (DevicesBox.Items.Count > 0)
                {
                    DevicesBox.SelectedIndex = 0; // Select the first item (index 0)
                    divoom.IPAddress = DevicesBox.SelectedItem.ToString();
                }
            }
            catch (Exception ex)
            {
                await ShowException(ex);
            }
            finally
            {
                // Re-enable the button
                ((Button)sender).IsEnabled = true;
            }
        }
        private void DevicesBox_Changed(object sender, RoutedEventArgs e)
        {
            divoom.IPAddress = DevicesBox.SelectedItem.ToString();
        }

        private async void PickImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            PickImageOutputTextBlock.Text = "";

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            // See the sample code below for how to make the window accessible from the App class.
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            AppWindow window = AppWindow.GetFromWindowId(windowId);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PickImageOutputTextBlock.Text = file.Name;
                SendImageButton.IsEnabled = true;
            }
            else
            {
                PickImageOutputTextBlock.Text = "Operation cancelled.";
            }
        }

        private async void SendImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await divoom.SendFile(PickImageOutputTextBlock.Text);
            }
            catch (Exception ex)
            {
                await ShowException(ex);
            }
        }
    }
}
