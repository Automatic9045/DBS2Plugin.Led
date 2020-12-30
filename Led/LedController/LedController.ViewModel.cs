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

using DbsPlugin.Standard.Led.RichTextEditorViewModels;

namespace DbsPlugin.Standard.Led
{
    public partial class LedControllerViewModel : BindableBase
    {
        private bool _isEnabledReserving = false;
        public bool IsEnabledReserving
        {
            get { return _isEnabledReserving; }
            set { SetProperty(ref _isEnabledReserving, value); }
        }

        private string _shortcut = "";
        public string Shortcut
        {
            get { return _shortcut; }
            set { SetProperty(ref _shortcut, value); }
        }

        private string _part = "";
        public string Part
        {
            get { return _part; }
            set { SetProperty(ref _part, value); }
        }

        private string _group = "";
        public string Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        private string _def = "";
        public string Def
        {
            get { return _def; }
            set { SetProperty(ref _def, value); }
        }

        private int _ShortcutCur = 0;
        public int ShortcutCur
        {
            get { return _ShortcutCur; }
            set { SetProperty(ref _ShortcutCur, value); }
        }

        private int _partCur = -2;
        public int PartCur
        {
            get { return _partCur; }
            set { SetProperty(ref _partCur, value); }
        }

        private int _groupCur = 0;
        public int GroupCur
        {
            get { return _groupCur; }
            set { SetProperty(ref _groupCur, value); }
        }

        private int _defCur = 0;
        public int DefCur
        {
            get { return _defCur; }
            set { SetProperty(ref _defCur, value); }
        }

        private string _shortcutHelp = "";
        public string ShortcutHelp
        {
            get { return _shortcutHelp; }
            set { SetProperty(ref _shortcutHelp, value); }
        }

        private string _partHelp = "";
        public string PartHelp
        {
            get { return _partHelp; }
            set { SetProperty(ref _partHelp, value); }
        }

        private string _groupHelp = "";
        public string GroupHelp
        {
            get { return _groupHelp; }
            set { SetProperty(ref _groupHelp, value); }
        }

        private string _defHelp = "";
        public string DefHelp
        {
            get { return _defHelp; }
            set { SetProperty(ref _defHelp, value); }
        }

        private bool _ShortcutIsEnabled = false;
        public bool ShortcutIsEnabled
        {
            get { return _ShortcutIsEnabled; }
            set { SetProperty(ref _ShortcutIsEnabled, value); }
        }

        private int _PreviewWidth = 0;
        public int PreviewWidth
        {
            get { return _PreviewWidth; }
            set { SetProperty(ref _PreviewWidth, value); }
        }

        private int _PreviewHeight = 0;
        public int PreviewHeight
        {
            get { return _PreviewHeight; }
            set { SetProperty(ref _PreviewHeight, value); }
        }

        private BitmapSource _preview = null;
        public BitmapSource Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        public RichTextEditorViewModel FreeTextEditor { get; private set; }
        public RichTextEditorViewModel FullTextEditor { get; private set; }

        private string _FreeText = "";
        public string FreeText
        {
            get { return _FreeText; }
            set { SetProperty(ref _FreeText, value); }
        }

        private string _FullText = "";
        public string FullText
        {
            get { return _FullText; }
            set { SetProperty(ref _FullText, value); }
        }

        private int _FreeTextSelectionStart = 0;
        public int FreeTextSelectionStart
        {
            get { return _FreeTextSelectionStart; }
            set { SetProperty(ref _FreeTextSelectionStart, value); }
        }

        private int _FullTextSelectionStart = 0;
        public int FullTextSelectionStart
        {
            get { return _FullTextSelectionStart; }
            set { SetProperty(ref _FullTextSelectionStart, value); }
        }

        private int _FreeTextSelectionLength = 0;
        public int FreeTextSelectionLength
        {
            get { return _FreeTextSelectionLength; }
            set { SetProperty(ref _FreeTextSelectionLength, value); }
        }

        private int _FullTextSelectionLength = 0;
        public int FullTextSelectionLength
        {
            get { return _FullTextSelectionLength; }
            set { SetProperty(ref _FullTextSelectionLength, value); }
        }

        private bool _IsNotFreeTextContentLatest = false;
        public bool IsNotFreeTextContentLatest
        {
            get { return _IsNotFreeTextContentLatest; }
            set { SetProperty(ref _IsNotFreeTextContentLatest, value); }
        }

        private bool _IsNotFullTextContentLatest = false;
        public bool IsNotFullTextContentLatest
        {
            get { return _IsNotFullTextContentLatest; }
            set { SetProperty(ref _IsNotFullTextContentLatest, value); }
        }

        private bool _UseFreeText = false;
        public bool UseFreeText
        {
            get { return _UseFreeText; }
            set { SetProperty(ref _UseFreeText, value); }
        }

        private bool _UseFullText = false;
        public bool UseFullText
        {
            get { return _UseFullText; }
            set { SetProperty(ref _UseFullText, value); }
        }

        private bool _FullTextScroll = false;
        public bool FullTextScroll
        {
            get { return _FullTextScroll; }
            set { SetProperty(ref _FullTextScroll, value); }
        }

        private bool _FreeTextIsEnabled = false;
        public bool FreeTextIsEnabled
        {
            get { return _FreeTextIsEnabled; }
            set { SetProperty(ref _FreeTextIsEnabled, value); }
        }

        public DelegateCommand RunFreeText { get; internal set; }
        public DelegateCommand RunFullText { get; internal set; }
        public DelegateCommand FreeTextTextChanged { get; internal set; }
        public DelegateCommand FullTextTextChanged { get; internal set; }
        public DelegateCommand UseFreeTextChanged { get; internal set; }
        public DelegateCommand UseFullTextChanged { get; internal set; }
        public DelegateCommand FullTextScrollChanged { get; internal set; }

        internal List<LedShortcut> Shortcuts { get; set; }
        public List<LedPart> Parts { get; internal set; }
        public List<LedPartBitmap> Groups { get; internal set; }
        public List<LedPartDefinition> Defs { get; internal set; }

        public ObservableCollection<string> ShortcutCollection { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> PartCollection { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> GroupCollection { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DefCollection { get; } = new ObservableCollection<string>();

        public DelegateCommand ShortcutTextChanged { get; internal set; }
        public DelegateCommand PartTextChanged { get; internal set; }
        public DelegateCommand GroupTextChanged { get; internal set; }
        public DelegateCommand DefTextChanged { get; internal set; }

        public DelegateCommand ShortcutSelectionChanged { get; internal set; }
        public DelegateCommand PartSelectionChanged { get; internal set; }
        public DelegateCommand GroupSelectionChanged { get; internal set; }
        public DelegateCommand DefSelectionChanged { get; internal set; }

        public DelegateCommand ShortcutCancel { get; }
        public DelegateCommand PartCancel { get; }
        public DelegateCommand GroupCancel { get; }
        public DelegateCommand DefCancel { get; }

        public DelegateCommand ShortcutDown { get; }
        public DelegateCommand PartDown { get; }
        public DelegateCommand GroupDown { get; }
        public DelegateCommand DefDown { get; }

        public DelegateCommand ShortcutUp { get; }
        public DelegateCommand PartUp { get; }
        public DelegateCommand GroupUp { get; }
        public DelegateCommand DefUp { get; }

        public DelegateCommand RunShortcut { get; internal set; }

        private LedControllerConnector connector;

        internal LedControllerViewModel(LedControllerConnector connector, Action focusToFreeTextTextBoxAction, Action focusToFullTextTextBoxAction)
        {
            this.connector = connector;

            ShortcutCancel = new DelegateCommand(() =>
            {
                Shortcut = "";
            });

            PartCancel = new DelegateCommand(() =>
            {
                Part = "";
            });

            GroupCancel = new DelegateCommand(() =>
            {
                Group = "";
            });

            DefCancel = new DelegateCommand(() =>
            {
                Def = "";
            });

            ShortcutDown = new DelegateCommand(() =>
            {
                if (ShortcutCur - 1 < ShortcutCollection.Count) ShortcutCur += 1;
            });

            PartDown = new DelegateCommand(() =>
            {
                if (PartCur - 1 < PartCollection.Count) PartCur += 1;
            });

            GroupDown = new DelegateCommand(() =>
            {
                if (GroupCur - 1 < GroupCollection.Count) GroupCur += 1;
            });

            DefDown = new DelegateCommand(() =>
            {
                if (DefCur - 1 < DefCollection.Count) DefCur += 1;
            });

            ShortcutUp = new DelegateCommand(() =>
            {
                if (ShortcutCur != 0) ShortcutCur -= 1;
            });

            PartUp = new DelegateCommand(() =>
            {
                if (PartCur != 0) PartCur -= 1;
            });

            GroupUp = new DelegateCommand(() =>
            {
                if (GroupCur != 0) GroupCur -= 1;
            });

            DefUp = new DelegateCommand(() =>
            {
                if (DefCur != 0) DefCur -= 1;
            });

            Initialize(focusToFreeTextTextBoxAction, focusToFullTextTextBoxAction);
        }
    }
}
