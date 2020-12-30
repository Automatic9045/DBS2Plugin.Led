using System;
using System.Threading.Tasks;

namespace DbsPlugin.Standard.Led
{
    internal class ElapsedMilliSecondsLedCalculator
    {
        internal static void WriteToDots(byte[] dots, int dotsStride, int milliSecond)
        {
            int number = milliSecond / 10000;
            Parallel.For(0, 5, i =>
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

                if (i == 0) number = milliSecond / 1000 - milliSecond / 10000 * 10;
                if (i == 1) number = milliSecond / 100 - milliSecond / 1000 * 10;
                if (i == 2) number = milliSecond / 10 - milliSecond / 100 * 10;
                if (i == 3) number = milliSecond - milliSecond / 10 * 10;
            });
        }
    }
}
