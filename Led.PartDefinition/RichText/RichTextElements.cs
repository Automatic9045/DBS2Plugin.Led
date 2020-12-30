using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbsPlugin.Standard.Led.RichTextElements
{
    public interface IRichTextElement
    {
        bool Flash { get; set; }
    }

    public class Run : IRichTextElement
    {
        public string InnerText { get; set; }
        public LedFont Font { get; set; }
        public ulong FontColor { get; set; }
        public ulong BackgroundColor { get; set; }
        public bool Flash { get; set; } = false;

        public Run(string innerText)
        {
            InnerText = innerText;
        }
    }

    public class Image : IRichTextElement
    {
        public byte[] Bitmap { get; }
        public int Width { get; }
        public int Height { get; }
        public int DrawWidth { get; }
        public int DrawHeight { get; }
        public int XIndex { get; }
        public int YIndex { get; }
        public bool Flash { get; set; } = false;

        public Image(byte[] bitmap, int drawWidth, int drawHeight, int width, int height, int yIndex, int xIndex, bool flash)
        {
            Bitmap = bitmap;
            Width = width;
            Height = height;
            DrawWidth = drawWidth;
            DrawHeight = drawHeight;
            XIndex = xIndex;
            YIndex = yIndex;
            Flash = flash;
        }
    }

    public class Stop : IRichTextElement
    {
        public bool Flash { get; set; } = false;

        public Stop(bool flash)
        {
            Flash = flash;
        }
    }
}
