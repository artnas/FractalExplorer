using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals
{
    public abstract class Fractal
    {

        internal byte[] buffer;
        internal int width, height;
        internal float aspectRatio = 1;

        public abstract void DrawOnSingleThread(double xOffset, double yOffset, double zoom);
        public abstract void DrawOnMultipleThreads(double xOffset, double yOffset, double zoom);
        public abstract void DrawOnGpu(double xOffset, double yOffset, double zoom);
        public abstract int GetValue(int index, double x, double y);

    }
}
