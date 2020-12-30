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
    public class StopSetter : BindableBase
    {
        public StopSetter(Action<string> addCommandAction)
        {
            Stop = new DelegateCommand(() => addCommandAction.Invoke("{stp:}"));
        }

        public DelegateCommand Stop { get; }
    }
}
