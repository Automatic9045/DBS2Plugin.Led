using System;
using System.Collections.Generic;

using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    internal class LedFont
    {
        internal string Name { get; }
        internal string SystemName { get; }
        internal IEnumerable<string> SearchNames { get; }
        internal int FontIndex { get; }
        internal int Ascent { get; }
        internal int Height { get; }

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

    internal class LedFontFactory
    {
        private CalculatorGateway calculatorGateway;

        internal LedFontFactory(CalculatorGateway calculatorGateway)
        {
            this.calculatorGateway = calculatorGateway;
        }

        internal LedFont Create(string name, string systemName, IEnumerable<string> searchNames, string fontFamily, int fontSize, int fontWeight, AntialiasFormat antialiasFormat)
        {
            int fontIndex = calculatorGateway.RegisterFont(fontFamily, fontSize, fontWeight, antialiasFormat);

            int ascent = calculatorGateway.GetStringAscent(fontIndex);
            int height = calculatorGateway.GetStringHeight(fontIndex);

            return new LedFont(name, systemName, searchNames, fontIndex, ascent, height);
        }
    }
}
