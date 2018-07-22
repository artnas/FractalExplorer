﻿using System;
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

        protected new FractalType type = FractalType.Mandelbrot;
        protected readonly int maxIterations = 127;

        protected new readonly double zoomMultiplier = 0.004;
        protected new readonly double offsetMultiplier = 0.04;
        protected new readonly double xBaseOffset = -0.7;
        protected new readonly double yBaseOffset = -0.35;

        public Mandelbrot(byte[] buffer, int width, int height)
        {
            this.buffer = buffer;
            this.width = width;
            this.height = height;

            this.aspectRatio = (float) width / height;
        }

        public override void DrawOnSingleThread(double xOffset, double yOffset, double zoom)
        {
            xOffset += xBaseOffset;
            yOffset += yBaseOffset;
            xOffset *= offsetMultiplier;
            yOffset *= offsetMultiplier;
            zoom *= zoomMultiplier;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    int index = (x + y * width) * 4;

                    double _x = (x - width / 2.0) * zoom + xOffset;
                    double _y = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                    int iterations = GetValue(index, _x, _y);

                    Utils.GetIterationColor(iterations, ref buffer[index], ref buffer[index+1], ref buffer[index+2]);

                    buffer[index + 3] = (byte) (iterations > 255 ? 255 : iterations);

                }
            }
        }

        public override void DrawOnMultipleThreads(double xOffset, double yOffset, double zoom)
        {
            xOffset += xBaseOffset;
            yOffset += yBaseOffset;
            xOffset *= offsetMultiplier;
            yOffset *= offsetMultiplier;
            zoom *= zoomMultiplier;

            Parallel.For(0, width * height, (i =>
            {

                int x = i % width;
                int y = (int) (Math.Floor( (double) (i - x) ) / height);

                double _x = (x - width / 2.0) * zoom + xOffset;
                double _y = (y - height / 2.0) * zoom / aspectRatio + yOffset / aspectRatio;

                int iterations = GetValue(0, _x, _y);

                i *= 4;

                Utils.GetIterationColor(iterations, ref buffer[i], ref buffer[i + 1], ref buffer[i + 2]);

                buffer[i + 3] = (byte)(iterations > 255 ? 255 : iterations);

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

            int size = width * height;

            Gpu.Default.For(0, size, i =>
            {

                int x = i % width;
                int y = (int)(Math.Floor((double)(i - x)) / height);

                double _x = (x - width / 2.0) * zoom + xOffset;
                double _y = (y - height / 2.0) / aspectRatio * zoom + yOffset * aspectRatio;

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

                buffer[i + 3] = (byte)(buffer[i] > 255 ? 255 : buffer[i]);
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
