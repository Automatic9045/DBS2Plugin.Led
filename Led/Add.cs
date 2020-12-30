using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using static System.IO.Path;
using Drawing = System.Drawing;
using System.Drawing.Imaging;
using Imaging = System.Drawing.Imaging;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
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

using DepartureBoardSimulator;

namespace DbsPlugin.Standard.Led
{
    public partial class Led : DbsPluginBase
    {
        private void AddLedDisplay(XElement element, LedDisplayMode mode, string xmlPath)
        {
            string type = element.Name.LocalName;
            string name = (string)element.Attribute("Name");

            int width = (int)((uint?)element.Attribute("Width") ?? 98);
            int height = (int)((uint?)element.Attribute("Height") ?? 98);
            WriteableBitmap bmp = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr32, null);
            Image image = new Image()
            {
                HorizontalAlignment = (HorizontalAlignment)TypeDescriptor.GetConverter(typeof(HorizontalAlignment)).ConvertFromString((string)element.Attribute("Horizontal") ?? "Left"),
                VerticalAlignment = (VerticalAlignment)TypeDescriptor.GetConverter(typeof(VerticalAlignment)).ConvertFromString((string)element.Attribute("Vertical") ?? "Top"),
                Margin = new Thickness((double?)element.Attribute("X") ?? 0.0, (double?)element.Attribute("Y") ?? 0.0, 0, 0),

                Width = width,
                Height = height,

                Source = bmp,
            };
            int stride = width * 4;
            int dotWidth = (int)((uint?)element.Attribute("DotWidth") ?? 16);
            int dotHeight = (int)((uint?)element.Attribute("DotHeight") ?? 16);
            int dotStride = dotWidth * 3;
            int dotDiameter = (int)((uint?)element.Attribute("DotDiameter") ?? 4);
            double dotRadius = dotDiameter / 2.0;
            double dotXDistance = (width - dotDiameter * dotWidth) / (dotWidth + 1.0);
            double dotYDistance = (height - dotDiameter * dotHeight) / (dotHeight + 1.0);

            if (dotXDistance <= 0 || dotYDistance <= 0)
            {
                ThrowControlError("LED 粒子の直径が大きすぎます。");
                return;
            }

            ControlInfo controlInfo = new ControlInfo()
            {
                PluginName = PluginName,
                ControlType = this,
                ControlTypeName = type,
                ControlName = name,
                IsNameRequired = true,
                UseControllerWindow = mode == LedDisplayMode.Normal,
                Controller = controller,
            };
            if (controlInfo.ControlName != null)
            {
                byte[] dots = new byte[dotStride * dotHeight];
                byte[] pixels = new byte[stride * height];

                List<LedPart> parts = new List<LedPart>();

                List<LedShortcut> ledShortcuts = null;
                List<LedFont> ledFonts = null;

                LedFont defaultFont = null;
                LedFont biggestFont = null;

                if (mode == LedDisplayMode.Normal)
                {
                    string layoutXmlPath = Combine(xmlPath, (string)element.Attribute("Source") ?? "");
                    Loader loader = new Loader(layoutXmlPath, tempDllPath, calculatorGateway, ThrowControlError, (sourcePath, references) => PluginConnector.CompileAssembly(sourcePath, references, PluginName, type, name));

                    if (loader.Element != null)
                    {
                        ledFonts = loader.LoadFonts();

                        string defaultFontName = (string)loader.Element.Attribute("DefaultFont");
                        if (defaultFontName == null)
                        {
                            ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で、デフォルトフォントが指定されていません。");
                            return;
                        }
                        else if (!ledFonts.Any(f => f.SystemName == defaultFontName))
                        {
                            ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で指定されているデフォルトフォント \"" + defaultFontName + "\" が見つかりませんでした。");
                            return;
                        }
                        else
                        {
                            defaultFont = ledFonts.Find(f => f.SystemName == defaultFontName);
                        }

                        string biggestFontName = (string)loader.Element.Attribute("BiggestFont");
                        if (biggestFontName == null)
                        {
                            biggestFont = defaultFont;
                        }
                        else if (!ledFonts.Any(f => f.SystemName == biggestFontName))
                        {
                            ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で指定されている基準フォント \"" + biggestFontName + "\" が見つかりませんでした。");
                            return;
                        }
                        else
                        {
                            biggestFont = ledFonts.Find(f => f.SystemName == biggestFontName);
                        }

                        XElement partDefinitionsElement = loader.Element.Element("PartDefinitions");
                        if (partDefinitionsElement != null)
                        {
                            List<XElement> partElements = partDefinitionsElement.Elements("Part").ToList();
                            partElements.ForEach(partElement =>
                            {
                                string partName = (string)partElement.Attribute("Name");
                                if (partName == null)
                                {
                                    ThrowControlError("名前のついていない LED パーツがあります。");
                                    return;
                                }

                                string partSystemName = (string)partElement.Attribute("System");
                                if (partSystemName == null || partSystemName == "") partSystemName = partName;

                                string partSearchNamesElement = (string)partElement.Attribute("Search");
                                if (partSearchNamesElement == null || partSearchNamesElement == "") partSearchNamesElement = partName;
                                IEnumerable<string> partSearchNames = partSearchNamesElement.Split(';').Select(n => ToSearchString(n));

                                LedPart part = new LedPart(() => PluginConnector.ElapsedMilliseconds)
                                {
                                    Name = partName,
                                    SystemName = partSystemName,
                                    SearchNames = partSearchNames,
                                    X = (int)((uint?)partElement.Attribute("X") ?? 0),
                                    Y = (int)((uint?)partElement.Attribute("Y") ?? 0),
                                    DisplayingBitmap = 0,
                                    GetBitmapsForRichTextFunc = () => ledPartBitmaps,
                                };

                                #region Visibilityの設定
                                int visibility = (int)((uint?)partElement.Attribute("Visibility") ?? 0b0);
                                string visibilityConverterSourceName = (string)partElement.Attribute("VisibilityConverterSource");
                                if (visibilityConverterSourceName == null)
                                {
                                    part.Visibility = visibility;
                                }
                                else
                                {
                                    string visibilityConverterSourcePath = Combine(GetDirectoryName(layoutXmlPath), visibilityConverterSourceName);
                                    IVisibilityConverter visibilityConverter = null;
                                    if (!visibilityConverters.Any((x) => x.Key == visibilityConverterSourcePath))
                                    {
                                        if (!File.Exists(visibilityConverterSourcePath))
                                        {
                                            ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\"で指定されている VisibilityConverter \"" + visibilityConverterSourcePath + "\" が見つかりませんでした。");
                                            return;
                                        }
                                        string[] referencesBase = File.ReadAllText(visibilityConverterSourcePath).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        List<string> references = new List<string>() { tempDllPath };
                                        if (referencesBase[0].StartsWith("/// ReferenceAssemblies = "))
                                        {
                                            references.AddRange(referencesBase[0].Substring("/// ReferenceAssemblies = ".Length).Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries));
                                        }
                                        Assembly assembly = PluginConnector.CompileAssembly(visibilityConverterSourcePath, references, PluginName, type, name);
                                        if (assembly == null)
                                        {
                                            ThrowControlError("VisibilityConverter \"" + visibilityConverterSourcePath + "\" のコンパイルに失敗しました。");
                                            return;
                                        }
                                        string iName = typeof(IVisibilityConverter).FullName;
                                        Type[] assemblyTypes = assembly.GetExportedTypes();
                                        foreach (Type assemblyType in assemblyTypes)
                                        {
                                            if (assemblyType.IsClass && !assemblyType.IsAbstract && assemblyType.GetInterface(iName) != null && (assemblyType.Namespace ?? "").Split('.')[0] == "DbsData")
                                            {
                                                visibilityConverter = (IVisibilityConverter)Activator.CreateInstance(assemblyType);
                                                visibilityConverters.Add(visibilityConverterSourcePath, visibilityConverter);
                                                break;
                                            }
                                        }
                                        if (visibilityConverter == null)
                                        {
                                            ThrowControlError("VisibilityConverter \"" + visibilityConverterSourcePath + "\" で、 IVisibilityConverter を実装し DbsData.* 名前空間に存在する有効なクラスが見つかりませんでした。");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        visibilityConverter = visibilityConverters[visibilityConverterSourcePath];
                                    }
                                    part.VisibilityConverterFunc = visibilityConverter.GetVisibility;
                                }
                                #endregion

                                #region DisplayPhaseの設定
                                string displayPhaseSourceName = (string)partElement.Attribute("DisplayPhaseSource");
                                if (displayPhaseSourceName == null)
                                {
                                    ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\"で、DisplayPhaseSource が指定されていないパーツがあります。");
                                    return;
                                }
                                string displayPhaseSourcePath = Combine(GetDirectoryName(layoutXmlPath), displayPhaseSourceName);
                                XDocument displayPhaseDefinitionXml = XDocument.Load(displayPhaseSourcePath);
                                XElement displayPhaseDefinitions = displayPhaseDefinitionXml.Element("LedDisplayPhaseDefinitions");
                                if (displayPhaseDefinitions == null)
                                {
                                    ThrowControlError("\"" + displayPhaseSourcePath + "\" は表示フェーズファイルではありません。");
                                    return;
                                }

                                part.DisplayPhases = displayPhaseDefinitions.Elements("DisplayPhaseDefinition").Select(displayPhaseDefinition =>
                                {
                                    int? span = (int?)displayPhaseDefinition.Attribute("Span");
                                    if (span == null)
                                    {
                                        ThrowControlError("表示フェーズファイル \"" + displayPhaseSourcePath + "\"で、Span が指定されていない表示フェーズがあります。");
                                        return null;
                                    }

                                    string rawVisibilityIndex = (string)displayPhaseDefinition.Attribute("VisibilityIndex");
                                    int visibilityIndex;
                                    if (rawVisibilityIndex == null)
                                    {
                                        ThrowControlError("表示フェーズファイル \"" + displayPhaseSourcePath + "\"で、VisibilityIndex が指定されていない表示フェーズがあります。");
                                        return null;
                                    }
                                    try
                                    {
                                        visibilityIndex = Convert.ToInt32(rawVisibilityIndex, 2);
                                    }
                                    catch
                                    {
                                        ThrowControlError("表示フェーズファイル \"" + displayPhaseSourcePath + "\"で、無効な FaceIndex が指定されている表示フェーズがあります。");
                                        return null;
                                    }

                                    int? faceIndex = (int?)displayPhaseDefinition.Attribute("FaceIndex");
                                    if (faceIndex == null)
                                    {
                                        ThrowControlError("表示フェーズファイル \"" + displayPhaseSourcePath + "\"で、FaceIndex が指定されていない表示フェーズがあります。");
                                        return null;
                                    }

                                    return new LedPartDisplayPhase() { Span = (int)span, VisibilityIndex = visibilityIndex, FaceIndex = (int)faceIndex };
                                }).ToList();
                                #endregion

                                #region パーツの取得
                                string partDefsXmlPath = Combine(GetDirectoryName(layoutXmlPath), (string)partElement.Attribute("Source") ?? "");
                                if (partDefsXmlPath == GetDirectoryName(layoutXmlPath))
                                {

                                }
                                else if (!File.Exists(partDefsXmlPath))
                                {
                                    ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" が見つかりませんでした。");
                                }
                                else
                                {
                                    XDocument partDefsXml = XDocument.Load(partDefsXmlPath);
                                    XElement partDefs = partDefsXml.Element("LedPart");
                                    if (partDefs == null)
                                    {
                                        ThrowControlError("\"" + partDefsXmlPath + "\" は LED パーツファイルではありません。");
                                        return;
                                    }

                                    part.Width = (int)((uint?)partDefs.Attribute("Width") ?? 0);
                                    part.Height = (int)((uint?)partDefs.Attribute("Height") ?? 0);
                                    part.SetBlankFreeText();
                                    part.FreeText.RegisterBitmap(calculatorGateway);

                                    part.Bitmaps = new List<LedPartBitmap>();

                                    IEnumerable<XElement> images = partDefs.Elements("Image");
                                    int imagesCount = images.Count();
                                    foreach (XElement imageElement in images)
                                    {
                                        string imageName = (string)imageElement.Attribute("Name");
                                        if (imageName == null || imageName == "")
                                        {
                                            if (imagesCount != 1)
                                            {
                                                ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で、名前付けがされていないパーツ画像があります。名前付けが不要なのはパーツ画像が 1 つのみ定義されている場合に限ります。");
                                                continue;
                                            }
                                            else
                                            {
                                                imageName = "(共通)";
                                            }
                                        }

                                        string imageSystemName = (string)imageElement.Attribute("System");
                                        if (imageSystemName == null || imageSystemName == "") imageSystemName = imageName;

                                        string imageSearchNamesBase = (string)imageElement.Attribute("Search");
                                        if (imageSearchNamesBase == null || imageSearchNamesBase == "") imageSearchNamesBase = imageName;
                                        IEnumerable<string> imageSearchNames = imageSearchNamesBase.Split(';').Select(n => ToSearchString(n));

                                        IEnumerable<XElement> definitionElements = imageElement.Elements("Definition");
                                        List<LedPartDefinition> definitions = new List<LedPartDefinition>();
                                        foreach (XElement definitionElement in definitionElements)
                                        {
                                            string definitionName = (string)definitionElement.Attribute("Name");
                                            if (definitionName == null)
                                            {
                                                ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で、名前付けがされていないコマがあります。");
                                                break;
                                            }

                                            string definitionSystemName = (string)definitionElement.Attribute("System");
                                            if (definitionSystemName == null || definitionSystemName == "") definitionSystemName = definitionName;

                                            string definitionSearchNamesBase = (string)definitionElement.Attribute("Search");
                                            if (definitionSearchNamesBase == null || definitionSearchNamesBase == "") definitionSearchNamesBase = definitionName;
                                            IEnumerable<string> definitionSearchNames = definitionSearchNamesBase.Split(';').Select(n => ToSearchString(n));

                                            bool flash = ToSearchString((string)definitionElement.Attribute("Flash") ?? "") == ToSearchString("True");
                                            if (definitionElement.Attribute("Flash") != null)
                                            {

                                            }

                                            LedPartDefinition definition = new LedPartDefinition()
                                            {
                                                Name = definitionName,
                                                SystemName = definitionSystemName,
                                                SearchNames = definitionSearchNames,
                                                Flash = flash,
                                            };
                                            definitions.Add(definition);
                                        }

                                        string sourcePath = Combine(GetDirectoryName(partDefsXmlPath), (string)imageElement.Attribute("Source") ?? "");
                                        if (sourcePath == partDefsXmlPath)
                                        {
                                            ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で、パスが指定されていないパーツ画像があります。");
                                            continue;
                                        }
                                        else if (!File.Exists(sourcePath))
                                        {
                                            ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で指定されているパーツ画像 \"" + sourcePath + "\" が見つかりませんでした。");
                                            continue;
                                        }
                                        try
                                        {
                                            Drawing.Bitmap sourceBmp = new Drawing.Bitmap(sourcePath);
                                            BitmapData sourceBmpData = sourceBmp.LockBits(new Drawing.Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height), ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb);
                                            byte[] sourceBmpBytes = new byte[sourceBmpData.Stride * sourceBmpData.Height];
                                            Marshal.Copy(sourceBmpData.Scan0, sourceBmpBytes, 0, sourceBmpBytes.Length);
                                            LedPartBitmap partBitmap = new LedPartBitmap()
                                            {
                                                Name = imageName,
                                                SystemName = imageSystemName,
                                                SearchNames = imageSearchNames,
                                                Path = new Uri(layoutXmlPath).MakeRelativeUri(new Uri(sourcePath)).ToString().Replace('/', '\\'),
                                                Pixels = sourceBmpBytes,
                                                Definitions = definitions,
                                                Width = sourceBmpData.Width,
                                                Height = sourceBmpData.Height,
                                            };
                                            part.Bitmaps.Add(partBitmap);
                                            sourceBmp.UnlockBits(sourceBmpData);
                                            if (part.DisplayPhases.Any(dp => (dp.FaceIndex + 1) * part.Width > partBitmap.Width))
                                            {
                                                ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で指定されているパーツ画像 \"" + sourcePath + "\" は、必要な数のフェイスがありません。継続すると不正なメモリアクセスが発生し、描画が乱れる可能性があります。");
                                            }
                                            if (definitions.Count * part.Height > partBitmap.Height)
                                            {
                                                ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で指定されているパーツ画像 \"" + sourcePath + "\" は、必要な数のコマがありません。継続すると不正なメモリアクセスが発生し、描画が乱れる可能性があります。");
                                            }
                                        }
                                        catch
                                        {
                                            ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で指定されているパーツ画像 \"" + sourcePath + "\" の読み込みに失敗しました。");
                                            continue;
                                        }
                                    }
                                }
                                #endregion

                                #region フォントの設定
                                part.Fonts = ledFonts;

                                string freeTextDefaultFontName = (string)loader.Element.Attribute("DefaultFont") ?? defaultFontName;
                                if (!ledFonts.Any(f => f.SystemName == freeTextDefaultFontName))
                                {
                                    ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で指定されている自由入力デフォルトフォント \"" + freeTextDefaultFontName + "\" が見つかりませんでした。");
                                    return;
                                }
                                else
                                {
                                    part.FreeTextDefaultFont = ledFonts.Find(f => f.SystemName == defaultFontName);
                                }

                                string freeTextBiggestFontName = (string)loader.Element.Attribute("BiggestFont") ?? biggestFontName;
                                if (!ledFonts.Any(f => f.SystemName == freeTextBiggestFontName))
                                {
                                    ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で指定されている自由入力基準フォント \"" + freeTextBiggestFontName + "\" が見つかりませんでした。");
                                    return;
                                }
                                else
                                {
                                    part.FreeTextBiggestFont = ledFonts.Find(f => f.SystemName == biggestFontName);
                                }
                                #endregion

                                parts.Add(part);
                            });
                            parts.ForEach(p => p.Parts = parts);
                        }

                        ledShortcuts = loader.LoadShortcuts(parts);
                    }
                }

                LedControl ledControl = new LedControl()
                {
                    ControlName = controlInfo.ControlName,

                    Width = width,
                    Height = height,
                    Stride = stride,
                    DotWidth = dotWidth,
                    DotHeight = dotHeight,
                    DotStride = dotStride,
                    DotDiameter = dotDiameter,
                    DotRadius = dotRadius,
                    DotXDistance = dotXDistance,
                    DotYDistance = dotYDistance,

                    Bitmap = bmp,
                    DotPixels = dots,
                    Pixels = pixels,

                    Parts = parts,

                    Shortcuts = ledShortcuts ?? new List<LedShortcut>(),
                    Fonts = ledFonts ?? new List<LedFont>(),

                    DefaultFont = defaultFont,
                    BiggestFont = biggestFont,

                    Mode = mode,

                    GetPartBitmapsForRichTextFunc = () => ledPartBitmaps,
                };
                ledControl.SetBlankFullText();
                ledControl.FullText.RegisterBitmap(calculatorGateway);

                ledControls.Add(ledControl);
                PluginConnector.AddControl(controlInfo, image);
            }

            void ThrowControlError(string message)
            {
                PluginConnector.ThrowError(message, PluginName, type, name ?? "(名前無し)");
            }
        }
    }
}