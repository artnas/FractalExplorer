using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;

namespace Fractals
{
    public class Julia : Fractal
    {

        protected new FractalType type = FractalType.Julia;
        protected readonly int maxIterations = 127;

        protected readonly double cX = -0.7, cY = 0.27015;

        protected new readonly double zoomMultiplier = 0.04;
        protected new readonly double offsetMultiplier = 1;
        protected new readonly double xBaseOffset = 0;
        protected new readonly double yBaseOffset = -4;

        public Julia(byte[] buffer, int width, int height, int maxIterations)
        {
            this.buffer = buffer;
            this.width = width;
            this.height = height;

            this.aspectRatio = (float) width / height;

            this.maxIterations = maxIterations;
        }

        public override void DrawOnSingleThread(double xOffset, double yOffset, double zoom)
        {
            xOffset += xBaseOffset;
            yOffset += yBaseOffset;
            xOffset *= offsetMultiplier;
            yOffset *= offsetMultiplier;
            zoom *= zoomMultiplier;

            for (int i = 0; i < width * height; i++)
            {

                int x = i % width;
                int y = (int)(Math.Floor((double)(i - x)) / height);

                double zx = (x - width / 2.0) * zoom + xOffset;
                double zy = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int iteration = maxIterations;
                while (zx * zx + zy * zy < 4 && iteration > 1)
                {
                    var tmp = zx * zx - zy * zy + cX;
                    zy = 2.0 * zx * zy + cY;
                    zx = tmp;
                    iteration--;
                }

                int index = i * 4;

                Utils.GetIterationColor(iteration, ref buffer[index], ref buffer[index + 1], ref buffer[index + 2]);
                buffer[index + 3] = (byte)iteration;

            }
        }

        public override void DrawOnMultipleThreads(double xOffset, double yOffset, double zoom)
        {
            xOffset += xBaseOffset;
            yOffset += yBaseOffset;
            xOffset *= offsetMultiplier;
            yOffset *= offsetMultiplier;
            zoom *= zoomMultiplier;

            Parallel.For(0, width * height, (index =>
            {

                int x = index % width;
                int y = (int) (Math.Floor( (double) (index - x) ) / height);

                double zx = (x - width / 2.0) * zoom + xOffset;
                double zy = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int i = maxIterations;
                while (zx * zx + zy * zy < 4 && i > 1)
                {
                    var tmp = zx * zx - zy * zy + cX;
                    zy = 2.0 * zx * zy + cY;
                    zx = tmp;
                    i--;
                }

                index *= 4;

                Utils.GetIterationColor(i, ref buffer[index], ref buffer[index + 1], ref buffer[index + 2]);
                buffer[index + 3] = (byte)i;

            }));

        }

        [GpuManaged]
        public override void DrawOnGpu(double xOffset, double yOffset, double zoom)
        {
            xOffset += xBaseOffset;
            yOffset += yBaseOffset;
            xOffset *= offsetMultiplier;
            yOffset *= offsetMultiplier;
            zoom *= zoomMultiplier;

            byte[] buffer = this.buffer;
            int width = this.width;
            int height = this.height;
            float aspectRatio = this.aspectRatio;
            int maxIterations = this.maxIterations;

            double cX = this.cX;
            double cY = this.cY;

            int size = width * height;

            Gpu.Default.For(0, size, index =>
            {

                int x = index % width;
                int y = (int)(Math.Floor((double)(index - x)) / height);

                double zx = (x - width / 2.0) * zoom + xOffset;
                double zy = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int i = maxIterations;
                while (zx * zx + zy * zy < 4 && i > 1)
                {
                    double tmp = zx * zx - zy * zy + cX;
                    zy = 2.0 * zx * zy + cY;
                    zx = tmp;
                    i--;
                }

                index *= 4;

                buffer[index] = (byte)i;

            });

            Parallel.For(0, size, (i =>
            {

                i *= 4;

                buffer[i + 3] = (byte)(buffer[i] > 255 ? 255 : buffer[i]);
                Utils.GetIterationColor((int)buffer[i + 3], ref buffer[i], ref buffer[i + 1], ref buffer[i + 2]);

            }));

        }

        public override int GetValue(int index, double x, double y)
        {
            double zReal = x;
            double zImag = y;

            for (int counter = 0; counter < maxIterations; ++counter)
            {
                double r2 = zReal * zReal;
                double i2 = zImag * zImag;
                if (r2 + i2 > 4.0)
                {
                    return counter;
                }
                zImag = 2.0 * zReal * zImag + y;
                zReal = r2 - i2 + x;
            }

            return maxIterations;
        }

    }
}
