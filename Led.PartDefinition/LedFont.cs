using System;
using System.Collections.Generic;

namespace DbsPlugin.Standard.Led
{
    public class LedFont
    {
        public string Name { get; }
        public string SystemName { get; }
        public IEnumerable<string> SearchNames { get; }
        public int FontIndex { get; }
        public int Ascent { get; }
        public int Height { get; }

        internal LedFont(string name, string systemName, IEnumerable<string> searchNames, int fontIndex, int ascent, int height)
        {
            Name = name;
            SystemName = systemName;
            SearchNames = searchNames;
            FontIndex = fontIndex;
            Ascent = ascent;
            Height = height;
        }
    }
}
