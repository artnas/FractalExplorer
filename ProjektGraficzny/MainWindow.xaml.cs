using System;
using System.Collections.Generic;
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

            this.SizeChanged += MainWindow_SizeChanged;
            this.KeyDown += MainWindow_KeyDown;

            DrawFractal();
        }

        private void DrawFractal()
        {

            performanceLogger.Start(Settings.selectedFractalType, Settings.selectedDrawingMode);

            Fractal fractal = null;

            Settings.selectedFractalType = FractalType.Julia;
            Settings.selectedDrawingMode = DrawingMode.Gpu;

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

            DrawColorBuffer();

        }

        private void DrawColorBuffer()
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
            }

            DrawFractal();

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // calculates incorrect when window is maximized
            //ContactsGrid.Width = this.ActualWidth - 20;
            //ContactsGrid.Height = this.ActualHeight - 190;

            Settings.renderWidth = (int)MainGrid.ActualWidth;
            Settings.renderHeight = (int)MainGrid.ActualHeight;

            Settings.Save();

            colorBuffer = new byte[Settings.renderWidth * Settings.renderHeight * 4];
            rendererBitmap = new WriteableBitmap(Settings.renderWidth, Settings.renderHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            RendererImage.Source = rendererBitmap;

            DrawFractal();
           
        }

    }
}
