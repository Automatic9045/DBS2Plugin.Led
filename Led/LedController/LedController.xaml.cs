using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace DbsPlugin.Standard.Led
{
    /// <summary>
    /// LedSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class LedController : UserControl
    {
        public LedControllerViewModel VM { get; }

        internal LedController(LedControllerConnector connector)
        {
            InitializeComponent();

            VM = new LedControllerViewModel(connector, () => FreeTextTextBox.Focus(), () => FullTextTextBox.Focus());
            DataContext = VM;
        }
    }
}
