using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using DbsPlugin.Standard.Led.RichTextElements;
using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    public class RichText
    {
        private ReadOnlyCollection<string> StandaloneCommands = new List<string>()
        {
            "hdef",
            "img", "image",
            "prt", "part",
            "stp", "stop",
        }.AsReadOnly();

        private List<IRichTextElement> _Text;
        public ReadOnlyCollection<IRichTextElement> Text { get; }

        public LedFont BiggestFont { get; }

        public byte[] StableBitmap { get; private set; }
        public byte[] FlashBitmap { get; private set; }
        public bool HasFlashElement { get; private set; } = false;

        public bool HasBitmapRegistered { get; private set; } = false;

        public int Width { get; private set; }
        public int Height { get; }
        public bool IsAutoWidth { get; } = false;

        public ReadOnlyCollection<int> StopPositions { get; private set; }

        public RichText(List<IRichTextElement> text, int width, int height)
        {
            Width = width;
            Height = height;

            _Text = text;
            Text = _Text.AsReadOnly();
        }

        internal RichText(string formattedString, List<LedFont> availableFonts, List<LedPartBitmapExtension> partBitmaps, int width, int height)
        {
            Width = width;
            Height = height;
            IsAutoWidth = Width == -1;

            List<string> splittedString = new List<string>();

            if (formattedString[0] != '{') throw new ArgumentException("与えられた文字列がコマンドで始まっていません。");
            string currentPhaseString = "";
            bool isInCommand = false;
            for (int i = 0; i < formattedString.Length; i++)
            {
                char currentChar = formattedString[i];

                if (currentChar == '{')
                {
                    if (isInCommand) // 直近に現れた'{'はコマンドの開始ではなかった
                    {
                        splittedString[splittedString.Count - 1] += "{" + currentPhaseString;
                        currentPhaseString = "";
                    }
                    else
                    {
                        if (currentPhaseString == ""
                            && !StandaloneCommands.Any(s => splittedString.Count >= 1 && splittedString[splittedString.Count - 1].StartsWith(s + ":"))
                            && !StandaloneCommands.Any(s => formattedString.Substring(i + 1).StartsWith(s + ":"))) // コマンドが連続していた（文字列修飾コマンドに限る）
                        {
                            currentPhaseString = splittedString[splittedString.Count - 1] + "\n";
                            splittedString.RemoveAt(splittedString.Count - 1);
                        }
                        else if (i != 0)
                        {
                            splittedString.Add(currentPhaseString);
                            currentPhaseString = "";
                        }
                        isInCommand = true;
                    }
                }
                else if (currentChar == '}' && isInCommand)
                {
                    splittedString.Add(currentPhaseString);
                    if (StandaloneCommands.Any(s => splittedString.Last().StartsWith(s + ":")))
                    {
                        splittedString.AddRange(new string[] { "", "" });
                    }
                    isInCommand = false;
                    currentPhaseString = "";
                }
                else
                {
                    currentPhaseString += currentChar;
                }
            }
            if (!isInCommand)
            {
                splittedString.Add(currentPhaseString);
            }

            List<IRichTextElement> text = new List<IRichTextElement>();
            for (int i = 0; i < splittedString.Count; i += 2)
            {
                if (StandaloneCommands.Any(s => splittedString[i].StartsWith(s + ":")))
                {
                    string[] splittedCommand = splittedString[i].Split(':');

                    if (splittedCommand.Length != 2) continue;

                    string commandType = splittedCommand[0];
                    string[] commandArgs = splittedCommand[1].Split(',');
                    switch (commandType)
                    {
                        case "hdef":
                            BiggestFont = availableFonts.Find(f => f.SystemName == commandArgs[0]);
                            break;

                        case "img":
                        case "image":
                            if (commandArgs.Length != 1) continue;
                            //text.Add(new Image());
                            break;

                        case "prt":
                        case "part":
                            if (commandArgs.Length != 3) continue;
                            try
                            {
                                LedPartBitmapExtension currentGroup = partBitmaps.First(b => b.Bitmap.Path == commandArgs[0]);
                                text.Add(new Image(currentGroup.Bitmap.Pixels, currentGroup.DrawWidth, currentGroup.DrawHeight, currentGroup.Bitmap.Width, currentGroup.Bitmap.Height, int.Parse(commandArgs[1]), int.Parse(commandArgs[2]), text.Last().Flash));
                            }
                            catch (Exception ex) { }
                            break;

                        case "stp":
                        case "stop":
                            text.Add(new Stop(text.Last().Flash));
                            break;
                    }
                }
                else
                {
                    string[] commands = splittedString[i].Split('\n');
                    string innerString = splittedString[i + 1];

                    LedFont font = null;
                    ulong fontColor = 0xffff8000;
                    ulong backgroundColor = 0x00000000;
                    bool flash = false;

                    if (text.Count > 0)
                    {
                        flash = text.Last().Flash;
                    }

                    if (text.Any(e => e is Run))
                    {
                        Run previousRun = (Run)text.FindLast(e => e is Run);

                        font = previousRun.Font;
                        fontColor = previousRun.FontColor;
                        backgroundColor = previousRun.BackgroundColor;
                    }

                    foreach (string command in commands)
                    {
                        string[] splittedCommand = command.Split(':');

                        if (splittedCommand.Length != 2) continue;

                        string commandType = splittedCommand[0];
                        string[] commandArgs = splittedCommand[1].Split(',');

                        switch (commandType)
                        {
                            case "fnt":
                            case "font":
                                if (availableFonts.Any(f => f.SystemName == commandArgs[0]))
                                {
                                    font = availableFonts.Find(f => f.SystemName == commandArgs[0]);
                                }
                                else if (availableFonts.Any(f => f.Name == commandArgs[0]))
                                {
                                    font = availableFonts.Find(f => f.Name == commandArgs[0]);
                                }
                                break;

                            case "clr":
                            case "color":
                                try
                                {
                                    fontColor = ColorStringToULong(commandArgs[0]);
                                }
                                catch { }
                                break;

                            case "bg":
                            case "background":
                                try
                                {
                                    backgroundColor = ColorStringToULong(commandArgs[0]);
                                }
                                catch { }
                                break;

                            case "fls":
                            case "flash":
                                switch (commandArgs[0])
                                {
                                    case "0":
                                        flash = false;
                                        break;

                                    case "1":
                                        flash = true;
                                        break;
                                }
                                break;
                        }
                    }

                    if (!(font is null))
                    {
                        text.Add(new Run(innerString)
                        {
                            Font = font,
                            FontColor = fontColor,
                            BackgroundColor = backgroundColor,
                            Flash = flash,
                        });
                    }
                }
            }

            _Text = text;
            Text = _Text.AsReadOnly();

            ulong ColorStringToULong(string colorString)
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorString);
                return ((ulong)color.A << 24) + ((ulong)color.R << 16) + ((ulong)color.G << 8) + (ulong)color.B;
            }
        }

        public RichText(int width, int height)
        {
            Width = width;
            Height = height;
            IsAutoWidth = Width == -1;

            _Text = new List<IRichTextElement>();
            Text = _Text.AsReadOnly();
        }

        public void RegisterBitmap(CalculatorGateway calculatorGateway)
        {
            HasFlashElement = Text.Any(e => e.Flash);

            if (IsAutoWidth)
            {
                int width = 0;

                foreach (IRichTextElement element in Text)
                {
                    if (element is Run)
                    {
                        Run run = (Run)element;
                        if (run.InnerText != "")
                        {
                            width += calculatorGateway.GetStringWidth(run.InnerText, run.Font.FontIndex);
                        }
                    }
                    else if (element is Image)
                    {
                        Image image = (Image)element;
                        width += image.DrawWidth;
                    }
                }

                Width = width;
            }

            byte[] stableBitmap = new byte[Width * Height * 3];
            calculatorGateway.ClearDots(stableBitmap, Width, Height);

            byte[] flashBitmap = new byte[Width * Height * 3];
            calculatorGateway.ClearDots(flashBitmap, Width, Height);

            List<int> stopPositions = new List<int>();

            if (!(BiggestFont is null))
            {
                int ascent = calculatorGateway.GetStringAscent(BiggestFont.FontIndex);
                int y = Height - calculatorGateway.GetStringHeight(BiggestFont.FontIndex);

                int x = 0;
                foreach (IRichTextElement element in Text)
                {
                    if (x >= Width) break;

                    if (element is Run)
                    {
                        Run run = (Run)element;
                        if (run.InnerText != "")
                        {
                            calculatorGateway.WriteStringToDots(
                                run.Flash ? flashBitmap : stableBitmap,
                                run.InnerText, run.Font.FontIndex, run.FontColor, run.BackgroundColor,
                                x, BiggestFont.FontIndex == run.Font.FontIndex ? y : y + ascent - calculatorGateway.GetStringAscent(run.Font.FontIndex),
                                Width, Height);
                            x += calculatorGateway.GetStringWidth(run.InnerText, run.Font.FontIndex);
                        }
                    }
                    else if (element is Image)
                    {
                        Image image = (Image)element;
                        calculatorGateway.WriteImageToDots(
                            image.Flash ? flashBitmap : stableBitmap, image.Bitmap,
                            x, Height - image.DrawHeight,
                            image.Width, image.Height,
                            image.DrawWidth, image.DrawHeight,
                            Width, Height,
                            image.XIndex, image.YIndex,
                            false);
                        x += image.DrawWidth;
                    }
                    else if (element is Stop)
                    {
                        stopPositions.Add(x);
                    }
                }
            }

            StableBitmap = stableBitmap;
            FlashBitmap = flashBitmap;
            StopPositions = stopPositions.AsReadOnly();

            HasBitmapRegistered = true;
        }
    }
}
