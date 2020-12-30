using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using DepartureBoardSimulator;
using DbsPlugin.Standard.Led.Calculation;

namespace DbsPlugin.Standard.Led
{
    internal class LedControllerConnector
    {
        internal DbsPluginConnector PluginConnector { get; }
        internal List<LedControl> LedControls { get; }
        internal CalculatorGateway CalculatorGateway { get; }

        internal LedControllerConnector(DbsPluginConnector pluginConnector, List<LedControl> ledControls, CalculatorGateway calculatorGateway)
        {
            PluginConnector = pluginConnector;
            LedControls = ledControls;
            CalculatorGateway = calculatorGateway;
        }
    }
}
