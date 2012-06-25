using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using UserInfoCommonLib;
using WebConsoleCommonLib;

namespace UserTreeLib
{
    public enum UserSearchModeEnum
    {
        User,
        Group,
    }

    public class UserSearch : Control
    {
        #region ParentID

        public int ParentID
        {
            get { return (int)GetValue(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        public static readonly DependencyProperty ParentIDProperty =
            DependencyProperty.Register("ParentID", typeof(int), typeof(UserSearch),
            new PropertyMetadata(UserTree.INVALID_PARENT_ID));

        #endregion

        #region CurrentParentID

        public int CurrentParentID
        {
            get { return (int)GetValue(CurrentParentIDProperty); }
            set { SetValue(CurrentParentIDProperty, value); }
        }

        public static readonly DependencyProperty CurrentParentIDProperty =
            DependencyProperty.Register("CurrentParentID", typeof(int), typeof(UserSearch),
            new PropertyMetadata(UserTree.INVALID_PARENT_ID, OnCurrentParentIDChanged));

        protected static void OnCurrentParentIDChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((UserSearch)o).OnCurrentParentIDChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected void OnCurrentParentIDChanged(int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                if (ParentID == UserTree.INVALID_PARENT_ID)
                    return;

                if (CurrentParentID == UserTree.INVALID_PARENT_ID)
                    return;

                if (oldValue == ParentID) // 見えなくなるたびにクリアする。
                    Reset();
            }
        }

        #endregion

        #region SelectedID

        public IDPair SelectedID
        {
            get { return (IDPair)GetValue(SelectedIDProperty); }
            set { SetValue(SelectedIDProperty, value); }
        }

        public static readonly DependencyProperty SelectedIDProperty =
            DependencyProperty.Register("SelectedID", typeof(IDPair), typeof(UserSearch),
            new PropertyMetadata(null));

        #endregion

        #region UserType

        public UserTypeEnum UserInfoType
        {
            get { return (UserTypeEnum)GetValue(UserInfoTypeProperty); }
            set { SetValue(UserInfoTypeProperty, value); }
        }

        public static readonly DependencyProperty UserInfoTypeProperty =
            DependencyProperty.Register("UserInfoType", typeof(UserTypeEnum), typeof(UserSearch),
            new PropertyMetadata(UserTypeEnum.Security));

        #endregion

        #region AdminName

        public string AdminName
        {
            get { return (string)GetValue(AdminNameProperty); }
            set { SetValue(AdminNameProperty, value); }
        }

        public static readonly DependencyProperty AdminNameProperty =
            DependencyProperty.Register("AdminName", typeof(string), typeof(UserSearch),
            new PropertyMetadata(null, OnAdminNameChanged));

        protected static void OnAdminNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((UserSearch)o).OnAdminNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected void OnAdminNameChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
                Reset();
        }

        #endregion

        #region Mode

        protected enum Mode
        {
            LogUser,
            LogGroup,
            SecUser,
        }

        protected Mode _currentMode = Mode.LogUser;

        protected void UpdateMode()
        {
            switch (UserInfoType)
            {
                case UserTypeEnum.Log:
                    if (_rbGroup.IsChecked == true)
                        _currentMode = Mode.LogGroup;
                    else
                        _currentMode = Mode.LogUser;

                    break;
                case UserTypeEnum.Security:
                    _currentMode = Mode.SecUser;

                    break;
            }
        }

        #endregion

        #region bSearch

        protected Button _bSearch;
        public Button bSearch
        {
            get { return _bSearch; }
            set
            {
                if (_bSearch != null)
                {
                    _bSearch.Click -= _bSearch_Click;
                }

                _bSearch = value;

                if (_bSearch != null)
                {
                    _bSearch.Click += new RoutedEventHandler(_bSearch_Click);
                }
            }
        }

        protected void _bSearch_Click(object sender, RoutedEventArgs e)
        {
            BeginDownload();
        }

        #endregion

        #region Download

        protected void BeginDownload()
        {
            Mode prevMode = _currentMode;
            UpdateMode();

            switch (_currentMode)
            {
                case Mode.LogUser:
                    if (_logUsers == null)
                        DoBeginDownload();
                    else if (prevMode != Mode.LogUser)
                        UpdateView(_logUsers);
                    else
                        UpdateLogUserFilter();

                    break;
                case Mode.LogGroup:
                    if (_logUsers == null)
                        DoBeginDownload();
                    else if (prevMode != Mode.LogGroup)
                        UpdateView(_logUsers);
                    else
                        UpdateLogGroupFilter();

                    break;
                case Mode.SecUser:
                    if (_secUsers == null)
                        DoBeginDownload();
                    else
                    {
                        UpdateSecUserFilter();
                        HideLogGroupColumn();
                    }

                    break;
            }
        }

        private void DoBeginDownload()
        {
            _waiting.Message = StrRes.MsgDownload;
            _waiting.IsBusy = true;

            IUserSearchProxy proxy = IoC.Current.Resolve<IUserSearchProxy>();
            proxy.AdminName = AdminName;
            proxy.Type = UserInfoType;
            proxy.GetUsers(UpdateView);
        }

        protected void UpdateView(UserInfoS[] users)
        {
            switch (_currentMode)
            {
                case Mode.LogUser:
                    _logUsers = users;

                    if (_logUsers == null)
                    {
                        _waiting.Message = StrRes.MsgNoData;
                        _view = new PagedCollectionView(new UserInfoS[0]);
                    }
                    else
                    {
                        _waiting.IsBusy = false;
                        _view = new PagedCollectionView(_logUsers);
                    }

                    UpdateLogUserFilter();

                    break;
                case Mode.LogGroup:
                    _logUsers = users;

                    var groups = from c in _logUsers
                                 group c by c.LogGroupID into g
                                 select new GroupInfoS
                                 {
                                     ID = g.Key,
                                     Code = g.First().LogGroup,
                                     Name = g.First().LogGroupName
                                 };

                    if (_logUsers == null)
                    {
                        _waiting.Message = StrRes.MsgNoData;
                        _view = new PagedCollectionView(new GroupInfoS[0]);
                    }
                    else
                    {
                        _waiting.IsBusy = false;
                        _view = new PagedCollectionView(groups);
                    }

                    UpdateLogGroupFilter();

                    break;
                case Mode.SecUser:
                    _secUsers = users;

                    if (_secUsers == null)
                    {
                        _waiting.Message = StrRes.MsgNoData;
                        _view = new PagedCollectionView(new UserInfoS[0]);
                    }
                    else
                    {
                        _waiting.IsBusy = false;
                        _view = new PagedCollectionView(_secUsers);
                    }

                    UpdateSecUserFilter();

                    _userGrid.LayoutUpdated += new EventHandler(_userGrid_LayoutUpdated);

                    break;
            }

            _pager.Source = _view;
            _userGrid.ItemsSource = _view;
        }

        void _userGrid_LayoutUpdated(object sender, EventArgs e)
        {
            Debug.Assert(_currentMode == Mode.SecUser);
            _userGrid.LayoutUpdated -= _userGrid_LayoutUpdated;

            HideLogGroupColumn();
        }

        private void HideLogGroupColumn()
        {
            if (_userGrid.Columns.Count > 0)
                _userGrid.Columns[_userGrid.Columns.Count - 1].Visibility = Visibility.Collapsed;
        }

        protected void Reset()
        {
            if (_tKeyword == null)
                return;

            Debug.Assert(_tKeyword != null, "_tKeyword != null");
            Debug.Assert(_pager != null, "_pager != null");
            Debug.Assert(userGrid != null, "userGrid != null");
            Debug.Assert(_rbID != null, "_rbID != null");

            if (SelectedID != null)
                SelectedID = null;

            _secUsers = null;
            _logUsers = null;
            _view = null;

            _pager.Source = null;
            userGrid.ItemsSource = null;

            _rbID.IsChecked = true;

            _tKeyword.Text = string.Empty;
        }

        #endregion

        #region Filter

        protected void UpdateSecUserFilter()
        {
            string kwd = _tKeyword.Text;
            if (!string.IsNullOrEmpty(kwd))
                kwd = kwd.ToLower();

            if (!string.IsNullOrEmpty(kwd))
            {
                if (_rbID.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Code.ToLower().Contains(kwd));

                else if (_rbName.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Name.ToLower().Contains(kwd));

                else if (_rbAccount.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Account.ToLower().Contains(kwd));

                else if (_rbPC.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).PC.ToLower().Contains(kwd));

                else if (_rbDomain.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Domain.ToLower().Contains(kwd));

                else if (_rbGroup.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).SecurityGroup.ToLower().Contains(kwd));

                else
                    _view.Filter = null;
            }
            else
                _view.Filter = null;
        }

        protected void UpdateLogGroupFilter()
        {
            string kwd = _tKeyword.Text;
            if (!string.IsNullOrEmpty(kwd))
                kwd = kwd.ToLower();

            if (!string.IsNullOrEmpty(kwd))
            {
                if (_rbGroup.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((GroupInfoS)c).Code.ToLower().Contains(kwd));

                else
                    _view.Filter = null;
            }
            else
                _view.Filter = null;
        }

        protected void UpdateLogUserFilter()
        {
            string kwd = _tKeyword.Text;
            if (!string.IsNullOrEmpty(kwd))
                kwd = kwd.ToLower();

            if (!string.IsNullOrEmpty(kwd))
            {
                if (_rbID.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Code.ToLower().Contains(kwd));

                else if (_rbName.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Name.ToLower().Contains(kwd));

                else if (_rbAccount.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Account.ToLower().Contains(kwd));

                else if (_rbPC.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).PC.ToLower().Contains(kwd));

                else if (_rbDomain.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((UserInfoS)c).Domain.ToLower().Contains(kwd));

                else
                    _view.Filter = null;
            }
            else
                _view.Filter = null;
        }

        #endregion

        #region rbGroup

        protected RadioButton _rbGroup;
        public RadioButton rbGroup
        {
            get { return _rbGroup; }
            set
            {
                _rbGroup = value;

                if (_rbGroup != null)
                {
                    switch (UserInfoType)
                    {
                        case UserTypeEnum.Security:
                            _rbGroup.Content = StrRes.USSecGroup;
                            break;

                        case UserTypeEnum.Log:
                            _rbGroup.Content = StrRes.USLogGroup;
                            break;
                    }
                }
            }
        }

        #endregion

        #region userGrid

        protected DataGrid _userGrid;
        protected DataGrid userGrid
        {
            get { return _userGrid; }
            set
            {
                _userGrid = value;
                if (_userGrid != null)
                    _userGrid.MouseLeftButtonUp += new MouseButtonEventHandler(_userGrid_MouseLeftButtonUp);
            }
        }

        void _userGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UserInfoS ui = null;
            GroupInfoS gi = null;

            switch (_currentMode)
            {
                case Mode.LogUser:
                    ui = _userGrid.SelectedItem as UserInfoS;

                    break;
                case Mode.LogGroup:
                    gi = _userGrid.SelectedItem as GroupInfoS;

                    break;
                case Mode.SecUser:
                    ui = _userGrid.SelectedItem as UserInfoS;

                    break;
            }



            if (gi != null)
            {
                if (SelectedID == null || SelectedID.GroupID != gi.ID)
                {
                    SelectedID = new IDPair { GroupID = gi.ID, UserID = UserTree.INVALID_USER_ID };
                    Debug.WriteLine("{0}, {1}", SelectedID.GroupID, SelectedID.UserID);
                }
            }
            else if (ui != null)
            {
                if (SelectedID == null || SelectedID.UserID != ui.ID)
                {
                    SelectedID = new IDPair { GroupID = UserTree.INVALID_USER_ID, UserID = ui.ID };
                    Debug.WriteLine("{0}, {1}", SelectedID.GroupID, SelectedID.UserID);
                }
            }
            else
            {
                if (SelectedID != null)
                    SelectedID = null;
            }
        }

        #endregion

        public UserSearch()
        {
            this.DefaultStyleKey = typeof(UserSearch);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            bSearch = GetTemplateChild("bSearch") as Button;
            _tKeyword = GetTemplateChild("tKeyword") as TextBox;

            userGrid = GetTemplateChild("userGrid") as DataGrid;
            _pager = GetTemplateChild("pager") as DataPager;

            _rbID = GetTemplateChild("rbID") as RadioButton;
            _rbName = GetTemplateChild("rbName") as RadioButton;
            _rbAccount = GetTemplateChild("rbAccount") as RadioButton;
            _rbPC = GetTemplateChild("rbPC") as RadioButton;
            _rbDomain = GetTemplateChild("rbDomain") as RadioButton;

            rbGroup = GetTemplateChild("rbGroup") as RadioButton;

            _waiting = GetTemplateChild("waiting") as WaitingPanel;
        }

        protected PagedCollectionView _view;
        protected UserInfoS[] _secUsers;
        protected UserInfoS[] _logUsers;

        protected TextBox _tKeyword;
        protected DataPager _pager;

        protected RadioButton _rbID;
        protected RadioButton _rbName;
        protected RadioButton _rbAccount;
        protected RadioButton _rbPC;
        protected RadioButton _rbDomain;

        protected WaitingPanel _waiting;
    }
}
