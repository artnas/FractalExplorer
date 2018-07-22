using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals
{
    public abstract class Fractal
    {

        protected byte[] buffer;
        protected int width, height;
        protected float aspectRatio = 1;
        protected FractalType type;

        protected readonly double xBaseOffset = 0;
        protected readonly double yBaseOffset = 0;
        protected readonly double zoomMultiplier = 1;
        protected readonly double offsetMultiplier = 1;

        public abstract void DrawOnSingleThread(double xOffset, double yOffset, double zoom);
        public abstract void DrawOnMultipleThreads(double xOffset, double yOffset, double zoom);
        public abstract void DrawOnGpu(double xOffset, double yOffset, double zoom);
        public abstract int GetValue(int index, double x, double y);

    }
}
