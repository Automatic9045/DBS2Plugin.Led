using System;
using System.Collections.Generic;

namespace DbsPlugin.Standard.Led
{
    internal class LedShortcut
    {
        internal string Name { get; set; }
        internal IEnumerable<string> SearchNames { get; set; }
        internal List<ILedShortcutSet> Sets { get; set; }
    }

    internal interface ILedShortcutSet
    {

    }

    internal class LedShortcutSetConverter : ILedShortcutSet
    {
        internal Action<List<LedPart>, string> SetConverter { get; set; }
        internal string SetConverterArgument { get; set; }
    }

    internal class LedShortcutSet : ILedShortcutSet
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
                return parts[TargetIndex].Bitmaps[ImageIndex].SystemName;
            }
            set
            {
                ImageIndex = parts[TargetIndex].Bitmaps.FindIndex(p => p.SystemName == value);
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
                    return parts[TargetIndex].Bitmaps[ImageIndex].Definitions[FrameIndex].SystemName;
            }
            set
            {
                if (value == null)
                    FrameIndex = -1;
                else
                    FrameIndex = parts[TargetIndex].Bitmaps[ImageIndex].Definitions.FindIndex(p => p.SystemName == value);
            }
        }

        private List<LedPart> parts;
        internal LedShortcutSet(List<LedPart> parts)
        {
            this.parts = parts;
        }
    }
}
