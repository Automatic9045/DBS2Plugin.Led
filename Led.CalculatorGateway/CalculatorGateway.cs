using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DbsPlugin.Standard.Led.Calculation
{
    public enum AntialiasFormat
    {
        Bitmap = 1,
        Gray2Bitmap = 4,
        Gray4Bitmap = 5,
        Gray8Bitmap = 6,
    }

    public class CalculatorGateway
    {
        private bool isX64;
        private List<FontInfo> fonts = new List<FontInfo>();

        public CalculatorGateway(bool isX64)
        {
            this.isX64 = isX64;
        }

        public void Draw(byte[] bmp, byte[] dots, int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll)
        {
            if (isX64)
            {
                CalculationX64.draw(bmp, dots, dotWidth, dotHeight, dotXDistance, dotYDistance, dotRadius, stride, redrawAll);
            }
            else
            {
                CalculationX86.draw(bmp, dots, dotWidth, dotHeight, dotXDistance, dotYDistance, dotRadius, stride, redrawAll);
            }
        }

        public int RegisterFont(string fontFamily, int fontSize, int fontWeight, AntialiasFormat antialiasFormat)
        {
            IntPtr hfont = isX64 ? CalculationX64.getHfont(fontFamily, fontSize, fontWeight) : CalculationX86.getHfont(fontFamily, fontSize, fontWeight);
            if (fonts.Any(f => f.FontFamily == fontFamily && f.FontSize == fontSize && f.FontWeight == fontWeight && f.AntialiasFormat == antialiasFormat))
            {
                return fonts.FindIndex(f => f.FontFamily == fontFamily && f.FontSize == fontSize && f.FontWeight == fontWeight && f.AntialiasFormat == antialiasFormat);
            }
            else
            {
                fonts.Add(new FontInfo(hfont, fontFamily, fontSize, fontWeight, antialiasFormat));
                return fonts.Count - 1;
            }
        }

        public int GetStringAscent(int fontIndex)
            => isX64 ?
            CalculationX64.getStringAscent(fonts[fontIndex].Hfont) :
            CalculationX86.getStringAscent(fonts[fontIndex].Hfont);

        public int GetStringHeight(int fontIndex)
            => isX64 ?
            CalculationX64.getStringHeight(fonts[fontIndex].Hfont) :
            CalculationX86.getStringHeight(fonts[fontIndex].Hfont);

        public int GetStringWidth(string str, int fontIndex)
            => isX64 ?
            CalculationX64.getStringWidth(str, fonts[fontIndex].Hfont, (uint)fonts[fontIndex].AntialiasFormat) :
            CalculationX86.getStringWidth(str, fonts[fontIndex].Hfont, (uint)fonts[fontIndex].AntialiasFormat);

        public void WriteStringToDots(byte[] dots, string str, int fontIndex, ulong fontColor, ulong backgroundColor, int x, int y, int dotWidth, int dotHeight)
        {
            if (isX64)
            {
                CalculationX64.writeStringToDots(dots, str, fonts[fontIndex].Hfont, (uint)fonts[fontIndex].AntialiasFormat, fontColor, backgroundColor, x, y, dotWidth, dotHeight);
            }
            else
            {
                CalculationX86.writeStringToDots(dots, str, fonts[fontIndex].Hfont, (uint)fonts[fontIndex].AntialiasFormat, fontColor, backgroundColor, x, y, dotWidth, dotHeight);
            }
        }

        public void WriteImageToDots(byte[] dots, byte[] source, int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent)
        {
            if (isX64)
            {
                CalculationX64.writeImageToDots(dots, source, x, y, width, height, drawWidth, drawHeight, dotWidth, dotHeight, xIndex, yIndex, enableTransparent);
            }
            else
            {
                CalculationX86.writeImageToDots(dots, source, x, y, width, height, drawWidth, drawHeight, dotWidth, dotHeight, xIndex, yIndex, enableTransparent);
            }
        }

        public void CopyDots(byte[] destination, byte[] source, int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent)
        {
            if (isX64)
            {
                CalculationX64.copyDots(destination, source, width, height, destX, destY, destWidth, destHeight, sourceX, sourceY, sourceWidth, sourceHeight, enableTransparent);
            }
            else
            {
                CalculationX86.copyDots(destination, source, width, height, destX, destY, destWidth, destHeight, sourceX, sourceY, sourceWidth, sourceHeight, enableTransparent);
            }
        }

        public void ClearDots(byte[] dots, int dotWidth, int dotHeight)
        {
            if (isX64)
            {
                CalculationX64.clearDots(dots, dotWidth, dotHeight);
            }
            else
            {
                CalculationX86.clearDots(dots, dotWidth, dotHeight);
            }
        }


        struct FontInfo
        {
            public IntPtr Hfont { get;  }
            public string FontFamily { get; }
            public int FontSize { get; }
            public int FontWeight { get; }
            public AntialiasFormat AntialiasFormat { get; set; }

            public FontInfo(IntPtr hfont, string fontFamily, int fontSize, int fontWeight, AntialiasFormat antialiasFormat)
            {
                Hfont = hfont;
                FontFamily = fontFamily;
                FontSize = fontSize;
                FontWeight = fontWeight;
                AntialiasFormat = antialiasFormat;
            }
        }


        static class CalculationX64
        {
            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void draw(byte[] bmp, byte[] dots, int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr getHfont([MarshalAs(UnmanagedType.LPWStr)] string fontFamily, int fontSize, int fontWeight);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringAscent(IntPtr hfont);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringHeight(IntPtr hfont);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringWidth([MarshalAs(UnmanagedType.LPWStr)] string str, IntPtr hfont, uint antialiasFormat);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void writeStringToDots(byte[] dots, [MarshalAs(UnmanagedType.LPWStr)] string str, IntPtr hfont, uint antialiasFormat, ulong fontColor, ulong backgroundColor, int x, int y, int dotW, int dotH);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void writeImageToDots(byte[] dots, byte[] source, int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void copyDots(byte[] destination, byte[] source, int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void fillDots(byte[] dots, ulong color, int x, int y, int drawWidth, int drawHeight, int width, int height);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern void clearDots(byte[] dots, int dotWidth, int dotHeight);
        }

        static class CalculationX86
        {
            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void draw(byte[] bmp, byte[] dots, int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr getHfont([MarshalAs(UnmanagedType.LPWStr)] string fontFamily, int fontSize, int fontWeight);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringAscent(IntPtr hfont);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringHeight(IntPtr hfont);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int getStringWidth([MarshalAs(UnmanagedType.LPWStr)] string str, IntPtr hfont, uint antialiasFormat);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void writeStringToDots(byte[] dots, [MarshalAs(UnmanagedType.LPWStr)] string str, IntPtr hfont, uint antialiasFormat, ulong fontColor, ulong backgroundColor, int x, int y, int dotW, int dotH);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void writeImageToDots(byte[] dots, byte[] source, int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void copyDots(byte[] destination, byte[] source, int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void fillDots(byte[] dots, ulong color, int x, int y, int drawWidth, int drawHeight, int width, int height);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern void clearDots(byte[] dots, int dotWidth, int dotHeight);
        }
    }
}