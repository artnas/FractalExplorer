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

        private double zoom = 0.004;

        private double offsetX = -0.7;
        private double offsetY = -0.35;

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

            RendererImage.Source = rendererBitmap;

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            DrawFractal();
        }

        private void DrawFractal()
        {

            Mandelbrot mandelBrot = new Mandelbrot(colorBuffer, Settings.renderWidth, Settings.renderHeight);

            mandelBrot.DrawOnGpu(offsetX, offsetY, zoom);

            // Julia fractal = new Julia(colorBuffer, Settings.renderWidth, Settings.renderHeight);
            //
            // fractal.DrawOnSingleThread(offsetX, offsetY, zoom);

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

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                default: return;
                case Key.W: offsetY -= 4 * zoom; break;
                case Key.S: offsetY += 4 * zoom; break;
                case Key.A: offsetX -= 4 * zoom; break;
                case Key.D: offsetX += 4 * zoom; break;
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
