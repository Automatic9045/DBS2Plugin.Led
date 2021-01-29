using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;

using DepartureBoardSimulator;
using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    internal partial class Loader
    {
        internal XElement Element { get; }

        private string layoutXmlPath;
        private string tempDllPath;
        private CalculatorGateway calculatorGateway;
        private Action<string> throwControlErrorAction;
        private Func<string, IEnumerable<string>, Assembly> compileAssemblyFunc;

        internal Loader(string layoutXmlPath, string tempDllPath, CalculatorGateway calculatorGateway, Action<string> throwControlErrorAction, Func<string, IEnumerable<string>, Assembly> compileAssemblyFunc)
        {
            this.layoutXmlPath = layoutXmlPath;
            this.tempDllPath = tempDllPath;
            this.calculatorGateway = calculatorGateway;
            this.throwControlErrorAction = throwControlErrorAction;
            this.compileAssemblyFunc = compileAssemblyFunc;

            fontFactory = new LedFontFactory(this.calculatorGateway);

            if (File.GetAttributes(this.layoutXmlPath).HasFlag(FileAttributes.Directory))
            {
                this.throwControlErrorAction.Invoke("LED レイアウトファイルのパス \"" + this.layoutXmlPath + "\" は無効です。");
            }
            else if (!File.Exists(layoutXmlPath))
            {
                this.throwControlErrorAction.Invoke("LED レイアウトファイル \"" + this.layoutXmlPath + "\" が見つかりませんでした。");
            }
            else
            {
                XDocument layoutXml = XDocument.Load(this.layoutXmlPath);
                Element = layoutXml.Element("LedDisplayLayout");
                if (Element is null)
                {
                    this.throwControlErrorAction.Invoke("\"" + this.layoutXmlPath + "\" は LED レイアウトファイルではありません。");
                }
            }
        }
    }
}
