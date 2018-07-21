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

        internal readonly int maxIterations = 128;
        internal readonly double cX = -0.7, cY = 0.27015;
        internal readonly double zoomMultiplier = 250;
        internal readonly double offsetMultiplier = 0.04;

        public Julia(byte[] buffer, int width, int height)
        {
            this.buffer = buffer;
            this.width = width;
            this.height = height;

            this.aspectRatio = (float) width / height;
        }

        public override void DrawOnSingleThread(double xOffset, double yOffset, double zoom)
        {
            Console.WriteLine(zoom);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (x + y * width) * 4;

                    double zx = 1.5 * (x - width / 2.0) / (0.5 * zoom * width * zoomMultiplier) + xOffset * offsetMultiplier;
                    double zy = 1.0 * (y - height / 2.0) / (0.5 * zoom * height * zoomMultiplier) / aspectRatio + (yOffset / aspectRatio) * offsetMultiplier;
                    int i = maxIterations;
                    while (zx * zx + zy * zy < 4 && i > 1)
                    {
                        var tmp = zx * zx - zy * zy + cX;
                        zy = 2.0 * zx * zy + cY;
                        zx = tmp;
                        i--;
                    }

                    Utils.GetIterationColor(i, ref buffer[index], ref buffer[index + 1], ref buffer[index + 2]);
                }
            }
        }

        public override void DrawOnMultipleThreads(double xOffset, double yOffset, double zoom)
        {

            Parallel.For(0, width * height, (i =>
            {

                int x = i % width;
                int y = (int) (Math.Floor( (double) (i - x) ) / height);

                double _x = (x - width / 2.0) * zoom + xOffset;
                double _y = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int iterations = GetValue(0, _x, _y);

                i *= 4;

                Utils.GetIterationColor(iterations, ref buffer[i], ref buffer[i + 1], ref buffer[i + 2]);

            }));

        }

        [GpuManaged]
        public override void DrawOnGpu(double xOffset, double yOffset, double zoom)
        {

            byte[] buffer = this.buffer;
            int width = this.width;
            int height = this.height;
            float aspectRatio = this.aspectRatio;
            int maxIterations = this.maxIterations;

            int size = width * height;

            Gpu.Default.For(0, size, i =>
            {

                int x = i % width;
                int y = (int)(Math.Floor((double)(i - x)) / height);

                double _x = (x - width / 2.0) * zoom + xOffset;
                double _y = (y - height / 2.0) / aspectRatio * zoom + yOffset / aspectRatio;

                int counter;

                double zReal = _x;
                double zImag = _y;

                for (counter = 0; counter < maxIterations; ++counter)
                {
                    double r2 = zReal * zReal;
                    double i2 = zImag * zImag;
                    if (r2 + i2 > 4.0)
                    {
                        break;
                    }
                    zImag = 2.0 * zReal * zImag + _y;
                    zReal = r2 - i2 + _x;
                }

                i *= 4;              

                buffer[i] = (byte)counter;

            });

            Parallel.For(0, size, (i =>
            {

                i *= 4;

                Utils.GetIterationColor((int)buffer[i], ref buffer[i], ref buffer[i + 1], ref buffer[i + 2]);

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
