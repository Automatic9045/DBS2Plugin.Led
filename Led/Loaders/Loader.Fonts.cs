using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

using static DbsPlugin.Standard.Led.Led;
using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    internal partial class Loader
    {
        private LedFontFactory fontFactory;

        internal List<LedFont> LoadFonts()
        {
            List<LedFont> fonts = new List<LedFont>();

            XElement fontDefinitionsElement = Element.Element("FontDefinitions");
            if (fontDefinitionsElement is null)
            {
                throwControlErrorAction.Invoke("LED レイアウトファイル \"" + layoutXmlPath + "\" にフォント定義がありません。");
                return new List<LedFont>();
            }

            IEnumerable<XElement> fontDefinitionElements = fontDefinitionsElement.Elements("FontDefinition");
            foreach (XElement fontDefinitionElement in fontDefinitionElements)
            {
                string fontSourceName = (string)fontDefinitionElement.Attribute("Source");
                if (fontSourceName is null)
                {
                    throwControlErrorAction.Invoke("LED レイアウトファイル \"" + layoutXmlPath + "\" で、ファイルパスが指定されていないフォント定義があります。");
                    continue;
                }

                string fontSourcePath = Path.Combine(Path.GetDirectoryName(layoutXmlPath), fontSourceName);
                if (!File.Exists(fontSourcePath))
                {
                    throwControlErrorAction.Invoke("LED レイアウトファイル \"" + layoutXmlPath + "\" で指定されているフォントファイル \"" + fontSourcePath + "\" が見つかりませんでした。");
                    continue;
                }

                XElement fontsElement = XDocument.Load(fontSourcePath).Element("LedFonts");
                if (fontsElement is null)
                {
                    throwControlErrorAction.Invoke("\"" + fontSourcePath + "\" はフォントファイルではありません。");
                    continue;
                }
                IEnumerable<XElement> fontElements = fontsElement.Elements("LedFont");
                foreach (XElement fontElement in fontElements)
                {
                    string name = (string)fontElement.Attribute("Name");
                    if (name is null)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で、名前が指定されていないフォントがあります。");
                    }

                    string systemName = (string)fontElement.Attribute("System");
                    if (systemName is null || systemName == "") systemName = name;
                    if (fonts.Any(f => f.SystemName == systemName))
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォントのシステム名 \"" + systemName + "\" は重複しています。");
                    }

                        string searchNamesElement = (string)fontElement.Attribute("Search");
                    if (searchNamesElement is null || searchNamesElement == "") searchNamesElement = name;
                    IEnumerable<string> searchNames = searchNamesElement.Split(';').Select(n => ToSearchString(n));

                    string fontFamily = (string)fontElement.Attribute("FontFamily");
                    if (fontFamily is null)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、フォントファミリーが指定されていません。");
                    }

                    int? fontSize = (int?)fontElement.Attribute("FontSize");
                    if (fontSize is null)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、フォントサイズが指定されていません。");
                    }
                    else if (fontSize == 0)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、フォントサイズに 0 が指定されています。");
                    }
                    else if (1000 < fontSize && ToSearchString((string)fontElement.Attribute("IgnoreFontSizeTooLargeWarning") ?? "") != ToSearchString("true"))
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で指定されているフォントサイズ \"" + fontSize + "\" はバッファオーバーフローの危険があるため無視されました。" +
                            "この警告を無視するには、当該の設定 'FontSize=\"" + fontSize + "\"' の後にこの警告を無視する設定 'IgnoreFontSizeTooLargeWarning=\"True\"' を追加して下さい。");
                    }

                    int? fontWeight = (int?)fontElement.Attribute("FontWeight");
                    if (fontWeight is null)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、フォントウェイトが指定されていません。");
                    }
                    else if (fontWeight < 0 || 1000 < fontWeight)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、無効なフォントウェイト \"" + fontWeight + "\"が指定されています。");
                    }

                    int? antialiasFormat = (int?)fontElement.Attribute("Antialias");
                    if (antialiasFormat is null)
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、アンチエイリアスが指定されていません。");
                    }
                    else if (!Enum.IsDefined(typeof(AntialiasFormat), antialiasFormat))
                    {
                        throwControlErrorAction.Invoke("LED フォントファイル \"" + fontSourcePath + "\" で指定されているフォント \"" + systemName + "\" で、無効なアンチエイリアス \"" + antialiasFormat + "\"が指定されています。");
                    }

                    fonts.Add(fontFactory.Create(name, systemName, searchNames, fontFamily, (int)fontSize, (int)fontWeight, (AntialiasFormat)antialiasFormat));
                }
            }

            return fonts;
        }
    }
}
