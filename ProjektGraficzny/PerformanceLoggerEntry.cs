using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals;

namespace ProjektGraficzny
{
    public class PerformanceLoggerEntry
    {

        public readonly FractalType fractalType;
        public readonly DrawingMode drawingMode;
        public readonly long totalIterations;
        public readonly int timeTicks;

        public PerformanceLoggerEntry(FractalType fractalType, DrawingMode drawingMode, long totalIterations, int timeTicks)
        {
            this.fractalType = fractalType;
            this.drawingMode = drawingMode;
            this.totalIterations = totalIterations;
            this.timeTicks = timeTicks;
        }

        public override string ToString()
        {
            return $"{fractalType}{Settings.csvSeparator}{drawingMode}{Settings.csvSeparator}{totalIterations}{Settings.csvSeparator}{timeTicks}";
        }
    }
}
