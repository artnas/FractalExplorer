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

        public abstract void DrawCpuSingle(double xOffset, double yOffset, double zoom);
        public abstract void DrawCpuMulti(double xOffset, double yOffset, double zoom);
        public abstract void DrawGpu(double xOffset, double yOffset, double zoom);
        public abstract int GetValue(int index, double x, double y, int maxIteration);

    }
}
