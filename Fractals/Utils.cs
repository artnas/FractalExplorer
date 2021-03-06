﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals
{
    public static class Utils
    {

        private static byte[] cachedIterationColors;
        public static void BuildIterationColorsCache()
        {
            cachedIterationColors = new byte[128 * 3];

            for (int i = 0; i < 128; i++)
            {
                EvaluateIterationColor(i, ref cachedIterationColors[i * 3], ref cachedIterationColors[i * 3 + 1], ref cachedIterationColors[i * 3 + 2]);
            }
        }

        public static void GetIterationColor(int iteration, ref byte b, ref byte g, ref byte r)
        {
            iteration %= 128;
            b = cachedIterationColors[iteration * 3];
            g = cachedIterationColors[iteration * 3 + 1];
            r = cachedIterationColors[iteration * 3 + 2];
        }

        private static void EvaluateIterationColor(int iteration, ref byte b, ref byte g, ref byte r)
        {

            iteration %= 128;

            if (iteration == -1)
            {
                r = 0;
                g = 0;
                b = 0;
            }
            else if (iteration == 0)
            {
                r = 255;
                g = 0;
                b = 0;
            }
            else
            {
                // colour gradient:      Red -> Blue -> Green -> Red -> Black
                // corresponding values:  0  ->  16  ->  32   -> 64  ->  127 (or -1)
                if (iteration < 16)
                {
                    r = (byte) (16 * (16 - iteration));
                    g = 0;
                    b = (byte) (16 * iteration - 1);
                }
                else if (iteration < 32)
                {
                    r = 0;
                    g = (byte) (16 * (iteration - 16));
                    b = (byte) (16 * (32 - iteration) - 1);
                }
                else if (iteration < 64)
                {
                    r = (byte) (8 * (iteration - 32));
                    g = (byte) (8 * (64 - iteration) - 1);
                    b = 0;
                }
                else
                {
                    // range is 64 - 127
                    r = (byte) (255 - (iteration - 64) * 4);
                    g = 0;
                    b = 0;
                }

            }

        }
    }

}
