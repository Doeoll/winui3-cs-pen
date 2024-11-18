using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI;
using Windows.UI;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private Polyline currentLine;
        private bool isDrawing;
        private List<Line> _lines = new List<Line>(); // 그린 선을 저장할 리스트
        private Color selectedColor = Colors.Black;
        private double _currentThickness = 2; // 기본 펜 굵기

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isDrawing = true;

            // 새로운 선 생성
            currentLine = new Polyline
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = _currentThickness
            };

            // 선을 캔버스에 추가
            DrawingCanvas.Children.Add(currentLine);

            // 시작 지점 추가
            currentLine.Points.Add(e.GetCurrentPoint(DrawingCanvas).Position);
        }

        private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isDrawing)
            {
                // 마우스 이동 경로 추가
                currentLine.Points.Add(e.GetCurrentPoint(DrawingCanvas).Position);
            }
        }

        private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isDrawing = false;
        }

        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string colorName = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            selectedColor = colorName switch
            {
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                _ => Colors.Black,
            };
        }

        private void ThicknessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _currentThickness = e.NewValue;
        }

        // 저장 버튼 클릭 이벤트
        private async void SaveCanvasContent(object sender, RoutedEventArgs e)
        {
            var renderTarget = new RenderTargetBitmap();
            await renderTarget.RenderAsync(DrawingCanvas);

            var pixels = await renderTarget.GetPixelsAsync();
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "drawing",
                FileTypeChoices = { { "PNG Image", new[] { ".png" } } }
            };

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                        (uint)renderTarget.PixelWidth,
                        (uint)renderTarget.PixelHeight,
                        96, 96,
                        pixels.ToArray());
                    await encoder.FlushAsync();
                }
            }
        }
        private async void LoadCanvasContent(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".png" }
            };

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    var image = new Image { Source = bitmap };
                    DrawingCanvas.Children.Clear();
                    DrawingCanvas.Children.Add(image);
                }
            }
        }
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
        }

    }
}
