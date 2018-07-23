using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals;

namespace ProjektGraficzny
{
    public class PerformanceLogger
    {

        private readonly Stopwatch stopWatch;

        private List<PerformanceLoggerEntry> entries;

        private FractalType currentFractalType;
        private DrawingMode currentDrawingMode;

        private MainWindow mainWindow;

        public PerformanceLogger(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            stopWatch = new Stopwatch();
            entries = new List<PerformanceLoggerEntry>();
        }

        public void Start(FractalType fractalType, DrawingMode drawingMode)
        {
            stopWatch.Restart();

            this.currentFractalType = fractalType;
            this.currentDrawingMode = drawingMode;
        }

        public void Stop(long totalIterations)
        {
            stopWatch.Stop();

            int timeTicks = (int)stopWatch.ElapsedTicks;

            PerformanceLoggerEntry newEntry = new PerformanceLoggerEntry(currentFractalType, currentDrawingMode, totalIterations, timeTicks);
            entries.Add(newEntry);

            Console.WriteLine(totalIterations + " in " + timeTicks + " ticks, rate: " + ((double)totalIterations/timeTicks));

            //mainWindow.SendMessageToClient(0, newEntry.ToString());
        }

        public void WriteLogsToService()
        {
            String s = "";
            for (int a = 0; a < 2; a++)
            {
                FractalType fractalType = (FractalType) a;
                for (int b = 0; b < 3; b++)
                {
                    DrawingMode drawingMode = (DrawingMode)b;

                    int count = 0;
                    ulong totalIterations = 0;
                    ulong totalTicks = 0;

                    foreach (var entry in entries)
                    {
                        if (entry.drawingMode == drawingMode && entry.fractalType == fractalType)
                        {
                            // pomin pierwszy rekord GPU
                            if (drawingMode == DrawingMode.Gpu && count == 0)
                            {
                                count++;
                                continue;
                            }

                            totalIterations += (ulong) entry.totalIterations;
                            totalTicks += (ulong) entry.timeTicks;

                            count++;
                        }
                    }

                    if (drawingMode == DrawingMode.Gpu)
                        count--;

                    if (count > 0)
                    {
                        double performance = (double) totalIterations / totalTicks;
                        s += $"Fraktal: {fractalType}, Tryb: {drawingMode}, Współczynnik wydajności: {performance}#";
                    }
                }
            }

            if (s != "")
            {
                mainWindow.SendMessageToClient(1, s);
            }
        }

    }
}
