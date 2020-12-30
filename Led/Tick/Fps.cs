using System;
using System.Threading.Tasks;

namespace DbsPlugin.Standard.Led
{
    internal class FpsLedCalculator
    {
        internal static void WriteToDots(byte[] dots, int dotsStride, int fps)
        {
            int number = fps / 100;
            Parallel.For(0, 3, i =>
            {
                if (number != 1) dots[12 * i + 2] = 0xff; else dots[12 * i + 2] = 0x33;
                if (number != 1 && number != 4) dots[12 * i + 5] = 0xff; else dots[12 * i + 5] = 0x33;
                dots[12 * i + 8] = 0xff;

                if (number != 1 && number != 2 && number != 3 && number != 7) dots[12 * i + dotsStride + 2] = 0xff; else dots[12 * i + dotsStride + 2] = 0x33;
                dots[12 * i + dotsStride + 5] = 0x33;
                if (number != 5 && number != 6) dots[12 * i + dotsStride + 8] = 0xff; else dots[12 * i + dotsStride + 8] = 0x33;

                if (number != 1 && number != 7) dots[12 * i + dotsStride * 2 + 2] = 0xff; else dots[12 * i + dotsStride * 2 + 2] = 0x33;
                if (number != 0 && number != 1 && number != 7) dots[12 * i + dotsStride * 2 + 5] = 0xff; else dots[12 * i + dotsStride * 2 + 5] = 0x33;
                dots[12 * i + dotsStride * 2 + 8] = 0xff;

                if (number == 0 || number == 2 || number == 6 || number == 8) dots[12 * i + dotsStride * 3 + 2] = 0xff; else dots[12 * i + dotsStride * 3 + 2] = 0x33;
                dots[12 * i + dotsStride * 3 + 5] = 0x33;
                if (number != 2) dots[12 * i + dotsStride * 3 + 8] = 0xff; else dots[12 * i + dotsStride * 3 + 8] = 0x33;

                if (number != 1 && number != 4 && number != 7) dots[12 * i + dotsStride * 4 + 2] = 0xff; else dots[12 * i + dotsStride * 4 + 2] = 0x33;
                if (number != 1 && number != 4 && number != 7) dots[12 * i + dotsStride * 4 + 5] = 0xff; else dots[12 * i + dotsStride * 4 + 5] = 0x33;
                dots[12 * i + dotsStride * 4 + 8] = 0xff;

                if (i == 0) number = fps / 10 - fps / 100 * 10;
                if (i == 1) number = fps - fps / 10 * 10;
            });
        }
    }
}
