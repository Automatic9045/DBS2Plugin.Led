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
        private partial class ShortcutSetLoader
        {
            private Dictionary<string, ILedShortcutSetConverter> setConverters = new Dictionary<string, ILedShortcutSetConverter>();

            internal LedShortcutSetConverter LoadSetConverter(XElement setConverterElement)
            {
                string setConverterSourceName = (string)setConverterElement.Attribute("Source");
                if (setConverterSourceName == null)
                {
                    throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\" で、ファイルパスが指定されていない SetConverter があります。");
                    return null;
                }

                string setConverterSourcePath = Path.Combine(Path.GetDirectoryName(shortcutSourcePath), setConverterSourceName);
                ILedShortcutSetConverter setConverter = null;
                if (!setConverters.Any((x) => x.Key == setConverterSourcePath))
                {
                    if (!File.Exists(setConverterSourcePath))
                    {
                        throwControlErrorAction.Invoke("LED ショートカットファイル \"" + shortcutSourcePath + "\"で指定されている SetConverter \"" + setConverterSourcePath + "\" が見つかりませんでした。");
                        return null;
                    }
                    Assembly assembly = compileAssemblyFunc.Invoke(setConverterSourcePath, new string[] { tempDllPath });
                    if (assembly == null)
                    {
                        throwControlErrorAction.Invoke("SetConverter \"" + setConverterSourcePath + "\" のコンパイルに失敗しました。");
                        return null;
                    }
                    string iName = typeof(ILedShortcutSetConverter).FullName;
                    Type[] asmTypes = assembly.GetExportedTypes();
                    foreach (Type asmType in asmTypes)
                    {
                        if (asmType.IsClass && !asmType.IsAbstract && !(asmType.GetInterface(iName) is null) && (asmType.Namespace ?? "").Split('.')[0] == "DbsData")
                        {
                            setConverter = (ILedShortcutSetConverter)Activator.CreateInstance(asmType);
                            setConverters.Add(setConverterSourcePath, setConverter);
                            break;
                        }
                    }
                    if (setConverter == null)
                    {
                        throwControlErrorAction.Invoke("LedShortcutSetConverter \"" + setConverterSourcePath + "\" で、 ILedShortcutSetConverter を実装し DbsData.* 名前空間に存在する有効なクラスが見つかりませんでした。");
                        return null;
                    }
                }
                else
                {
                    setConverter = setConverters[setConverterSourcePath];
                }

                return new LedShortcutSetConverter() { SetConverter = setConverter.Set };
            }
        }
    }
}
