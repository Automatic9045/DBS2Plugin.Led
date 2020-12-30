using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;

using static DbsPlugin.Standard.Led.Led;

namespace DbsPlugin.Standard.Led
{
    internal partial class Loader
    {
        internal List<LedShortcut> LoadShortcuts(List<LedPart> parts)
        {
            List<LedShortcut> shortcuts = new List<LedShortcut>();

            XElement shortcutDefinitionsElement = Element.Element("ShortcutDefinitions");
            if (shortcutDefinitionsElement != null)
            {
                IEnumerable<XElement> shortcutDefinitionElements = shortcutDefinitionsElement.Elements("ShortcutDefinition");
                foreach (XElement shortcutDefinitionElement in shortcutDefinitionElements)
                {
                    string shortcutSourceName = (string)shortcutDefinitionElement.Attribute("Source");
                    if (shortcutSourceName == null)
                    {
                        throwControlErrorAction.Invoke("LED レイアウトファイル \"" + layoutXmlPath + "\" で、ファイルパスが指定されていないショートカット定義があります。");
                        continue;
                    }

                    string shortcutSourcePath = Path.Combine(Path.GetDirectoryName(layoutXmlPath), shortcutSourceName);
                    if (!File.Exists(shortcutSourcePath))
                    {
                        throwControlErrorAction.Invoke("LED レイアウトファイル \"" + layoutXmlPath + "\"で指定されている ショートカットファイル \"" + shortcutSourcePath + "\" が見つかりませんでした。");
                        continue;
                    }

                    XElement shortcutsElement = XDocument.Load(shortcutSourcePath).Element("LedShortcuts");
                    if (shortcutsElement == null)
                    {
                        throwControlErrorAction.Invoke("\"" + shortcutSourcePath + "\" はショートカットファイルではありません。");
                        continue;
                    }
                    IEnumerable<XElement> shortcutElements = shortcutsElement.Elements("Shortcut");
                    foreach (XElement shortcutElement in shortcutElements)
                    {
                        string shortcutName = (string)shortcutElement.Attribute("Name");
                        if (shortcutName == null)
                        {
                            throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で、名前が指定されていないショートカットがあります。");
                        }

                        string shortcutSearchNamesElement = (string)shortcutElement.Attribute("Search");
                        if (shortcutSearchNamesElement == null || shortcutSearchNamesElement == "") shortcutSearchNamesElement = shortcutName;
                        IEnumerable<string> shortcutSearchNames = shortcutSearchNamesElement.Split(';').Select(n => ToSearchString(n));

                        List<ILedShortcutSet> shortcutSets = new List<ILedShortcutSet>();
                        ShortcutSetLoader shortcutSetLoader = new ShortcutSetLoader(parts, shortcutSourcePath, shortcutName, tempDllPath, throwControlErrorAction, compileAssemblyFunc);

                        IEnumerable<XElement> setElements = shortcutElement.Elements("Set");
                        foreach (XElement setElement in setElements)
                        {
                            LedShortcutSet set = shortcutSetLoader.LoadSet(setElement);
                            if (set != null) shortcutSets.Add(set);
                        }

                        IEnumerable<XElement> setConverterElements = shortcutElement.Elements("SetConverter");
                        foreach (XElement setConverterElement in setConverterElements)
                        {
                            LedShortcutSetConverter setConverter = shortcutSetLoader.LoadSetConverter(setConverterElement);
                            if (setConverter != null) shortcutSets.Add(setConverter);
                        }

                        shortcuts.Add(new LedShortcut() { Name = shortcutName, SearchNames = shortcutSearchNames, Sets = shortcutSets });
                    }
                }
            }

            return shortcuts;
        }
    }
}
