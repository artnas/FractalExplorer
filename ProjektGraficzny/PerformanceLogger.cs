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

        public PerformanceLogger()
        {
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
        }

    }
}
