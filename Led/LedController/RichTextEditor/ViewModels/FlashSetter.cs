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
    public class FlashSetter : BindableBase
    {
        private Action<string> addCommandAction;

        public FlashSetter(Action<string> addCommandAction)
        {
            this.addCommandAction = addCommandAction;

            FlashStart = CreateCommand(true);
            FlashEnd = CreateCommand(false);
        }

        private DelegateCommand CreateCommand(bool flash) => new DelegateCommand(() => addCommandAction.Invoke($"{{fls:{(flash ? 1 : 0)}}}"));

        public DelegateCommand FlashStart { get; }
        public DelegateCommand FlashEnd { get; }
    }
}
