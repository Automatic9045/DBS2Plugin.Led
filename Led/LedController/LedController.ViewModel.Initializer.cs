using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

using Prism.Mvvm;
using Prism.Commands;

using static DbsPlugin.Standard.Led.Led;
using DbsPlugin.Standard.Led.RichTextEditorViewModels;

namespace DbsPlugin.Standard.Led
{
    public partial class LedControllerViewModel : BindableBase
    {
        private bool hasDefChanged = true;

        private void Initialize(Action focusToFreeTextTextBoxAction, Action focusToFullTextTextBoxAction)
        {
            ShortcutSelectionChanged = new DelegateCommand(() =>
            {

            });

            PartSelectionChanged = new DelegateCommand(() =>
            {
                if (PartCur != -1)
                {
                    List<LedPart> parts = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName).Parts;
                    UpdateGroupEditing(parts);
                    LedPart part = parts.Find(p => p.Name == PartCollection[PartCur]);
                    if (!(part is null) && part.DisplayingYIndex != -1)
                    {
                        //MessageBox.Show("" + part.DisplayingImage + ", " + part.DisplayingYIndex);
                        DefCur = DefCollection.IndexOf(part.Bitmaps[part.DisplayingBitmapIndex].Definitions[part.DisplayingYIndex].Name);
                    }
                    else
                        DefCur = 0;
                }
            });

            GroupSelectionChanged = new DelegateCommand(() =>
            {
                if (GroupCur != -1 && !(GroupCollection[0] == "(該当無し)" && PartCollection[0] == "(該当無し)"))
                {
                    LedPart part = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName).Parts.Find(p => p.Name == PartCollection[PartCur]);
                    UpdateDefEditing(part, part.Bitmaps);
                }
            });

            DefSelectionChanged = new DelegateCommand(() =>
            {
                if (DefCur != -1 && hasDefChanged && DefCollection[0] != "(該当無し)")
                {
                    LedPart part = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName).Parts.First(p => p.Name == PartCollection[PartCur]);
                    part.DisplayingBitmapIndex = Groups.FindIndex(g => g.Name == GroupCollection[GroupCur]);
                    part.DisplayingYIndex = Defs.FindIndex(d => d.Name == DefCollection[DefCur]);
                    UpdatePreview(part);
                }
            });


            ShortcutTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(Shortcut);
                int searchStringLength = searchString.Length;
                string oldShortcut = "";
                if (ShortcutCur >= 0 && ShortcutCollection.Count != 0) oldShortcut = ShortcutCollection[ShortcutCur];
                ShortcutCollection.Clear();
                ShortcutCollection.AddRange(Shortcuts.Where(p =>
                {
                    return p.SearchNames.Any(n =>
                    {
                        if (Shortcut.Contains(';'))
                        {
                            return p.Name == Shortcut.Split(';')[0];
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
                ShortcutIsEnabled = ShortcutCollection.Count != 0;
                if (ShortcutCollection.Count == 0)
                {
                    ShortcutCollection.Add("(該当無し)");
                    ShortcutCur = 0;
                    ShortcutHelp = "(候補 0 件 / " + Shortcuts.Count + " 件)";
                }
                else if (!ShortcutCollection.Contains(oldShortcut))
                {
                    ShortcutCur = 0;
                    ShortcutHelp = "(候補 " + ShortcutCollection.Count + " 件 / " + Shortcuts.Count + " 件)";
                }
                else
                {
                    ShortcutCur = ShortcutCollection.IndexOf(oldShortcut);
                    ShortcutHelp = "(候補 " + ShortcutCollection.Count + " 件 / " + Shortcuts.Count + " 件)";
                }
            });

            PartTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(Part);
                int searchStringLength = searchString.Length;
                string oldPart = "";
                if (PartCur >= 0 && PartCollection.Count != 0) oldPart = PartCollection[PartCur];
                PartCollection.Clear();
                PartCollection.AddRange(Parts.Where(p =>
                {
                    return p.SearchNames.Any(n =>
                    {
                        if (Part.Contains(';'))
                        {
                            return p.Name == Part.Split(';')[0];
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
                if (PartCollection.Count == 0)
                {
                    PartCollection.Add("(該当無し)");
                    PartCur = 0;
                    PartHelp = "(候補 0 件 / " + Parts.Count + " 件)";
                }
                else if (!PartCollection.Contains(oldPart))
                {
                    PartCur = 0;
                    PartHelp = "(候補 " + PartCollection.Count + " 件 / " + Parts.Count + " 件)";
                }
                else
                {
                    PartCur = PartCollection.IndexOf(oldPart);
                    PartHelp = "(候補 " + PartCollection.Count + " 件 / " + Parts.Count + " 件)";
                }
            });

            GroupTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(Group);
                int searchStringLength = searchString.Length;
                string oldGroup = "";
                if (GroupCur >= 0 && GroupCollection.Count != 0) oldGroup = GroupCollection[GroupCur];
                GroupCollection.Clear();
                GroupCollection.AddRange(Groups.Where(g =>
                {
                    return g.SearchNames.Any(n =>
                    {
                        if (Group.Contains(';'))
                        {
                            return g.Name == Group.Split(';')[0];
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
                if (GroupCollection.Count == 0)
                {
                    GroupCollection.Add("(該当無し)");
                    GroupCur = 0;
                    GroupHelp = "(候補 0 件 / " + Groups.Count + " 件)";
                }
                else if (!GroupCollection.Contains(oldGroup))
                {
                    GroupCur = 0;
                    GroupHelp = "(候補 " + GroupCollection.Count + " 件 / " + Groups.Count + " 件)";
                }
                else
                {
                    GroupCur = GroupCollection.IndexOf(oldGroup);
                    GroupHelp = "(候補 " + GroupCollection.Count + " 件 / " + Groups.Count + " 件)";
                }
            });

            DefTextChanged = new DelegateCommand(() =>
            {
                string searchString = ToSearchString(Def);
                int searchStringLength = searchString.Length;
                string oldDef = "";
                if (DefCur >= 0 && DefCollection.Count != 0) oldDef = DefCollection[DefCur];
                bool actualChangeDef = hasDefChanged;
                hasDefChanged = false;
                DefCollection.Clear();
                hasDefChanged = actualChangeDef;
                if (Def == "" || Def == "n;") DefCollection.Add("(非表示)");
                if (Def != "n;")
                {
                    DefCollection.AddRange(Defs.Where(d =>
                    {
                        return d.SearchNames.Any(n =>
                        {
                            if (Def.Contains(';'))
                            {
                                return d.Name == Def.Split(';')[0];
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
                if (DefCollection.Count == 0)
                {
                    DefCollection.Add("(該当無し)");
                    DefCur = 0;
                    DefHelp = "(候補 0 件 / " + (Defs.Count + 1) + " 件)";
                }
                else if (!DefCollection.Contains(oldDef))
                {
                    DefCur = 0;
                    DefHelp = "(候補 " + DefCollection.Count + " 件 / " + (Defs.Count + 1) + " 件)";
                }
                else
                {
                    DefCur = DefCollection.IndexOf(oldDef);
                    DefHelp = "(候補 " + DefCollection.Count + " 件 / " + (Defs.Count + 1) + " 件)";
                }
            });

            FreeTextTextChanged = new DelegateCommand(() =>
            {
                IsNotFreeTextContentLatest = true;

                LedPart currentPart = Parts.Find(p => p.Name == PartCollection[PartCur]);

                currentPart.FreeTextContent = FreeText;
            });

            FullTextTextChanged = new DelegateCommand(() =>
            {
                IsNotFullTextContentLatest = true;

                LedControl currentControl = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);

                currentControl.FullTextContent = FullText;
            });

            RunShortcut = new DelegateCommand(() =>
            {
                if (ShortcutCur != -1)
                {
                    LedControl control = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);
                    List<ILedShortcutSet> sets = control.Shortcuts.Find(s => s.Name == ShortcutCollection[ShortcutCur]).Sets;
                    foreach (ILedShortcutSet set in sets)
                    {
                        if (set is LedShortcutSet)
                        {
                            LedShortcutSet staticSet = (LedShortcutSet)set;

                            LedPart part = control.Parts[staticSet.TargetIndex];
                            part.DisplayingBitmapIndex = staticSet.ImageIndex;
                            part.DisplayingYIndex = staticSet.FrameIndex;
                        }
                        else if (set is LedShortcutSetConverter)
                        {
                            LedShortcutSetConverter dynamicSet = (LedShortcutSetConverter)set;

                            dynamicSet.SetConverter.Invoke(control.Parts, dynamicSet.SetConverterArgument);
                        }
                        else
                        {
                            throw new NotSupportedException("ILedShortcutSet としてサポートされない型です。");
                        }
                    }
                }
            });

            RunFreeText = new DelegateCommand(() =>
            {
                if (PartCur >= 0 && PartCollection[0] != "(該当無し)")
                {
                    IsNotFreeTextContentLatest = false;

                    LedPart currentPart = Parts.Find(p => p.Name == PartCollection[PartCur]);

                    currentPart.SetFreeText();
                    currentPart.FreeText.RegisterBitmap(connector.CalculatorGateway);

                    byte[] dots = new byte[currentPart.FreeText.Width * currentPart.FreeText.Height * 3];
                    connector.CalculatorGateway.CopyDots(
                        dots, currentPart.FreeText.StableBitmap,
                        currentPart.FreeText.Width, currentPart.FreeText.Height,
                        0, 0,
                        currentPart.FreeText.Width, currentPart.FreeText.Height,
                        0, 0,
                        currentPart.FreeText.Width, currentPart.FreeText.Height,
                        false);
                    if (currentPart.FreeText.HasFlashElement)
                    {
                        connector.CalculatorGateway.CopyDots(
                            dots, currentPart.FreeText.FlashBitmap,
                            currentPart.FreeText.Width, currentPart.FreeText.Height,
                            0, 0,
                            currentPart.FreeText.Width, currentPart.FreeText.Height,
                            0, 0,
                            currentPart.FreeText.Width, currentPart.FreeText.Height,
                            true);
                    }
                    UpdatePreview(dots, currentPart.FreeText.Width, currentPart.FreeText.Height);
                }
            });

            RunFullText = new DelegateCommand(() =>
            {
                IsNotFullTextContentLatest = false;

                LedControl currentControl = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);

                currentControl.SetFullText();
                currentControl.FullText.RegisterBitmap(connector.CalculatorGateway);
                if (currentControl.ScrollFullText) currentControl.FunnTextScrollStartedTime = connector.PluginConnector.ElapsedMilliseconds;

                byte[] dots = new byte[currentControl.FullText.Width * currentControl.FullText.Height * 3];
                connector.CalculatorGateway.CopyDots(
                    dots, currentControl.FullText.StableBitmap,
                    currentControl.FullText.Width, currentControl.FullText.Height,
                    0, 0,
                    currentControl.FullText.Width, currentControl.FullText.Height,
                    0, 0,
                    currentControl.FullText.Width, currentControl.FullText.Height,
                    false);
                if (currentControl.FullText.HasFlashElement)
                {
                    connector.CalculatorGateway.CopyDots(
                        dots, currentControl.FullText.FlashBitmap,
                        currentControl.FullText.Width, currentControl.FullText.Height,
                        0, 0,
                        currentControl.FullText.Width, currentControl.FullText.Height,
                        0, 0,
                        currentControl.FullText.Width, currentControl.FullText.Height,
                        true);
                }
                UpdatePreview(dots, currentControl.FullText.Width, currentControl.FullText.Height);
            });

            UseFreeTextChanged = new DelegateCommand(() =>
            {
                if (PartCur >= 0 && PartCollection[0] != "(該当無し)")
                {
                    LedPart currentPart = Parts.Find(p => p.Name == PartCollection[PartCur]);

                    currentPart.UseFreeText = UseFreeText;
                }
            });

            UseFullTextChanged = new DelegateCommand(() =>
            {
                LedControl currentControl = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);

                currentControl.UseFullText = UseFullText;
            });

            FullTextScrollChanged = new DelegateCommand(() =>
            {
                LedControl currentControl = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);

                currentControl.ScrollFullText = FullTextScroll;
                currentControl.FunnTextScrollStartedTime = connector.PluginConnector.ElapsedMilliseconds;
            });

            FreeTextEditor = new RichTextEditorViewModel(command =>
            {
                int oldStart = FreeTextSelectionStart;
                FreeText = FreeText.Remove(FreeTextSelectionStart, FreeTextSelectionLength).Insert(FreeTextSelectionStart, command);
                FreeTextSelectionStart = oldStart + command.Length;
                focusToFreeTextTextBoxAction.Invoke();
            });
            FullTextEditor = new RichTextEditorViewModel(command =>
            {
                int oldStart = FullTextSelectionStart;
                FullText = FullText.Remove(FullTextSelectionStart, FullTextSelectionLength).Insert(FullTextSelectionStart, command);
                FullTextSelectionStart = oldStart + command.Length;
                focusToFullTextTextBoxAction.Invoke();
            });
        }

        public void Shown()
        {
            string oldPartSelection = "";
            bool isFirst = false;
            //if (PartCur != -2)
            if (PartCur > 0)
            {
                oldPartSelection = PartCollection[PartCur];
            }
            else
            {
                PartCur = 0;
                isFirst = true;
            }

            LedControl currentControl = connector.LedControls.Find(c => c.ControlName == connector.PluginConnector.Controller.ControlName);

            UpdatePartEditing(currentControl);
            if (!isFirst)
                PartCur = PartCollection.IndexOf(oldPartSelection);

            UpdateShortcutEditing(currentControl);
        }

        private void UpdateShortcutEditing(LedControl currentControl)
        {
            Shortcuts = currentControl.Shortcuts;
            ShortcutCollection.Clear();
            ShortcutCollection.AddRange(currentControl.Shortcuts.Select(s => s.Name));
            ShortcutTextChanged.Execute();
        }

        private void UpdatePartEditing(LedControl currentControl)
        {
            List<LedPart> currentParts = currentControl.Parts;
            Parts = currentParts;

            PartTextChanged.Execute();

            FullText = currentControl.FullTextContent;
            UseFullText = currentControl.UseFullText;
            FullTextScroll = currentControl.ScrollFullText;
            FullTextEditor.UpdateFonts(currentControl.Fonts);
        }

        private void UpdateGroupEditing(IEnumerable<LedPart> currentParts)
        {
            if (PartCollection[0] == "(該当無し)")
            {
                GroupCollection.Clear();
                GroupCollection.Add("(該当無し)");
                GroupCur = 0;
                DefCollection.Clear();
                DefCollection.Add("(該当無し)");
                DefCur = 0;

                FreeTextIsEnabled = false;
            }
            else
            {
                LedPart currentPart = currentParts.First(p => p.Name == PartCollection[PartCur]);

                PreviewWidth = currentPart.Width;
                PreviewHeight = currentPart.Height;

                List<LedPartBitmap> currentGroups = currentPart.Bitmaps;
                Groups = currentGroups;
                GroupTextChanged.Execute();
                GroupCur = GroupCollection.IndexOf(currentGroups[currentPart.DisplayingBitmapIndex].Name);

                FreeText = currentPart.FreeTextContent;
                UseFreeText = currentPart.UseFreeText;
                FreeTextEditor.UpdateFonts(currentPart.Fonts);
                FreeTextIsEnabled = true;
            }
        }

        private void UpdateDefEditing(LedPart currentPart, List<LedPartBitmap> currentGroups)
        {
            hasDefChanged = false;
            if (GroupCollection[0] == "(該当無し)")
            {
                DefCollection.Clear();
                DefCollection.Add("(該当無し)");
                DefCur = 0;
            }
            else
            {
                LedPartBitmap currentGroup = currentGroups.FirstOrDefault(g => g.Name == GroupCollection[GroupCur]);
                List<LedPartDefinition> currentDefs = currentGroup.Definitions;
                Defs = currentDefs;
                DefTextChanged.Execute();
                if (currentPart.DisplayingYIndex == -1)
                {
                    DefCur = 0;
                }
                else if (Groups.FindIndex(g => g.Name == currentGroups[currentPart.DisplayingBitmapIndex].Name) != currentPart.DisplayingBitmapIndex)
                {
                    DefCur = 0;
                }
                else
                {
                    DefCur = DefCollection.IndexOf(currentDefs[currentPart.DisplayingYIndex].Name);
                    UpdatePreview(currentPart);
                }
            }
            hasDefChanged = true;
        }

        private void UpdatePreview(byte[] dots, int dotWidth, int dotHeight)
        {
            int bitmapWidth = dotWidth * 3 + 1;
            int bitmapHeight = dotHeight * 3 + 1;
            int bitmapStride = bitmapWidth * 4;

            byte[] bitmap = new byte[bitmapStride * bitmapHeight];
            connector.CalculatorGateway.Draw(bitmap, dots, dotWidth, dotHeight, 1, 1, 1, bitmapStride, true);

            Preview = BitmapSource.Create(bitmapWidth, bitmapHeight, 96.0, 96.0, PixelFormats.Bgr32, null, bitmap, bitmapStride);
            PreviewWidth = dotWidth;
            PreviewHeight = dotHeight;
        }

        private void UpdatePreview(LedPart currentPart)
        {
            int baseWidth = currentPart.Bitmaps[currentPart.DisplayingBitmapIndex].Width;
            int baseHeight = currentPart.Bitmaps[currentPart.DisplayingBitmapIndex].Height;

            int dotWidth = baseWidth;
            int dotHeight = currentPart.Height;

            byte[] dots = new byte[dotWidth * dotHeight * 3];
            connector.CalculatorGateway.ClearDots(dots, dotWidth, dotHeight);
            connector.CalculatorGateway.WriteImageToDots(dots, currentPart.Bitmaps[currentPart.DisplayingBitmapIndex].Pixels, 0, 0, baseWidth, baseHeight, dotWidth, dotHeight, dotWidth, dotHeight, 0, currentPart.DisplayingYIndex, false);

            UpdatePreview(dots, dotWidth, dotHeight);
        }
    }
}
