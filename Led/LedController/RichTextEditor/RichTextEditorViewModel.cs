using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

using Prism.Mvvm;
using Prism.Commands;

namespace DbsPlugin.Standard.Led.RichTextEditorViewModels
{
    public class RichTextEditorViewModel : BindableBase
    {
        public RichTextEditorViewModel(Action<string> addCommandAction)
        {
            FontSetter = new FontSetter(addCommandAction);
            FontColorSetter = new FontColorSetter(addCommandAction);
            BackgroundColorSetter = new BackgroundColorSetter(addCommandAction);
            PartSetter = new PartSetter(addCommandAction);
            FlashSetter = new FlashSetter(addCommandAction);
            StopSetter = new StopSetter(addCommandAction);
        }

        public void UpdateFonts(List<LedFont> fonts)
        {
            FontSetter.Fonts = fonts;
        }

        public void UpdatePartBitmaps(List<LedPartBitmapExtension> partBitmaps)
        {
            PartSetter.Bitmaps = partBitmaps;
        }

        public FontSetter FontSetter { get; }
        public FontColorSetter FontColorSetter { get; }
        public BackgroundColorSetter BackgroundColorSetter { get; }
        public PartSetter PartSetter { get; }
        public FlashSetter FlashSetter { get; }
        public StopSetter StopSetter { get; }
    }
}
