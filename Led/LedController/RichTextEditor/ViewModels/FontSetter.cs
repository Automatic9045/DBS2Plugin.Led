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
using System.Windows.Threading;

using Prism.Mvvm;
using Prism.Commands;

namespace DbsPlugin.Standard.Led.RichTextEditorViewModels
{
    public class FontSetter : BindableBase
    {
        public FontSetter(Action<string> addCommandAction)
        {
            TextChanged = new DelegateCommand(() =>
            {
                string searchString = Led.ToSearchString(Text);
                int searchStringLength = searchString.Length;
                string oldFont = "";
                if (Cur >= 0 && Collection.Count != 0) oldFont = Collection[Cur];

                Collection.Clear();
                Collection.AddRange(Fonts.Where(f =>
                {
                    return f.SearchNames.Any(n =>
                    {
                        bool isEmpty = searchString == "";
                        bool isMatchFromStart = false;
                        if (n.Length >= searchStringLength)
                            isMatchFromStart = n.Substring(0, searchStringLength) == searchString;
                        return isEmpty || isMatchFromStart;
                    });
                }).Select(f => f.Name));

                Cur = -1;

                if (Collection.Count == 0)
                {
                    Collection.Add("(該当無し)");
                    Cur = 0;
                    Help = "(候補 0 件 / " + Fonts.Count + " 件)";
                }
                else if (!Collection.Contains(oldFont))
                {
                    Cur = 0;
                    Help = "(候補 " + Collection.Count + " 件 / " + Fonts.Count + " 件)";
                }
                else
                {
                    Cur = Collection.IndexOf(oldFont);
                    Help = "(候補 " + Collection.Count + " 件 / " + Fonts.Count + " 件)";
                }
            });

            Cancel = new DelegateCommand(() => Text = "");

            Down = new DelegateCommand(() =>
            {
                if (Cur - 1 < Collection.Count) Cur += 1;
            });

            Up = new DelegateCommand(() =>
            {
                if (Cur != 0) Cur -= 1;
            });

            Run = new DelegateCommand(() =>
            {
                if (Cur != -1 && Collection[0] != "(該当無し)")
                {
                    LedFont currentFont = Fonts.Find(f => f.Name == Collection[Cur]);
                    addCommandAction.Invoke("{font:" + currentFont.SystemName + "}");
                }
            });

            BindingOperations.EnableCollectionSynchronization(Collection, new object());
        }

        private string _Text = "";
        public string Text
        {
            get { return _Text; }
            set { SetProperty(ref _Text, value); }
        }

        private int _Cur = 0;
        public int Cur
        {
            get { return _Cur; }
            set { SetProperty(ref _Cur, value); }
        }

        private string _Help = "";
        public string Help
        {
            get { return _Help; }
            set { SetProperty(ref _Help, value); }
        }

        private List<LedFont> _Fonts;
        public List<LedFont> Fonts
        {
            get => _Fonts;
            set
            {
                _Fonts = value;
                TextChanged.Execute();
            }
        }
        public ObservableCollection<string> Collection { get; } = new ObservableCollection<string>();

        public DelegateCommand TextChanged { get; }
        public DelegateCommand Cancel { get; }
        public DelegateCommand SelectionChanged { get; }
        public DelegateCommand Down { get; }
        public DelegateCommand Up { get; }
        public DelegateCommand Run { get; }
    }
}
