using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fractals;

namespace ProjektGraficzny
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WriteableBitmap rendererBitmap;
        private byte[] colorBuffer;

        private MenuItem[] drawingChoiceItems;
        private MenuItem[] fractalChoiceItems;

        private double zoom = 1;
        private double offsetX = 0;
        private double offsetY = 0;

        private PerformanceLogger performanceLogger;

        public MainWindow()
        {
            InitializeComponent();

            OnStart();
        }

        private void OnStart()
        {
            Settings.Load();
            Utils.BuildIterationColorsCache();

            rendererBitmap = new WriteableBitmap(Settings.renderWidth, Settings.renderHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            colorBuffer = new byte[Settings.renderWidth * Settings.renderHeight * 4];

            performanceLogger = new PerformanceLogger();

            RendererImage.Source = rendererBitmap;
            fractalChoiceItems = new MenuItem[] { FractalChoice0, FractalChoice1 };
            drawingChoiceItems = new MenuItem[] { DrawingChoice0, DrawingChoice1, DrawingChoice2 };

            SizeChanged += MainWindow_SizeChanged;
            KeyDown += MainWindow_KeyDown;

            DrawingChoice_OnClick(drawingChoiceItems[(int)Settings.selectedDrawingMode], null);
            FractalChoice_OnClick(fractalChoiceItems[(int)Settings.selectedFractalType], null);

            Draw();
        }

        private void Draw()
        {

            performanceLogger.Start(Settings.selectedFractalType, Settings.selectedDrawingMode);

            Fractal fractal = null;

            switch (Settings.selectedFractalType)
            {
                case FractalType.Mandelbrot:
                    fractal = new Mandelbrot(colorBuffer, Settings.renderWidth, Settings.renderHeight);
                    break;
                case FractalType.Julia:
                    fractal = new Julia(colorBuffer, Settings.renderWidth, Settings.renderHeight);
                    break;
                default:
                    throw new Exception("Nieprawidłowy typ fraktala: " + Settings.selectedFractalType);
            }

            switch (Settings.selectedDrawingMode)
            {
                case DrawingMode.CpuSingle:
                    fractal.DrawOnSingleThread(offsetX, offsetY, zoom);
                    break;
                case DrawingMode.CpuMulti:
                    fractal.DrawOnMultipleThreads(offsetX, offsetY, zoom);
                    break;
                case DrawingMode.Gpu:
                    fractal.DrawOnGpu(offsetX, offsetY, zoom);
                    break;
                default:
                    throw new Exception("Nieprawidłowy typ rysowania: " + Settings.selectedDrawingMode);
            }

            performanceLogger.Stop(CountTotalIterations());

            UpdateBitmap();

        }

        private void UpdateBitmap()
        {

            rendererBitmap.WritePixels(
                new Int32Rect(0, 0, Settings.renderWidth, Settings.renderHeight),
                colorBuffer,
                Settings.renderWidth * sizeof(int),
                0);

        }

        private long CountTotalIterations()
        {
            long sum = 0;

            for (int i = 3; i < Settings.renderWidth * Settings.renderHeight * 4; i += 4)
            {
                sum += colorBuffer[i];
            }

            return sum;
        }

        private void ResetOffsetsAndZoom()
        {
            offsetX = offsetY = 0;
            zoom = 1;
        }

    #region Event Handlers

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                default: return;
                case Key.W: offsetY -= zoom; break;
                case Key.S: offsetY += zoom; break;
                case Key.A: offsetX -= zoom; break;
                case Key.D: offsetX += zoom; break;
                case Key.Q: zoom *= 0.85; break;
                case Key.E: zoom *= 1.15; break;
                case Key.R: ResetOffsetsAndZoom(); break;
            }

            Draw();

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Settings.renderWidth = (int)MainGrid.ActualWidth;
            Settings.renderHeight = (int)MainGrid.ActualHeight - 20;

            Settings.Save();

            colorBuffer = new byte[Settings.renderWidth * Settings.renderHeight * 4];
            rendererBitmap = new WriteableBitmap(Settings.renderWidth, Settings.renderHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            RendererImage.Source = rendererBitmap;

            ResetOffsetsAndZoom();

            Draw();

        }

        private void DrawingChoice_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem) sender;

            for (int i = 0; i < drawingChoiceItems.Length; i++)
            {
                var item = drawingChoiceItems[i];

                if (item != menuItem)
                {
                    item.IsChecked = false;
                    item.IsEnabled = true;
                }
                else
                {
                    item.IsChecked = true;
                    item.IsEnabled = false;

                    Settings.selectedDrawingMode = (DrawingMode) i;
                }
            }

            Draw();
        }

        private void FractalChoice_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            for (int i = 0; i < fractalChoiceItems.Length; i++)
            {
                var item = fractalChoiceItems[i];

                if (item != menuItem)
                {
                    item.IsChecked = false;
                    item.IsEnabled = true;
                }
                else
                {
                    item.IsChecked = true;
                    item.IsEnabled = false;

                    Settings.selectedFractalType = (FractalType)i;

                    ResetOffsetsAndZoom();
                }
            }

            Draw();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Save();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        #endregion

    }
}
