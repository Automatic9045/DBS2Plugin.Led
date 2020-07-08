using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using static System.IO.Path;
using Drawing = System.Drawing;
using System.Drawing.Imaging;
using Imaging = System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
using Prism.Commands;

using DepartureBoardSimulator;

namespace DbsPlugin.Standard.Led
{
    public class Led : DbsPluginBase
    {
        protected override DbsPluginConnector PluginConnector { get; }

        private List<LedControlInfo> ledControls = new List<LedControlInfo>();
        private Dictionary<string, IVisibilityConverter> visibilityConverters = new Dictionary<string, IVisibilityConverter>();
        private Dictionary<string, IDisplayPhaseConverter> displayPhaseConverters = new Dictionary<string, IDisplayPhaseConverter>();
        private Dictionary<string, ISetConverter> setConverters = new Dictionary<string, ISetConverter>();
        private LedEditingUserControl editingUserControl = new LedEditingUserControl();

        private static readonly bool isX64 = Environment.Is64BitProcess;
        private Dispatcher dispatcher;
        private static string dllPath = typeof(IVisibilityConverter).Assembly.Location;
        private static string tempDllPath;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private bool changeDef = true;

        public Led(DbsPluginConnector pluginConnector) : base(pluginConnector)
        {
            PluginConnector = pluginConnector;
            dispatcher = PluginConnector.GetDispatcher();

            #region 編集ウィンドウのコマンド群の定義
            editingUserControl.VM.ShortcutSelectionChanged = new DelegateCommand(() =>
            {

            });

            editingUserControl.VM.PartSelectionChanged = new DelegateCommand(() =>
            {
                if (editingUserControl.VM.PartCur != -1)
                {
                    List<LedPart> parts = ledControls.Find(c => c.ControlName == PluginConnector.EditingControl.ControlName).LedParts;
                    UpdateGroupEditing(parts);
                    LedPart part = parts.Find(p => p.Name == editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur]);
                    if (part != null && part.DisplayingYIndex != -1)
                    {
                        //MessageBox.Show("" + part.DisplayingImage + ", " + part.DisplayingYIndex);
                        editingUserControl.VM.DefCur = editingUserControl.VM.DefCollection.IndexOf(part.BasedBytes[part.DisplayingImage].DefinitionNames[part.DisplayingYIndex].Name);
                    }
                    else
                        editingUserControl.VM.DefCur = 0;
                }
            });

            editingUserControl.VM.GroupSelectionChanged = new DelegateCommand(() =>
            {
                if (editingUserControl.VM.GroupCur != -1 && !(editingUserControl.VM.GroupCollection[0] == "(該当無し)" && editingUserControl.VM.PartCollection[0] == "(該当無し)"))
                {
                    LedPart part = ledControls.Find(c => c.ControlName == PluginConnector.EditingControl.ControlName).LedParts.Find(p => p.Name == editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur]);
                    UpdateDefEditing(part, part.BasedBytes);
                }
            });

            editingUserControl.VM.DefSelectionChanged = new DelegateCommand(() =>
            {
                if (editingUserControl.VM.DefCur != -1 && changeDef && editingUserControl.VM.DefCollection[0] != "(該当無し)")
                {
                    LedPart part = ledControls.Find(c => c.ControlName == PluginConnector.EditingControl.ControlName).LedParts.First(p => p.Name == editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur]);
                    part.DisplayingImage = editingUserControl.VM.Groups.FindIndex(g => g.Name == editingUserControl.VM.GroupCollection[editingUserControl.VM.GroupCur]);
                    part.DisplayingYIndex = editingUserControl.VM.Defs.FindIndex(d => d.Name == editingUserControl.VM.DefCollection[editingUserControl.VM.DefCur]);
                    //MessageBox.Show(part.Name + ": " + part.DisplayingImage + ", " + part.DisplayingYIndex);
                }
            });


            editingUserControl.VM.ShortcutTextChanged = new DelegateCommand(() =>
            {

            });

            editingUserControl.VM.PartTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(editingUserControl.VM.Part);
                int searchStringLength = searchString.Length;
                string oldPart = "";
                if (editingUserControl.VM.PartCur >= 0 && editingUserControl.VM.PartCollection.Count != 0) oldPart = editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur];
                editingUserControl.VM.PartCollection.Clear();
                editingUserControl.VM.PartCollection.AddRange(editingUserControl.VM.Parts.Where(p =>
                {
                    return p.SearchNames.Any(n =>
                    {
                        if (editingUserControl.VM.Part.Contains(';'))
                        {
                            return p.Name == editingUserControl.VM.Part.Split(';')[0];
                        }
                        else
                        {
                            bool isEmpty = searchString == "";
                            bool isMatchFromStart = false;
                            if (n.Length >= searchStringLength)
                                isMatchFromStart = n.Substring(0, searchStringLength) == searchString;
                            return isEmpty || isMatchFromStart;
                        }
                    });
                }).Select(p => p.Name));
                if (editingUserControl.VM.PartCollection.Count == 0)
                {
                    editingUserControl.VM.PartCollection.Add("(該当無し)");
                    editingUserControl.VM.PartCur = 0;
                    editingUserControl.VM.PartHelp = "(候補 0 件 / " + editingUserControl.VM.Parts.Count + " 件)";
                }
                else if (!editingUserControl.VM.PartCollection.Contains(oldPart))
                {
                    editingUserControl.VM.PartCur = 0;
                    editingUserControl.VM.PartHelp = "(候補 " + editingUserControl.VM.PartCollection.Count + " 件 / " + editingUserControl.VM.Parts.Count + " 件)";
                }
                else
                {
                    editingUserControl.VM.PartCur = editingUserControl.VM.PartCollection.IndexOf(oldPart);
                    editingUserControl.VM.PartHelp = "(候補 " + editingUserControl.VM.PartCollection.Count + " 件 / " + editingUserControl.VM.Parts.Count + " 件)";
                }
            });

            editingUserControl.VM.GroupTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(editingUserControl.VM.Group);
                int searchStringLength = searchString.Length;
                string oldGroup = "";
                if (editingUserControl.VM.GroupCur >= 0 && editingUserControl.VM.GroupCollection.Count != 0) oldGroup = editingUserControl.VM.GroupCollection[editingUserControl.VM.GroupCur];
                editingUserControl.VM.GroupCollection.Clear();
                editingUserControl.VM.GroupCollection.AddRange(editingUserControl.VM.Groups.Where(g =>
                {
                    return g.SearchNames.Any(n =>
                    {
                        if (editingUserControl.VM.Group.Contains(';'))
                        {
                            return g.Name == editingUserControl.VM.Group.Split(';')[0];
                        }
                        else
                        {
                            bool isEmpty = searchString == "";
                            bool isMatchFromStart = false;
                            if (n.Length >= searchStringLength)
                                isMatchFromStart = n.Substring(0, searchStringLength) == searchString;
                            return isEmpty || isMatchFromStart;
                        }
                    });
                }).Select(g => g.Name));
                if (editingUserControl.VM.GroupCollection.Count == 0)
                {
                    editingUserControl.VM.GroupCollection.Add("(該当無し)");
                    editingUserControl.VM.GroupCur = 0;
                    editingUserControl.VM.GroupHelp = "(候補 0 件 / " + editingUserControl.VM.Groups.Count + " 件)";
                }
                else if (!editingUserControl.VM.GroupCollection.Contains(oldGroup))
                {
                    editingUserControl.VM.GroupCur = 0;
                    editingUserControl.VM.GroupHelp = "(候補 " + editingUserControl.VM.GroupCollection.Count + " 件 / " + editingUserControl.VM.Groups.Count + " 件)";
                }
                else
                {
                    editingUserControl.VM.GroupCur = editingUserControl.VM.GroupCollection.IndexOf(oldGroup);
                    editingUserControl.VM.GroupHelp = "(候補 " + editingUserControl.VM.GroupCollection.Count + " 件 / " + editingUserControl.VM.Groups.Count + " 件)";
                }
            });

            editingUserControl.VM.DefTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(editingUserControl.VM.Def);
                int searchStringLength = searchString.Length;
                string oldDef = "";
                if (editingUserControl.VM.DefCur >= 0 && editingUserControl.VM.DefCollection.Count != 0) oldDef = editingUserControl.VM.DefCollection[editingUserControl.VM.DefCur];
                bool actualChangeDef = changeDef;
                changeDef = false;
                editingUserControl.VM.DefCollection.Clear();
                changeDef = actualChangeDef;
                if (editingUserControl.VM.Def == "" || editingUserControl.VM.Def == "n;") editingUserControl.VM.DefCollection.Add("(非表示)");
                if (editingUserControl.VM.Def != "n;")
                {
                    editingUserControl.VM.DefCollection.AddRange(editingUserControl.VM.Defs.Where(d =>
                    {
                        return d.SearchNames.Any(n =>
                        {
                            if (editingUserControl.VM.Def.Contains(';'))
                            {
                                return d.Name == editingUserControl.VM.Def.Split(';')[0];
                            }
                            else
                            {
                                bool isEmpty = searchString == "";
                                bool isMatchFromStart = false;
                                if (n.Length >= searchStringLength)
                                    isMatchFromStart = n.Substring(0, searchStringLength) == searchString;
                                return isEmpty || isMatchFromStart;
                            }
                        });
                    }).Select(d => d.Name));
                }
                if (editingUserControl.VM.DefCollection.Count == 0)
                {
                    editingUserControl.VM.DefCollection.Add("(該当無し)");
                    editingUserControl.VM.DefCur = 0;
                    editingUserControl.VM.DefHelp = "(候補 0 件 / " + (editingUserControl.VM.Defs.Count + 1) + " 件)";
                }
                else if (!editingUserControl.VM.DefCollection.Contains(oldDef))
                {
                    editingUserControl.VM.DefCur = 0;
                    editingUserControl.VM.DefHelp = "(候補 " + editingUserControl.VM.DefCollection.Count + " 件 / " + (editingUserControl.VM.Defs.Count + 1) + " 件)";
                }
                else
                {
                    editingUserControl.VM.DefCur = editingUserControl.VM.DefCollection.IndexOf(oldDef);
                    editingUserControl.VM.DefHelp = "(候補 " + editingUserControl.VM.DefCollection.Count + " 件 / " + (editingUserControl.VM.Defs.Count + 1) + " 件)";
                }
            });


            editingUserControl.VM.RunShortcut = new DelegateCommand(() =>
            {
                if (editingUserControl.VM.ShortcutCur != -1)
                {
                    LedControlInfo control = ledControls.Find(c => c.ControlName == PluginConnector.EditingControl.ControlName);
                    List<LedShortcutSetDefinition> sets = control.Shortcuts.Find(s => s.Name == editingUserControl.VM.ShortcutCollection[editingUserControl.VM.ShortcutCur]).Sets;
                    foreach (LedShortcutSetDefinition set in sets)
                    {
                        switch (set.Mode)
                        {
                            case LedShortcutSetMode.Set:
                                LedPart part = control.LedParts[set.Set.TargetIndex];
                                part.DisplayingImage = set.Set.ImageIndex;
                                part.DisplayingYIndex = set.Set.FrameIndex;
                                break;
                            case LedShortcutSetMode.SetConverter:
                                set.SetConverter.Invoke(control.LedParts, set.SetConverterArgument);
                                break;
                        }
                    }
                }
            });
            #endregion

            stopwatch.Start();
            tempDllPath = Combine(PluginConnector.TempDirectory, GetFileName(dllPath));
            File.Copy(dllPath, tempDllPath, true);
        }

        private string ToSearchString(string basedString)
        {
            return Strings.StrConv(basedString, VbStrConv.Hiragana | VbStrConv.Wide | VbStrConv.Lowercase).Replace("　", "").Replace("‐", "").Replace("・", "");
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

        private void AddLedDisplay(XElement element, LedDisplayMode mode, string xmlPath)
        {
            string pluginName = this.GetType().Name;
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
                PluginName = pluginName,
                ControlType = this,
                ControlTypeName = type,
                ControlName = name,
                IsNameRequired = true,
                UseEditingWindow = mode == LedDisplayMode.Normal,
                EditingUserControl = editingUserControl,
            };
            if (controlInfo.ControlName != null)
            {
                byte[] dots = new byte[dotStride * dotHeight];
                byte[] pixels = new byte[stride * height];

                List<LedPart> parts = new List<LedPart>();
                List<LedShortcut> shortcuts = new List<LedShortcut>();
                string layoutXmlPath = Combine(xmlPath, (string)element.Attribute("Source") ?? "");
                if (mode != LedDisplayMode.Normal || layoutXmlPath == xmlPath)
                {

                }
                else if (!File.Exists(layoutXmlPath))
                {
                    ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" が見つかりませんでした。");
                }
                else
                {
                    XDocument layoutXml = XDocument.Load(layoutXmlPath);
                    XElement layout = layoutXml.Element("LedDisplayLayout");
                    if (layout == null)
                    {
                        ThrowControlError("\"" + layoutXmlPath + "\" は LED レイアウトファイルではありません。");
                    }
                    else
                    {
                        List<XElement> shorucuts = layout.Elements("Part").ToList();
                        for (int i = 0; i < shorucuts.Count; i++)
                        {
                            string partName = (string)shorucuts[i].Attribute("Name");
                            if (partName == null)
                            {
                                ThrowControlError("名前のついていない LED パーツがあります。");
                                continue;
                            }

                            string partSystemName = (string)shorucuts[i].Attribute("System");
                            if (partSystemName == null || partSystemName == "") partSystemName = partName;

                            string partSearchNamesBase = (string)shorucuts[i].Attribute("Search");
                            if (partSearchNamesBase == null || partSearchNamesBase == "") partSearchNamesBase = partName;
                            IEnumerable<string> partSearchNames = partSearchNamesBase.Split(';').Select(n => ToSearchString(n));

                            LedPart part = new LedPart((int)stopwatch.ElapsedMilliseconds)
                            {
                                Name = partName,
                                SystemName = partSystemName,
                                SearchNames = partSearchNames,
                                X = (int)((uint?)shorucuts[i].Attribute("X") ?? 0),
                                Y = (int)((uint?)shorucuts[i].Attribute("Y") ?? 0),
                                DisplayingImage = 0,

                            };

                            #region Visibilityの設定
                            int visibility = (int)((uint?)shorucuts[i].Attribute("Visibility") ?? 3);
                            string visibilityConverterSourceName = (string)shorucuts[i].Attribute("VisibilityConverterSource");
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
                                        continue;
                                    }
                                    string[] referencesBase = File.ReadAllText(visibilityConverterSourcePath).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    List<string> references = new List<string>() { tempDllPath };
                                    if (referencesBase[0].StartsWith("/// ReferenceAssemblies = "))
                                    {
                                        references.AddRange(referencesBase[0].Substring("/// ReferenceAssemblies = ".Length).Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries));
                                    }
                                    Assembly assembly = PluginConnector.CompileAssembly(visibilityConverterSourcePath, references, pluginName, type, name);
                                    if (assembly == null)
                                    {
                                        ThrowControlError("VisibilityConverter \"" + visibilityConverterSourcePath + "\" のコンパイルに失敗しました。");
                                        continue;
                                    }
                                    string iName = typeof(IVisibilityConverter).FullName;
                                    Type[] asmTypes = assembly.GetExportedTypes();
                                    foreach (Type asmType in asmTypes)
                                    {
                                        if (asmType.IsClass && !asmType.IsAbstract && asmType.GetInterface(iName) != null && (asmType.Namespace ?? "").Split('.')[0] == "DbsData")
                                        {
                                            visibilityConverter = (IVisibilityConverter)Activator.CreateInstance(asmType);
                                            visibilityConverters.Add(visibilityConverterSourcePath, visibilityConverter);
                                            break;
                                        }
                                    }
                                    if (visibilityConverter == null)
                                    {
                                        ThrowControlError("VisibilityConverter \"" + visibilityConverterSourcePath + "\" で、 IVisibilityConverter を実装し DbsData.* 名前空間に存在する有効なクラスが見つかりませんでした。");
                                        continue;
                                    }
                                }
                                else
                                {
                                    visibilityConverter = visibilityConverters[visibilityConverterSourcePath];
                                }
                                part.VisibilityConverterFunc = visibilityConverter.GetVisibility;
                            }
                            #endregion

                            #region DisplayPhaseConverterの取得
                            string displayPhaseConverterSourceName = (string)shorucuts[i].Attribute("DisplayPhaseConverterSource");
                            if (displayPhaseConverterSourceName == null)
                            {
                                ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\"で、DisplayPhaseConverter が指定されていないパーツがあります。");
                                continue;
                            }
                            string displayPhaseConverterSourcePath = Combine(GetDirectoryName(layoutXmlPath), displayPhaseConverterSourceName);
                            IDisplayPhaseConverter displayPhaseConverter = null;
                            if (!displayPhaseConverters.Any((x) => x.Key == displayPhaseConverterSourcePath))
                            {
                                if (!File.Exists(displayPhaseConverterSourcePath))
                                {
                                    ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\"で指定されている DisplayPhaseConverter \"" + displayPhaseConverterSourcePath + "\" が見つかりませんでした。");
                                    continue;
                                }
                                Assembly assembly = PluginConnector.CompileAssembly(displayPhaseConverterSourcePath, new string[] { tempDllPath }, pluginName, type, name);
                                if (assembly == null)
                                {
                                    ThrowControlError("DisplayPhaseConverter \"" + displayPhaseConverterSourcePath + "\" のコンパイルに失敗しました。");
                                    continue;
                                }
                                string iName = typeof(IDisplayPhaseConverter).FullName;
                                Type[] asmTypes = assembly.GetExportedTypes();
                                foreach (Type asmType in asmTypes)
                                {
                                    if (asmType.IsClass && !asmType.IsAbstract && asmType.GetInterface(iName) != null && (asmType.Namespace ?? "").Split('.')[0] == "DbsData")
                                    {
                                        displayPhaseConverter = (IDisplayPhaseConverter)Activator.CreateInstance(asmType);
                                        displayPhaseConverters.Add(displayPhaseConverterSourcePath, displayPhaseConverter);
                                        break;
                                    }
                                }
                                if (displayPhaseConverter == null)
                                {
                                    ThrowControlError("DisplayPhaseConverter \"" + displayPhaseConverterSourcePath + "\" で、 IDisplayPhaseConverter を実装し DbsData.* 名前空間に存在する有効なクラスが見つかりませんでした。");
                                    continue;
                                }
                            }
                            else
                            {
                                displayPhaseConverter = displayPhaseConverters[displayPhaseConverterSourcePath];
                            }
                            part.DisplayPhases = displayPhaseConverter.GetDisplayPhaseList(partSystemName);
                            #endregion

                            #region パーツの取得
                            string partDefsXmlPath = Combine(GetDirectoryName(layoutXmlPath), (string)shorucuts[i].Attribute("Source") ?? "");
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
                                    continue;
                                }

                                part.DrawWidth = (int)((uint?)partDefs.Attribute("Width") ?? 0);
                                part.DrawHeight = (int)((uint?)partDefs.Attribute("Height") ?? 0);

                                part.BasedBytes = new List<LedPartBasedBytesInfo>();

                                XElement[] images = partDefs.Elements("Image").ToArray();
                                int imagesCount = images.Length;
                                for (int p = 0; p < imagesCount; p++)
                                {
                                    string imageName = (string)images[p].Attribute("Name");
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
                                    
                                    string imageSystemName = (string)images[p].Attribute("System");
                                    if (imageSystemName == null || imageSystemName == "") imageSystemName = imageName;

                                    string imageSearchNamesBase = (string)images[p].Attribute("Search");
                                    if (imageSearchNamesBase == null || imageSearchNamesBase == "") imageSearchNamesBase = imageName;
                                    IEnumerable<string> imageSearchNames = imageSearchNamesBase.Split(';').Select(n => ToSearchString(n));

                                    IEnumerable<XElement> definitions = images[p].Elements("Definition");
                                    List<LedPartDefinitionNamesInfo> definitionNames = new List<LedPartDefinitionNamesInfo>();
                                    foreach (XElement definition in definitions)
                                    {
                                        string definitionName = (string)definition.Attribute("Name");
                                        if (definitionName == null)
                                        {
                                            ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で、名前付けがされていないコマがあります。");
                                            break;
                                        }

                                        string definitionSystemName = (string)definition.Attribute("System");
                                        if (definitionSystemName == null || definitionSystemName == "") definitionSystemName = definitionName;

                                        string definitionSearchNamesBase = (string)definition.Attribute("Search");
                                        if (definitionSearchNamesBase == null || definitionSearchNamesBase == "") definitionSearchNamesBase = definitionName;
                                        IEnumerable<string> definitionSearchNames = definitionSearchNamesBase.Split(';').Select(n => ToSearchString(n));

                                        LedPartDefinitionNamesInfo definitionNamesInfo = new LedPartDefinitionNamesInfo()
                                        {
                                            Name = definitionName,
                                            SystemName = definitionSystemName,
                                            SearchNames = definitionSearchNames,
                                        };
                                        definitionNames.Add(definitionNamesInfo);
                                    }

                                    string sourcePath = Combine(GetDirectoryName(partDefsXmlPath), (string)images[p].Attribute("Source") ?? "");
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
                                        LedPartBasedBytesInfo basedBytesInfo = new LedPartBasedBytesInfo()
                                        {
                                            Name = imageName,
                                            SystemName = imageSystemName,
                                            SearchNames = imageSearchNames,
                                            Bytes = sourceBmpBytes,
                                            DefinitionNames = definitionNames,
                                            Width = sourceBmpData.Width,
                                            Height = sourceBmpData.Height,
                                        };
                                        part.BasedBytes.Add(basedBytesInfo);
                                        sourceBmp.UnlockBits(sourceBmpData);
                                        if (part.DisplayPhases.Any(dp => (dp.FaceIndex + 1) * part.DrawWidth > basedBytesInfo.Width))
                                        {
                                            ThrowControlError("LED パーツファイル \"" + partDefsXmlPath + "\" で指定されているパーツ画像 \"" + sourcePath + "\" は、必要な数のフェイスがありません。継続すると不正なメモリアクセスが発生し、描画が乱れる可能性があります。");
                                        }
                                        if (definitionNames.Count * part.DrawHeight > basedBytesInfo.Height)
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

                            parts.Add(part);
                        }
                        foreach (LedPart p in parts)
                        {
                            p.LedParts = parts;
                        }

                        #region ショートカットファイルの取得
                        IEnumerable<XElement> shortcutDefinitionElements = layout.Element("ShortcutDefinitions").Elements("ShortcutDefinition");
                        foreach (XElement shortcutDefinitionElement in shortcutDefinitionElements)
                        {
                            string shortcutSourceName = (string)shortcutDefinitionElement.Attribute("Source");
                            if (shortcutSourceName == null)
                            {
                                ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\" で、ファイルパスが指定されていないショートカット定義があります。");
                                continue;
                            }

                            string shortcutSourcePath = Combine(GetDirectoryName(layoutXmlPath), shortcutSourceName);
                            if (!File.Exists(shortcutSourcePath))
                            {
                                ThrowControlError("LED レイアウトファイル \"" + layoutXmlPath + "\"で指定されている ショートカットファイル \"" + shortcutSourcePath + "\" が見つかりませんでした。");
                                continue;
                            }
                            
                            IEnumerable<XElement> shortcutElements = XDocument.Load(shortcutSourcePath).Element("LedShortcuts").Elements("Shortcut");
                            foreach (XElement shortcutElement in shortcutElements)
                            {
                                string shortcutName = (string)shortcutElement.Attribute("Name");
                                if (shortcutName == null)
                                {
                                    ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で、名前が指定されていないショートカットがあります。");
                                }

                                List<LedShortcutSetDefinition> shortcutSets = new List<LedShortcutSetDefinition>();

                                #region Set
                                IEnumerable<XElement> setElements = shortcutElement.Elements("Set");
                                foreach (XElement setElement in setElements)
                                {
                                    LedShortcutSet set = new LedShortcutSet(parts);

                                    int? targetIndex = (int?)setElement.Attribute("TargetIndex");
                                    string targetName = (string)setElement.Attribute("Target");
                                    if (targetIndex != null)
                                    {
                                        if (0 <= targetIndex && targetIndex < parts.Count)
                                            set.TargetIndex = (int)targetIndex;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないパーツ ID \"" + targetIndex + "\" が指定されています。");
                                            continue;
                                        }
                                    }
                                    else if (targetName != null)
                                    {
                                        if (parts.Any(p => p.SystemName == targetName))
                                            set.TargetSystemName = targetName;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないパーツ システム名 \"" + targetName + "\" が指定されています。");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のパーツが指定されていない Set があります。");
                                        continue;
                                    }
                                    
                                    int? imageIndex = (int?)setElement.Attribute("ImageIndex");
                                    string imageName = (string)setElement.Attribute("Image");
                                    if (targetIndex != null)
                                    {
                                        if (0 <= imageIndex && imageIndex < parts[set.TargetIndex].BasedBytes.Count)
                                            set.ImageIndex = (int)imageIndex;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないグループ ID \"" + imageIndex + "\" （パーツ：\"" + set.TargetSystemName + "\" ）が指定されています。");
                                            continue;
                                        }
                                    }
                                    else if (imageName != null)
                                    {
                                        if (parts[set.TargetIndex].BasedBytes.Any(p => p.SystemName == imageName))
                                            set.ImageSystemName = imageName;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないグループ システム名 \"" + imageName + "\" （パーツ：\"" + set.TargetSystemName + "\" ）が指定されています。");
                                            continue;
                                        }
                                    }
                                    else if (parts[set.TargetIndex].BasedBytes.Count == 1)
                                    {
                                        set.ImageIndex = 0;
                                    }
                                    else
                                    {
                                        ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のグループが指定されていない Set があります。変更先のグループの指定が不要なのは、そのパーツにグループが１つしか存在しないときのみです。");
                                        continue;
                                    }

                                    int? frameIndex = (int?)setElement.Attribute("FrameIndex");
                                    string frameName = (string)setElement.Attribute("Frame");
                                    if (frameIndex != null)
                                    {
                                        if (-1 <= frameIndex && frameIndex < parts[set.TargetIndex].BasedBytes[set.ImageIndex].DefinitionNames.Count)
                                            set.FrameIndex = (int)frameIndex;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないコマ ID \"" + frameIndex + "\" （パーツ：\"" + set.TargetSystemName + "\"、グループ：\"" + set.ImageSystemName + "\" ）が指定されています。");
                                            continue;
                                        }
                                    }
                                    else if (frameName == ";Null;")
                                    {
                                        set.FrameIndex = -1;
                                    }
                                    else if (frameName != null)
                                    {
                                        if (parts[set.TargetIndex].BasedBytes[set.ImageIndex].DefinitionNames.Any(p => p.SystemName == frameName))
                                            set.FrameSystemName = frameName;
                                        else
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、存在しないコマ システム名 \"" + frameName + "\" （パーツ：\"" + set.TargetSystemName + "\"、グループ：\"" + set.ImageSystemName + "\" ）が指定されています。");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で定義されているショートカット \"" + shortcutName + "\" で、変更先のコマが指定されていない Set があります。");
                                        continue;
                                    }

                                    shortcutSets.Add(new LedShortcutSetDefinition() { Mode = LedShortcutSetMode.Set, Set = set });
                                }
                                #endregion

                                #region SetConverter
                                IEnumerable<XElement> setConverterElements = shortcutElement.Elements("SetConverter");
                                foreach (XElement setConverterElement in setConverterElements)
                                {
                                    string setConverterSourceName = (string)setConverterElement.Attribute("Source");
                                    if (setConverterSourceName == null)
                                    {
                                        ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\" で、ファイルパスが指定されていない SetConverter があります。");
                                        continue;
                                    }

                                    string setConverterSourcePath = Combine(GetDirectoryName(shortcutSourcePath), setConverterSourceName);
                                    ISetConverter setConverter = null;
                                    if (!setConverters.Any((x) => x.Key == setConverterSourcePath))
                                    {
                                        if (!File.Exists(setConverterSourcePath))
                                        {
                                            ThrowControlError("LED ショートカットファイル \"" + shortcutSourcePath + "\"で指定されている SetConverter \"" + setConverterSourcePath + "\" が見つかりませんでした。");
                                            continue;
                                        }
                                        Assembly assembly = PluginConnector.CompileAssembly(setConverterSourcePath, new string[] { tempDllPath }, pluginName, type, name);
                                        if (assembly == null)
                                        {
                                            ThrowControlError("SetConverter \"" + setConverterSourcePath + "\" のコンパイルに失敗しました。");
                                            continue;
                                        }
                                        string iName = typeof(ISetConverter).FullName;
                                        Type[] asmTypes = assembly.GetExportedTypes();
                                        foreach (Type asmType in asmTypes)
                                        {
                                            if (asmType.IsClass && !asmType.IsAbstract && asmType.GetInterface(iName) != null && (asmType.Namespace ?? "").Split('.')[0] == "DbsData")
                                            {
                                                setConverter = (ISetConverter)Activator.CreateInstance(asmType);
                                                setConverters.Add(setConverterSourcePath, setConverter);
                                                break;
                                            }
                                        }
                                        if (setConverter == null)
                                        {
                                            ThrowControlError("SetConverter \"" + setConverterSourcePath + "\" で、 ISetConverter を実装し DbsData.* 名前空間に存在する有効なクラスが見つかりませんでした。");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        setConverter = setConverters[setConverterSourcePath];
                                    }
                                    shortcutSets.Add(new LedShortcutSetDefinition() { Mode = LedShortcutSetMode.SetConverter, SetConverter = setConverter.Set });
                                }
                                #endregion

                                shortcuts.Add(new LedShortcut() { Name = shortcutName, Sets = shortcutSets });
                            }
                        }
                        #endregion
                    }
                }

                ledControls.Add(new LedControlInfo()
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
                    DotBytes = dots,
                    PixelBytes = pixels,

                    LedParts = parts,

                    Shortcuts = shortcuts,

                    Mode = mode,
                });
            }
            PluginConnector.AddControl(controlInfo, image);

            void ThrowControlError(string message)
            {
                PluginConnector.ThrowError(message, pluginName, type, name ?? "(名前無し)");
            }
        }

        public override void Tick()
        {
            File.Delete(tempDllPath);
            stopwatch.Stop();

            Task.Run(async () =>
            {
                int n = 0, count = 0;
                var sw = new System.Diagnostics.Stopwatch();

                int b;

                byte[] dots, pixels;

                int width, height, stride;

                int dotWidth, dotHeight, dotStride, dotDiameter;
                double dotRadius, dotXDistance, dotYDistance;

                WriteableBitmap bitmap;

                List<LedPart> parts;

                Dictionary<string, byte[]> partSources = new Dictionary<string, byte[]>();

                LedDisplayMode mode;

                int numberOfControls = ledControls.Count;

                sw.Start();
                while (true)
                {
                    await Task.Delay(1);
                    for (int controlCount = 0; controlCount < numberOfControls; controlCount++)
                    {
                        mode = ledControls[controlCount].Mode;

                        dots = ledControls[controlCount].DotBytes;
                        pixels = ledControls[controlCount].PixelBytes;

                        width = ledControls[controlCount].Width;
                        height = ledControls[controlCount].Height;
                        stride = ledControls[controlCount].Stride;

                        dotWidth = ledControls[controlCount].DotWidth;
                        dotHeight = ledControls[controlCount].DotHeight;
                        dotStride = ledControls[controlCount].DotStride;
                        dotDiameter = ledControls[controlCount].DotDiameter;
                        dotRadius = ledControls[controlCount].DotRadius;
                        dotXDistance = ledControls[controlCount].DotXDistance;
                        dotYDistance = ledControls[controlCount].DotYDistance;

                        bitmap = ledControls[controlCount].Bitmap;
                        parts = ledControls[controlCount].LedParts;


                        switch (mode)
                        {
                            case LedDisplayMode.Fps:
                                #region FPS表示
                                b = n / 100;
                                Parallel.For(0, 3, a =>
                                {
                                    if (b != 1) dots[12 * a + 2] = 0xff; else dots[12 * a + 2] = 0x33;
                                    if (b != 1 && b != 4) dots[12 * a + 5] = 0xff; else dots[12 * a + 5] = 0x33;
                                    dots[12 * a + 8] = 0xff;

                                    if (b != 1 && b != 2 && b != 3 && b != 7) dots[12 * a + dotStride + 2] = 0xff; else dots[12 * a + dotStride + 2] = 0x33;
                                    dots[12 * a + dotStride + 5] = 0x33;
                                    if (b != 5 && b != 6) dots[12 * a + dotStride + 8] = 0xff; else dots[12 * a + dotStride + 8] = 0x33;

                                    if (b != 1 && b != 7) dots[12 * a + dotStride * 2 + 2] = 0xff; else dots[12 * a + dotStride * 2 + 2] = 0x33;
                                    if (b != 0 && b != 1 && b != 7) dots[12 * a + dotStride * 2 + 5] = 0xff; else dots[12 * a + dotStride * 2 + 5] = 0x33;
                                    dots[12 * a + dotStride * 2 + 8] = 0xff;

                                    if (b == 0 || b == 2 || b == 6 || b == 8) dots[12 * a + dotStride * 3 + 2] = 0xff; else dots[12 * a + dotStride * 3 + 2] = 0x33;
                                    dots[12 * a + dotStride * 3 + 5] = 0x33;
                                    if (b != 2) dots[12 * a + dotStride * 3 + 8] = 0xff; else dots[12 * a + dotStride * 3 + 8] = 0x33;

                                    if (b != 1 && b != 4 && b != 7) dots[12 * a + dotStride * 4 + 2] = 0xff; else dots[12 * a + dotStride * 4 + 2] = 0x33;
                                    if (b != 1 && b != 4 && b != 7) dots[12 * a + dotStride * 4 + 5] = 0xff; else dots[12 * a + dotStride * 4 + 5] = 0x33;
                                    dots[12 * a + dotStride * 4 + 8] = 0xff;

                                    if (a == 0) b = n / 10 - n / 100 * 10;
                                    if (a == 1) b = n - n / 10 * 10;
                                });
                                #endregion
                                break;
                            case LedDisplayMode.ElapsedMilliseconds:
                                #region 経過ミリ秒表示
                                try
                                {
                                    List<LedControlInfo> controls = ledControls.FindAll(c => c.Mode == LedDisplayMode.Normal && c.LedParts.Count != 0);
                                    int ms = 0;
                                    if (controls.Count != 0) ms = controls[0].LedParts[0].StopwatchElapsedMilliseconds;
                                    b = ms / 10000;
                                    Parallel.For(0, 5, a =>
                                    {
                                        if (b != 1) dots[12 * a + 2] = 0xff; else dots[12 * a + 2] = 0x33;
                                        if (b != 1 && b != 4) dots[12 * a + 5] = 0xff; else dots[12 * a + 5] = 0x33;
                                        dots[12 * a + 8] = 0xff;

                                        if (b != 1 && b != 2 && b != 3 && b != 7) dots[12 * a + dotStride + 2] = 0xff; else dots[12 * a + dotStride + 2] = 0x33;
                                        dots[12 * a + dotStride + 5] = 0x33;
                                        if (b != 5 && b != 6) dots[12 * a + dotStride + 8] = 0xff; else dots[12 * a + dotStride + 8] = 0x33;

                                        if (b != 1 && b != 7) dots[12 * a + dotStride * 2 + 2] = 0xff; else dots[12 * a + dotStride * 2 + 2] = 0x33;
                                        if (b != 0 && b != 1 && b != 7) dots[12 * a + dotStride * 2 + 5] = 0xff; else dots[12 * a + dotStride * 2 + 5] = 0x33;
                                        dots[12 * a + dotStride * 2 + 8] = 0xff;

                                        if (b == 0 || b == 2 || b == 6 || b == 8) dots[12 * a + dotStride * 3 + 2] = 0xff; else dots[12 * a + dotStride * 3 + 2] = 0x33;
                                        dots[12 * a + dotStride * 3 + 5] = 0x33;
                                        if (b != 2) dots[12 * a + dotStride * 3 + 8] = 0xff; else dots[12 * a + dotStride * 3 + 8] = 0x33;

                                        if (b != 1 && b != 4 && b != 7) dots[12 * a + dotStride * 4 + 2] = 0xff; else dots[12 * a + dotStride * 4 + 2] = 0x33;
                                        if (b != 1 && b != 4 && b != 7) dots[12 * a + dotStride * 4 + 5] = 0xff; else dots[12 * a + dotStride * 4 + 5] = 0x33;
                                        dots[12 * a + dotStride * 4 + 8] = 0xff;

                                        if (a == 0) b = ms / 1000 - ms / 10000 * 10;
                                        if (a == 1) b = ms / 100 - ms / 1000 * 10;
                                        if (a == 2) b = ms / 10 - ms / 100 * 10;
                                        if (a == 3) b = ms - ms / 10 * 10;
                                    });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                #endregion
                                break;
                            case LedDisplayMode.Normal:
                                #region 通常LED表示
                                if (isX64)
                                    CalculationX64.ClearDots(dots, dotWidth, dotHeight);
                                else
                                    CalculationX86.ClearDots(dots, dotWidth, dotHeight);

                                foreach (LedPart part in parts)
                                {
                                    LedPartDisplayPhase displaySetting = part.DisplayPhases[part.StopwatchCount];
                                    if ((part.Visibility & displaySetting.VisibleIndex) == displaySetting.VisibleIndex)
                                    {
                                        part.DisplayingXIndex = displaySetting.FaceIndex;

                                        if (part.DisplayingYIndex != -1)
                                        {
                                            if (isX64)
                                                CalculationX64.CopyImageToDots(dots, part.BasedBytes[part.DisplayingImage].Bytes, part.X, part.Y, part.BasedBytes[part.DisplayingImage].Width, part.BasedBytes[part.DisplayingImage].Height, part.DrawWidth, part.DrawHeight, dotWidth, dotHeight, part.DisplayingXIndex, part.DisplayingYIndex, true);
                                            else
                                                CalculationX86.CopyImageToDots(dots, part.BasedBytes[part.DisplayingImage].Bytes, part.X, part.Y, part.BasedBytes[part.DisplayingImage].Width, part.BasedBytes[part.DisplayingImage].Height, part.DrawWidth, part.DrawHeight, dotWidth, dotHeight, part.DisplayingXIndex, part.DisplayingYIndex, true);
                                        }
                                    }
                                    else
                                    {
                                        part.DisplayingXIndex = -1;
                                    }
                                }
                                #endregion
                                break;
                        }

                        if (isX64)
                            CalculationX64.DrawLedDots(pixels, dots, dotWidth, dotHeight, dotXDistance, dotYDistance, dotRadius, stride, n == 0);
                        else
                            CalculationX86.DrawLedDots(pixels, dots, dotWidth, dotHeight, dotXDistance, dotYDistance, dotRadius, stride, n == 0);
                    }

                    await dispatcher.BeginInvoke((Action)(() =>
                    {
                        foreach (LedControlInfo led in ledControls)
                        {
                            led.Bitmap.WritePixels(new Int32Rect(0, 0, led.Width, led.Height), led.PixelBytes, led.Stride, 0, 0);
                        }
                    }));

                    count++;
                    if (count == 10 || n == 0)
                    {
                        count = 0;
                        n = (int)(10000.0 / sw.ElapsedMilliseconds);
                        sw.Restart();
                    }
                }
            });
        }

        #region 編集ウィンドウ用処理
        public override void EditingUserControlShown()
        {
            string oldPartSelection = "";
            bool isFirst = false;
            if (editingUserControl.VM.PartCur != -2)
                oldPartSelection = editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur];
            else
            {
                editingUserControl.VM.PartCur = 0;
                isFirst = true;
            }
            LedControlInfo currentControl = ledControls.Find(c => c.ControlName == PluginConnector.EditingControl.ControlName);
            UpdatePartEditing(currentControl);
            if (!isFirst)
                editingUserControl.VM.PartCur = editingUserControl.VM.PartCollection.IndexOf(oldPartSelection);

            UpdateShortcutEditing(currentControl);
        }

        private void UpdateShortcutEditing(LedControlInfo currentControl)
        {
            editingUserControl.VM.Shortcuts = currentControl.Shortcuts;
            editingUserControl.VM.ShortcutCollection.Clear();
            editingUserControl.VM.ShortcutCollection.AddRange(currentControl.Shortcuts.Select(s => s.Name));
        }

        private void UpdatePartEditing(LedControlInfo currentControl)
        {
            List<LedPart> currentPart = currentControl.LedParts;
            editingUserControl.VM.Parts = currentPart;
            editingUserControl.VM.PartTextChanged.Execute();
        }

        private void UpdateGroupEditing(IEnumerable<LedPart> currentParts)
        {
            if (editingUserControl.VM.PartCollection[0] == "(該当無し)")
            {
                editingUserControl.VM.GroupCollection.Clear();
                editingUserControl.VM.GroupCollection.Add("(該当無し)");
                editingUserControl.VM.GroupCur = 0;
                editingUserControl.VM.DefCollection.Clear();
                editingUserControl.VM.DefCollection.Add("(該当無し)");
                editingUserControl.VM.DefCur = 0;
            }
            else
            {
                LedPart currentPart = currentParts.First(p => p.Name == editingUserControl.VM.PartCollection[editingUserControl.VM.PartCur]);
                List<LedPartBasedBytesInfo> currentGroups = currentPart.BasedBytes;
                editingUserControl.VM.Groups = currentGroups;
                editingUserControl.VM.GroupTextChanged.Execute();
                editingUserControl.VM.GroupCur = editingUserControl.VM.GroupCollection.IndexOf(currentGroups[currentPart.DisplayingImage].Name);
            }
        }

        private void UpdateDefEditing(LedPart currentPart, List<LedPartBasedBytesInfo> currentGroups)
        {
            changeDef = false;
            if (editingUserControl.VM.GroupCollection[0] == "(該当無し)")
            {
                editingUserControl.VM.DefCollection.Clear();
                editingUserControl.VM.DefCollection.Add("(該当無し)");
                editingUserControl.VM.DefCur = 0;
            }
            else
            {
                LedPartBasedBytesInfo currentGroup = currentGroups.FirstOrDefault(g => g.Name == editingUserControl.VM.GroupCollection[editingUserControl.VM.GroupCur]);
                List<LedPartDefinitionNamesInfo> currentDefs = currentGroup.DefinitionNames;
                editingUserControl.VM.Defs = currentDefs;
                editingUserControl.VM.DefTextChanged.Execute();
                if (currentPart.DisplayingYIndex == -1)
                {
                    editingUserControl.VM.DefCur = 0;
                }
                else if (editingUserControl.VM.Groups.FindIndex(g => g.Name == currentGroups[currentPart.DisplayingImage].Name) != currentPart.DisplayingImage)
                {
                    editingUserControl.VM.DefCur = 0;
                }
                else
                {
                    editingUserControl.VM.DefCur = editingUserControl.VM.DefCollection.IndexOf(currentDefs[currentPart.DisplayingYIndex].Name);
                }
            }
            changeDef = true;
        }
        #endregion

        #region 高速演算ライブラリの呼出定義
        static class CalculationX64
        {
            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int DrawLedDots(byte[] bmp, byte[] dots, int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool forceRedraw);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int CopyImageToDots(byte[] dots, byte[] source, int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x64.dll", CharSet = CharSet.Unicode)]
            internal static extern int ClearDots(byte[] dots, int dotWidth, int dotHeight);
        }

        static class CalculationX86
        {
            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int DrawLedDots(byte[] bmp, byte[] dots, int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool forceRedraw);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int CopyImageToDots(byte[] dots, byte[] source, int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent);

            [DllImport("Plugin/Standard.Led.Calculation.x86.dll", CharSet = CharSet.Unicode)]
            internal static extern int ClearDots(byte[] dots, int dotWidth, int dotHeight);
        }
        #endregion
    }

    internal struct LedControlInfo
    {
        internal string ControlName { get; set; }

        internal int Width { get; set; }
        internal int Height { get; set; }
        internal int Stride { get; set; }
        internal int DotWidth { get; set; }
        internal int DotHeight { get; set; }
        internal int DotStride { get; set; }
        internal int DotDiameter { get; set; }
        internal double DotRadius { get; set; }
        internal double DotXDistance { get; set; }
        internal double DotYDistance { get; set; }

        internal WriteableBitmap Bitmap { get; set; }
        internal byte[] DotBytes { get; set; }
        internal byte[] PixelBytes { get; set; }

        internal List<LedPart> LedParts { get; set; }

        internal List<LedShortcut> Shortcuts { get; set; }

        internal LedDisplayMode Mode { get; set; }
    }

    enum LedDisplayMode
    {
        Normal = 0,
        Fps,
        ElapsedMilliseconds,
        Debug,
    }

    internal struct LedShortcut
    {
        internal string Name { get; set; }
        internal List<LedShortcutSetDefinition> Sets { get; set; }
    }

    internal struct LedShortcutSetDefinition
    {
        internal LedShortcutSetMode Mode { get; set; }
        internal LedShortcutSet Set { get; set; }
        internal Action<List<LedPart>, string> SetConverter { get; set; }
        internal string SetConverterArgument { get; set; }
    }

    internal class LedShortcutSet
    {
        internal int TargetIndex { get; set; } = 0;
        internal string TargetSystemName
        {
            get
            {
                return parts[TargetIndex].SystemName;
            }
            set
            {
                TargetIndex = parts.FindIndex(p => p.SystemName == value);
            }
        }

        internal int ImageIndex { get; set; } = 0;
        internal string ImageSystemName
        {
            get
            {
                return parts[TargetIndex].BasedBytes[ImageIndex].SystemName;
            }
            set
            {
                ImageIndex = parts[TargetIndex].BasedBytes.FindIndex(p => p.SystemName == value);
            }
        }

        internal int FrameIndex { get; set; } = -1;
        internal string FrameSystemName
        {
            get
            {
                if (FrameIndex == -1)
                    return null;
                else
                    return parts[TargetIndex].BasedBytes[ImageIndex].DefinitionNames[FrameIndex].SystemName;
            }
            set
            {
                if (value == null)
                    FrameIndex = -1;
                else
                    FrameIndex = parts[TargetIndex].BasedBytes[ImageIndex].DefinitionNames.FindIndex(p => p.SystemName == value);
            }
        }

        private List<LedPart> parts;
        internal LedShortcutSet(List<LedPart> parts)
        {
            this.parts = parts;
        }
    }

    internal enum LedShortcutSetMode
    {
        Set = 0,
        SetConverter
    }
}