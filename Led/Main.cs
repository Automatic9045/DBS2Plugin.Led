using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using static System.IO.Path;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualBasic;

using DepartureBoardSimulator;
using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    public partial class Led : DbsPluginBase
    {
        protected override DbsPluginConnector PluginConnector { get; }

        private List<LedControl> ledControls = new List<LedControl>();
        private List<LedPartBitmapExtension> ledPartBitmaps = new List<LedPartBitmapExtension>();

        private Dictionary<string, IVisibilityConverter> visibilityConverters = new Dictionary<string, IVisibilityConverter>();
        private Dictionary<string, IDisplayPhaseConverter> displayPhaseConverters = new Dictionary<string, IDisplayPhaseConverter>();

        private LedController controller;

        private CalculatorGateway calculatorGateway = new CalculatorGateway(Environment.Is64BitProcess);

        private Dispatcher dispatcher;

        internal static string PluginName = null;

        private string dllPath = typeof(IVisibilityConverter).Assembly.Location;
        private string tempDllPath = null;

        public Led(DbsPluginConnector pluginConnector) : base(pluginConnector)
        {
            PluginConnector = pluginConnector;
            dispatcher = PluginConnector.GetDispatcher();

            if (PluginName == null) PluginName = this.GetType().Name;
            tempDllPath = Combine(PluginConnector.TempDirectory, GetFileName(dllPath));

            controller = new LedController(new LedControllerConnector(PluginConnector, ledControls, calculatorGateway));

            File.Copy(dllPath, tempDllPath, true);
        }

        internal static string ToSearchString(string basedString)
        {
            return Strings.StrConv(basedString, VbStrConv.Hiragana | VbStrConv.Wide | VbStrConv.Lowercase).Replace("　", "").Replace("‐", "").Replace("・", "") ?? "";
        }

        public override void Add(XElement element, string xmlPath)
        {
            switch (element.Name.LocalName)
            {
                case "LedDisplay":
                    AddLedDisplay(element, LedDisplayMode.Normal, xmlPath);
                    break;
                case "FpsLedDisplay":
                    AddLedDisplay(element, LedDisplayMode.Fps, xmlPath);
                    break;
                case "MillisecLedDisplay":
                    AddLedDisplay(element, LedDisplayMode.ElapsedMilliseconds, xmlPath);
                    break;
                case "DebugLedDisplay":
                    AddLedDisplay(element, LedDisplayMode.Debug, xmlPath);
                    break;
                default:
                    PluginConnector.ThrowError("コントロール \"" + element.Name.LocalName + "\" は定義されていません。", this.GetType().Name, "", "");
                    break;
            }
        }

        public override void Initialized()
        {
            ledPartBitmaps = ledControls.ConvertAll(control =>
            {
                return control.Parts.ConvertAll(part =>
                {
                    return part.Bitmaps.ConvertAll(bitmap =>
                    {
                        return new LedPartBitmapExtension()
                        {
                            Bitmap = bitmap,
                            DrawWidth = part.Width,
                            DrawHeight = part.Height,
                            ParentControlName = control.ControlName,
                            ParentPartName = part.Name,
                        };
                    });
                });
            }).SelectMany(b => b).SelectMany(b => b).Distinct(new LedPartBitmapExtensionEqualityComparer()).ToList();

            controller.VM.FullTextEditor.UpdatePartBitmaps(ledPartBitmaps);
            controller.VM.FreeTextEditor.UpdatePartBitmaps(ledPartBitmaps);
        }

        public override async void Loaded()
        {
            File.Delete(tempDllPath);

            long count = 0;
            int fps = 0;
            long elapsedMilliseconds = PluginConnector.ElapsedMilliseconds;
            while (true)
            {
                await Task.Delay(1);
                ledControls.ForEach(control =>
                {
                    switch (control.Mode)
                    {
                        case LedDisplayMode.Fps:
                            FpsLedCalculator.WriteToDots(control.DotPixels, control.DotStride, fps);
                            break;

                        case LedDisplayMode.ElapsedMilliseconds:
                            List<LedControl> controls = ledControls.FindAll(c => c.Mode == LedDisplayMode.Normal && c.Parts.Count != 0);
                            ElapsedMilliSecondsLedCalculator.WriteToDots(control.DotPixels, control.DotStride, (int)(PluginConnector.ElapsedMilliseconds % 10000));
                            break;

                        case LedDisplayMode.Normal:
                            calculatorGateway.ClearDots(control.DotPixels, control.DotWidth, control.DotHeight);

                            if (control.UseFullText)
                            {
                                if (control.FullText.HasBitmapRegistered)
                                {
                                    int x = control.ScrollFullText ? control.GetScrollingFullTextX((int)(PluginConnector.ElapsedMilliseconds - control.FunnTextScrollStartedTime)) : 0;

                                    calculatorGateway.CopyDots(
                                        control.DotPixels, control.FullText.StableBitmap,
                                        control.FullText.Width, control.DotHeight,
                                        x, 0,
                                        control.DotWidth, control.DotHeight,
                                        0, 0,
                                        control.FullText.Width, control.DotHeight,
                                        false);

                                    if (control.FullText.HasFlashElement && PluginConnector.ElapsedMilliseconds % 800 < 600)
                                    {
                                        calculatorGateway.CopyDots(
                                            control.DotPixels, control.FullText.FlashBitmap,
                                            control.FullText.Width, control.DotHeight,
                                            x, 0,
                                            control.DotWidth, control.DotHeight,
                                            0, 0,
                                            control.FullText.Width, control.DotHeight,
                                            true);
                                    }
                                }
                            }
                            else
                            {
                                control.Parts.ForEach(part =>
                                {
                                    LedPartDisplayPhase displaySetting = part.DisplayPhases[part.CurrentDisplayPhase];
                                    if ((part.Visibility & displaySetting.VisibilityIndex) == displaySetting.VisibilityIndex)
                                    {
                                        part.DisplayingXIndex = displaySetting.FaceIndex;

                                        if (part.UseFreeText)
                                        {
                                            if (part.FreeText.HasBitmapRegistered)
                                            {
                                                calculatorGateway.CopyDots(
                                                    control.DotPixels, part.FreeText.StableBitmap,
                                                    part.Width, part.Height,
                                                    part.X, part.Y,
                                                    control.DotWidth, control.DotHeight,
                                                    0, 0,
                                                    part.Width, part.Height,
                                                    false);

                                                if (part.FreeText.HasFlashElement && PluginConnector.ElapsedMilliseconds % 800 < 600)
                                                {
                                                    calculatorGateway.CopyDots(
                                                        control.DotPixels, part.FreeText.FlashBitmap,
                                                        part.Width, part.Height,
                                                        part.X, part.Y,
                                                        control.DotWidth, control.DotHeight,
                                                        0, 0,
                                                        part.Width, part.Height,
                                                        true);
                                                }
                                            }
                                        }
                                        else if (part.DisplayingYIndex != -1 && (!part.Bitmaps[part.DisplayingBitmap].Definitions[part.DisplayingYIndex].Flash || PluginConnector.ElapsedMilliseconds % 800 < 600))
                                        {
                                            calculatorGateway.WriteImageToDots(
                                                control.DotPixels, part.Bitmaps[part.DisplayingBitmap].Pixels,
                                                part.X, part.Y,
                                                part.Bitmaps[part.DisplayingBitmap].Width, part.Bitmaps[part.DisplayingBitmap].Height,
                                                part.Width, part.Height,
                                                control.DotWidth, control.DotHeight,
                                                part.DisplayingXIndex, part.DisplayingYIndex,
                                                true);
                                        }
                                    }
                                    else
                                    {
                                        part.DisplayingXIndex = -1;
                                    }
                                });
                            }
                            break;
                    }

                    calculatorGateway.Draw(control.Pixels, control.DotPixels, control.DotWidth, control.DotHeight, control.DotXDistance, control.DotYDistance, control.DotRadius, control.Stride, count == 0);
                });

                await dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (LedControl led in ledControls)
                    {
                        led.Bitmap.WritePixels(new Int32Rect(0, 0, led.Width, led.Height), led.Pixels, led.Stride, 0, 0);
                    }
                }));

                count++;
                if (((count >> 4) & 0b1) == 0b1)
                {
                    fps = (int)(1000L * count / (PluginConnector.ElapsedMilliseconds - elapsedMilliseconds));
                }
            }
        }

        public override void ControllerShown()
        {
            controller.VM.Shown();
        }
    }
}