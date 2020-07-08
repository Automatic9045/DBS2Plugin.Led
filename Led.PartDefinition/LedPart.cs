using System;
using System.Collections.Generic;
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

    public interface ISetConverter
    {
        void Set(List<LedPart> parts, string arguments);
    }

    public class LedPart
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        public IEnumerable<string> SearchNames { get; internal set; }
        public List<LedPart> LedParts { get; internal set; }

        public int X { get; internal set; }
        public int Y { get; internal set; }

        public List<LedPartBasedBytesInfo> BasedBytes { get; internal set; }

        public bool IsDisplayed { get => DisplayingYIndex != -1; }
        public int DisplayingImage { get; set; } = 0;
        public int DisplayingXIndex { get; set; } = 0;
        public int DisplayingYIndex { get; set; } = -1;

        public List<LedPartDisplayPhase> DisplayPhases { get; set; }

        internal Func<string, List<LedPart>, int> VisibilityConverterFunc { get; set; }
        public int Visibility
        {
            get => (VisibilityConverterFunc ?? ((a, b) => 3)).Invoke(SystemName, LedParts);
            internal set => VisibilityConverterFunc = (a, b) => value;
        }

        public int DrawWidth { get; internal set; }
        public int DrawHeight { get; internal set; }

        private System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();
        private int StopWatchSurplus = 0;
        public int StopwatchElapsedMilliseconds { get { return (int)Stopwatch.ElapsedMilliseconds + StopWatchSurplus; } }
        internal int StopwatchCount
        {
            get
            {
                int displaySettingsTotalMilliseconds = 0;
                foreach (LedPartDisplayPhase displayPhase in DisplayPhases)
                {
                    displaySettingsTotalMilliseconds += displayPhase.Span;
                }

                long now = Stopwatch.ElapsedMilliseconds + StopWatchSurplus;
                while (now >= displaySettingsTotalMilliseconds)
                {
                    StopWatchSurplus = (int)(now - displaySettingsTotalMilliseconds);
                    Stopwatch.Restart();
                    now = StopWatchSurplus;
                }

                int ms = 0;
                int i;
                for (i = 0; ms < now; i++)
                {
                    ms += DisplayPhases[i].Span;
                }
                if (i == 0) i = 1;
                return i - 1;
            }
        }

        internal LedPart(int surplus)
        {
            StopWatchSurplus = surplus;
            Stopwatch.Start();
        }
    }

    public class LedPartBasedBytesInfo
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        public IEnumerable<string> SearchNames { get; internal set; }
        public byte[] Bytes { get; internal set; }
        public List<LedPartDefinitionNamesInfo> DefinitionNames { get; internal set; }
        public int Height { get; internal set; }
        public int Width { get; internal set; }
    }

    public class LedPartDefinitionNamesInfo
    {
        public string Name { get; internal set; }
        public string SystemName { get; internal set; }
        internal IEnumerable<string> SearchNames { get; set; }
    }

    public struct LedPartDisplayPhase
    {
        public int Span { get; set; } // [ms]
        public int VisibleIndex { get; set; } // Visibilityでの設定の何番が表示されるか。ビットの論理和
        public int FaceIndex { get; set; } // DisplayingXIndex
    }
}
