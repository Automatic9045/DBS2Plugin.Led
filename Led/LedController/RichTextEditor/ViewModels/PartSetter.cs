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
    public class PartSetter : BindableBase
    {
        public PartSetter(Action<string> addCommandAction)
        {
            Search = new DelegateCommand(() =>
            {
                string searchString = Led.ToSearchString(Text);
                int searchStringLength = searchString.Length;
                
                LedPartDefinitionExtension oldDef = null;
                if (Cur >= 0 && CurrentDefs.Count != 0) oldDef = CurrentDefs[Cur];

                CurrentDefs = Bitmaps.Select(b =>
                {
                    return b.Bitmap.Definitions.FindAll(d =>
                    {
                        return d.SearchNames.Any(n =>
                        {
                            bool isEmpty = searchString == "";
                            bool isMatchFromStart = false;
                            if (n.Length >= searchStringLength)
                                isMatchFromStart = n.Substring(0, searchStringLength) == searchString;
                            return isEmpty || isMatchFromStart;
                        });
                    }).ConvertAll(d =>
                    {
                        return new LedPartDefinitionExtension()
                        {
                            Definition = d,
                            ParentBitmap = b,
                            Y = b.Bitmap.Definitions.IndexOf(d),
                        };
                    });
                }).SelectMany(d => d).ToList();
                CanSelect = CurrentDefs.Count < 200;

                Collection.Clear();
                Collection.AddRange(CurrentDefs.ConvertAll(d => $"{d.ParentBitmap.ParentControlName}\\{d.ParentBitmap.ParentPartName}\\{d.ParentBitmap.Bitmap.Name}\\{d.Definition.Name}"));

                Cur = -1;

                if (Collection.Count == 0)
                {
                    Collection.Add("(該当無し)");
                    Cur = 0;
                    Help = "(候補 0 件 / " + TotalCount + " 件)";
                }
                else if (!CurrentDefs.Contains(oldDef))
                {
                    Cur = 0;
                    Help = "(候補 " + Collection.Count + " 件 / " + TotalCount + " 件)";
                }
                else
                {
                    Cur = CurrentDefs.IndexOf(oldDef);
                    Help = "(候補 " + Collection.Count + " 件 / " + TotalCount + " 件)";
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
                    LedPartDefinitionExtension currentDef = CurrentDefs[Cur];
                    addCommandAction.Invoke($"{{prt:{currentDef.ParentBitmap.Bitmap.Path},{currentDef.Y},{XIndex}}}");
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

        private int _XIndex = 0;
        public int XIndex
        {
            get { return _XIndex; }
            set { SetProperty(ref _XIndex, value); }
        }

        private string _Help = "";
        public string Help
        {
            get { return _Help; }
            set { SetProperty(ref _Help, value); }
        }

        private bool _CanSelect = false;
        public bool CanSelect
        {
            get { return _CanSelect; }
            set { SetProperty(ref _CanSelect, value); }
        }

        private List<LedPartBitmapExtension> _Bitmaps;
        public List<LedPartBitmapExtension> Bitmaps
        {
            get => _Bitmaps;
            set
            {
                _Bitmaps = value;
                TotalCount = Bitmaps.Sum(b => b.Bitmap.Definitions.Count);
                Search.Execute();
            }
        }

        public int TotalCount { get; private set; }

        private List<LedPartDefinitionExtension> CurrentDefs { get; set; } = new List<LedPartDefinitionExtension>();
        public ObservableCollection<string> Collection { get; } = new ObservableCollection<string>();

        public DelegateCommand Search { get; }
        public DelegateCommand Cancel { get; }
        public DelegateCommand Down { get; }
        public DelegateCommand Up { get; }
        public DelegateCommand Run { get; }
    }
}
