﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceProcess;
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
using NamedPipeWrapper;

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

        private ServiceController service;
        private PerformanceLogger performanceLogger;

        private readonly string pipeName = "ProjektGraficznyPipe";
        private NamedPipeServer<PipeMessage> pipeServer;

        public MainWindow()
        {
            InitializeComponent();

            OnStart();
        }

        private void OnStart()
        {
            Settings.Load();
            Utils.BuildIterationColorsCache();
            CreatePipe();

            RendererImage.Source = rendererBitmap;

            rendererBitmap = new WriteableBitmap(Settings.renderWidth, Settings.renderHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            colorBuffer = new byte[Settings.renderWidth * Settings.renderHeight * 4];

            performanceLogger = new PerformanceLogger(this);
            StartService();

            fractalChoiceItems = new MenuItem[] { FractalChoice0, FractalChoice1 };
            drawingChoiceItems = new MenuItem[] { DrawingChoice0, DrawingChoice1, DrawingChoice2 };

            SizeChanged += MainWindow_SizeChanged;
            KeyDown += MainWindow_KeyDown;

            DrawingChoice_OnClick(drawingChoiceItems[(int)Settings.selectedDrawingMode], null);
            FractalChoice_OnClick(fractalChoiceItems[(int)Settings.selectedFractalType], null);

            this.Width = Settings.defaultWindowWidth;
            this.Height = Settings.defaultWindowHeight;
        }

        private void CreatePipe()
        {
            pipeServer = new NamedPipeServer<PipeMessage>(pipeName);

            // server.ClientConnected += delegate (NamedPipeConnection<PipeMessage, PipeMessage> conn)
            // {
            //     Console.WriteLine("Client {0} is now connected!", conn.Id);
            //     conn.PushMessage(new PipeMessage { Text: "Welcome!" });
            // };

            // server.ClientMessage += delegate (NamedPipeConnection<PipeMessage, PipeMessage> conn, PipeMessage message)
            // {
            //     Console.WriteLine("Client {0} says: {1}", conn.Id, message.Text);
            // };

            // Start up the server asynchronously and begin listening for connections.
            // This method will return immediately while the server runs in a separate background thread.
            pipeServer.Start();
        }

        public void SendMessageToClient(int messageType, string content)
        {
            pipeServer.PushMessage(new PipeMessage(){messageType = messageType, content = content});
        }

        private void StartService()
        {
            try
            {
                service = new ServiceController("PerformanceMonitoringService");
                service.Start();  
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }      
        }

        private void StopService()
        {
            try
            {
                service?.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }       
        }

        private void Draw()
        {

            performanceLogger.Start(Settings.selectedFractalType, Settings.selectedDrawingMode);

            Fractal fractal = null;

            switch (Settings.selectedFractalType)
            {
                case FractalType.Mandelbrot:
                    fractal = new Mandelbrot(colorBuffer, Settings.renderWidth, Settings.renderHeight, Settings.maxIterations);
                    break;
                case FractalType.Julia:
                    fractal = new Julia(colorBuffer, Settings.renderWidth, Settings.renderHeight, Settings.maxIterations);
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
            UpdateTransformationText();

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

        private void UpdateTransformationText()
        {
            TransformationText.Text = $"Position: ({offsetX}; {offsetY})\nZoom: {zoom}";
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
            performanceLogger.WriteLogsToService();
            StopService();
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ExportChoice_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            switch (menuItem.Name)
            {
                case "ExportChoice0":
                    performanceLogger.ExportLogsToCsv(0);
                    break;
                case "ExportChoice1":
                    performanceLogger.ExportLogsToCsv(1);
                    break;
                case "ExportChoice2":
                    performanceLogger.ExportLogsToCsv(2);
                    break;
            }
        }

        #endregion

    }
}
