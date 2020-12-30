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
            private List<LedPart> parts;
            private string shortcutSourcePath;
            private string shortcutName;
            private string tempDllPath;
            private Action<string> throwControlErrorAction;
            private Func<string, IEnumerable<string>, Assembly> compileAssemblyFunc;

            internal ShortcutSetLoader(List<LedPart> parts, string shortcutSourcePath, string shortcutName, string tempDllPath, Action<string> throwControlErrorAction, Func<string, IEnumerable<string>, Assembly> compileAssemblyFunc)
            {
                this.parts = parts;
                this.shortcutSourcePath = shortcutSourcePath;
                this.shortcutName = shortcutName;
                this.tempDllPath = tempDllPath;
                this.throwControlErrorAction = throwControlErrorAction;
                this.compileAssemblyFunc = compileAssemblyFunc;
            }
        }
    }
}
