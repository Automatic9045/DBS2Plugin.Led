using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DbsPlugin.Standard.Led")]
namespace DbsPlugin.Standard.Led
{
    public interface IVisibilityConverter
    {
        int GetVisibility(string partName, List<LedPart> parts);
    }

    public interface IDisplayPhaseConverter
    {
        List<LedPartDisplayPhase> GetDisplayPhaseList(string partSystemName);
    }

    public interface ILedShortcutSetConverter
    {
        void Set(List<LedPart> parts, string arguments);
    }

    public class LedPart
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        public IEnumerable<string> SearchNames { get; internal set; }

        public List<LedPart> Parts { get; internal set; }
        internal Func<List<LedPartBitmapExtension>> GetBitmapsForRichTextFunc { get; set; }

        public int X { get; internal set; }
        public int Y { get; internal set; }

        public List<LedPartBitmap> Bitmaps { get; internal set; }

        public bool IsDisplayed { get => DisplayingYIndex != -1; }
        public int DisplayingBitmap { get; set; } = 0;
        public int DisplayingXIndex { get; set; } = 0;
        public int DisplayingYIndex { get; set; } = -1;

        public List<LedPartDisplayPhase> DisplayPhases { get; internal set; }

        internal Func<string, List<LedPart>, int> VisibilityConverterFunc { get; set; }
        public int Visibility
        {
            get => (VisibilityConverterFunc ?? ((a, b) => 3)).Invoke(SystemName, Parts);
            internal set => VisibilityConverterFunc = (a, b) => value;
        }

        public int Width { get; internal set; }
        public int Height { get; internal set; }

        internal List<LedFont> Fonts { get; set; }
        public LedFont FreeTextDefaultFont { get; internal set; }
        public LedFont FreeTextBiggestFont { get; internal set; }
        public string FreeTextContent { get; internal set; } = "";
        public RichText FreeText { get; private set; }
        public bool UseFreeText { get; set; } = false;

        private Func<long> getElapsedMillisecondsFunc;
        internal int CurrentDisplayPhase
        {
            get
            {
                int spanTotal = 0;
                foreach (LedPartDisplayPhase displayPhase in DisplayPhases)
                {
                    spanTotal += displayPhase.Span;
                }

                long now = getElapsedMillisecondsFunc.Invoke() % spanTotal;

                int ms = 0;
                int i;
                for (i = 0; ms < now; i++)
                {
                    ms += DisplayPhases[i].Span;
                }
                return i == 0 ? 0 : i - 1;
            }
        }

        internal LedPart(Func<long> getElapsedMillisecondsFunc)
        {
            this.getElapsedMillisecondsFunc = getElapsedMillisecondsFunc;
        }

        internal void SetFreeText()
        {
            FreeText = new RichText($"{{hdef:{FreeTextBiggestFont.SystemName}}}{{font:{FreeTextDefaultFont.SystemName}}}" + FreeTextContent, Fonts, GetBitmapsForRichTextFunc.Invoke(), Width, Height);
        }

        internal void SetBlankFreeText()
        {
            FreeText = new RichText(Width, Height);
        }
    }

    public class LedPartBitmap
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        public IEnumerable<string> SearchNames { get; internal set; }
        public string Path { get; internal set; }
        public byte[] Pixels { get; internal set; }
        public List<LedPartDefinition> Definitions { get; internal set; }
        public int Height { get; internal set; }
        public int Width { get; internal set; }
    }

    public class LedPartDefinition
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        internal IEnumerable<string> SearchNames { get; set; }
        public bool Flash { get; internal set; }
    }

    public class LedPartDisplayPhase
    {
        public int Span { get; set; } // [ms]
        public int VisibilityIndex { get; set; } // Visibilityでの設定の何番が表示されるか。ビットの論理和
        public int FaceIndex { get; set; } // DisplayingXIndex
    }
}
