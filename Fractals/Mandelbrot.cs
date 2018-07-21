using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;

namespace Fractals
{
    public class Mandelbrot : Fractal
    {

        public Mandelbrot(byte[] buffer, int width, int height)
        {
            this.buffer = buffer;
            this.width = width;
            this.height = height;

            this.aspectRatio = (float) width / height;
        }

        public override void DrawCpuSingle(double xOffset, double yOffset, double zoom)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    int index = (x + y * width) * 4;

                    double _x = (x - width / 2.0) * zoom + xOffset;
                    double _y = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                    int iterations = GetValue(index, _x, _y, 127);

                    Utils.GetIterationColor(iterations, ref buffer[index], ref buffer[index+1], ref buffer[index+2]);

                }
            }
        }

        public override void DrawCpuMulti(double xOffset, double yOffset, double zoom)
        {

            Parallel.For(0, width * height, (i =>
            {

                int x = i % width;
                int y = (int) (Math.Floor( (double) (i - x) ) / height);

                double _x = (x - width / 2.0) * zoom + xOffset;
                double _y = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int iterations = GetValue(0, _x, _y, 127);

                i *= 4;

                Utils.GetIterationColor(iterations, ref buffer[i], ref buffer[i + 1], ref buffer[i + 2]);

            }));

        }

        [GpuManaged]
        public override void DrawGpu(double xOffset, double yOffset, double zoom)
        {

            byte[] buffer = this.buffer;
            int width = this.width;
            int height = this.height;
            float aspectRatio = this.aspectRatio;

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

                for (counter = 0; counter < 127; ++counter)
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

        public override int GetValue(int index, double x, double y, int maxIteration)
        {
            double zReal = x;
            double zImag = y;

            for (int counter = 0; counter < maxIteration; ++counter)
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

            return maxIteration;
        }

    }
}
