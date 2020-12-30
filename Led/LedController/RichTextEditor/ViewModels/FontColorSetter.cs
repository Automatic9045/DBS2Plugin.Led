using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Prism.Mvvm;
using Prism.Commands;

namespace DbsPlugin.Standard.Led.RichTextEditorViewModels
{
    public class FontColorSetter : BindableBase
    {
        private Action<string> addCommandAction;

        public FontColorSetter(Action<string> addCommandAction)
        {
            this.addCommandAction = addCommandAction;

            Run = CreateCommand(Text);
            
            RunOrange = CreateCommand("#ff8000");
            RunGreen = CreateCommand("#00ff00");
            RunRed = CreateCommand("#ff0000");
            RunWhite = CreateCommand("#ffffff");
            RunBlack = CreateCommand("#333333");
        }

        private DelegateCommand CreateCommand(string fontColor) => new DelegateCommand(() => addCommandAction.Invoke($"{{clr:{fontColor}}}"));

        private string _Text = "";
        public string Text
        {
            get { return _Text; }
            set { SetProperty(ref _Text, value); }
        }

        public DelegateCommand Run { get; }
        public DelegateCommand RunOrange { get; }
        public DelegateCommand RunGreen { get; }
        public DelegateCommand RunRed { get; }
        public DelegateCommand RunWhite { get; }
        public DelegateCommand RunBlack { get; }
    }
}
