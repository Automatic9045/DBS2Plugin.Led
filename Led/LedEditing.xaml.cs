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

namespace DbsPlugin.Standard.Led
{
    public class LedEditingUserControlViewModel : BindableBase
    {
        private bool _isEnabledReserving = false;
        public bool IsEnabledReserving
        {
            get { return _isEnabledReserving; }
            set { SetProperty(ref _isEnabledReserving, value); }
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

        public List<LedPart> Parts { get; internal set; }
        public List<LedPartBasedBytesInfo> Groups { get; internal set; }
        public List<LedPartDefinitionNamesInfo> Defs { get; internal set; }

        public ObservableCollection<string> PartCollection { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> GroupCollection { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DefCollection { get; } = new ObservableCollection<string>();

        public DelegateCommand PartTextChanged { get; internal set; }
        public DelegateCommand GroupTextChanged { get; internal set; }
        public DelegateCommand DefTextChanged { get; internal set; }

        public DelegateCommand PartSelectionChanged { get; internal set; }
        public DelegateCommand GroupSelectionChanged { get; internal set; }
        public DelegateCommand DefSelectionChanged { get; internal set; }

        public DelegateCommand PartCancel { get; }
        public DelegateCommand GroupCancel { get; }
        public DelegateCommand DefCancel { get; }

        public DelegateCommand PartDown { get; }
        public DelegateCommand GroupDown { get; }
        public DelegateCommand DefDown { get; }

        public DelegateCommand PartUp { get; }
        public DelegateCommand GroupUp { get; }
        public DelegateCommand DefUp { get; }

        public LedEditingUserControlViewModel()
        {
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
        }
    }

    /// <summary>
    /// LedSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class LedEditingUserControl : UserControl
    {
        public LedEditingUserControlViewModel VM = new LedEditingUserControlViewModel();

        public LedEditingUserControl()
        {
            InitializeComponent();
            DataContext = VM;
        }
    }
}
