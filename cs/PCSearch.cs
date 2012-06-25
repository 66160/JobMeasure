using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WebConsoleCommonLib;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using UserInfoCommonLib;
using System.Windows.Data;

namespace UserTreeLib
{
    public class PCSearch : Control
    {
        #region CheckedPC

        public PCInfo CheckedPC
        {
            get { return (PCInfo)GetValue(CheckedPCProperty); }
            set { SetValue(CheckedPCProperty, value); }
        }

        public static readonly DependencyProperty CheckedPCProperty =
            DependencyProperty.Register("CheckedPC", typeof(PCInfo), typeof(PCSearch),
            new PropertyMetadata(null, OnCheckedPCChanged));

        protected static void OnCheckedPCChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PCSearch)o).OnCheckedPCChanged((PCInfo)e.OldValue, (PCInfo)e.NewValue);
        }

        protected void OnCheckedPCChanged(PCInfo oldValue, PCInfo newValue)
        {
            if (newValue == null)
                _pcGrid.SelectedItem = null;
        }

        #endregion

        #region AdminName

        public string AdminName
        {
            get { return (string)GetValue(AdminNameProperty); }
            set { SetValue(AdminNameProperty, value); }
        }

        public static readonly DependencyProperty AdminNameProperty =
            DependencyProperty.Register("AdminName", typeof(string), typeof(PCSearch),
            new PropertyMetadata(null, OnAdminNameChanged));

        protected static void OnAdminNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((PCSearch)o).OnAdminNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected void OnAdminNameChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Reset();
                BeginDownload();
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

        private void BeginDownload()
        {
            if (bSearch == null)
                return;
            if (string.IsNullOrEmpty(this.AdminName))
                return;

            if (_allPCs == null)
                DoBeginDownload();
            else
                UpdatePCFilter();
        }

        private void DoBeginDownload()
        {
            _waiting.Message = StrRes.MsgDownload;
            _waiting.IsBusy = true;

            IUserSearchProxy proxy = IoC.Current.Resolve<IUserSearchProxy>();
            proxy.AdminName = AdminName;
            proxy.GetAllPCs(UpdateView);
        }

        protected void UpdateView(PCInfo[] pcs)
        {
            _allPCs = pcs;

            if (_allPCs == null)
            {
                _waiting.Message = StrRes.MsgNoData;
                _view = new PagedCollectionView(new PCInfo[0]);
            }
            else
            {
                _waiting.IsBusy = false;
                _view = new PagedCollectionView(_allPCs);
            }

            UpdatePCFilter();

            _pager.Source = _view;
            _pcGrid.ItemsSource = _view;
        }

        protected void Reset()
        {
            if (_tKeyword == null)
                return;

            if (CheckedPC != null)
                CheckedPC = null;

            _allPCs = null;
            _view = null;

            _pager.Source = null;
            PCGrid.ItemsSource = null;

            _rbPC.IsChecked = true;

            _tKeyword.Text = string.Empty;
        }

        #endregion

        #region Filter

        protected void UpdatePCFilter()
        {
            string kwd = _tKeyword.Text;
            if (!string.IsNullOrEmpty(kwd))
                kwd = kwd.ToLower();

            if (!string.IsNullOrEmpty(kwd))
            {
                if (_rbPC.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((PCInfo)c).PC.ToLower().Contains(kwd));

                else if (_rbDomain.IsChecked == true)
                    _view.Filter = new Predicate<object>(c => ((PCInfo)c).Domain.ToLower().Contains(kwd));

                else
                    _view.Filter = null;
            }
            else
                _view.Filter = null;
        }

        #endregion

        #region userGrid

        protected DataGrid _pcGrid;
        protected DataGrid PCGrid
        {
            get { return _pcGrid; }
            set
            {
                _pcGrid = value;
                if (_pcGrid != null)
                    _pcGrid.MouseLeftButtonUp += new MouseButtonEventHandler(_pcGrid_MouseLeftButtonUp);
            }
        }

        void _pcGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pc = (PCInfo)_pcGrid.SelectedItem;
            if (pc != null)
            {
                if (CheckedPC == null || CheckedPC.ID != pc.ID)
                {
                    CheckedPC = pc;
                    Debug.WriteLine("{0}, {1}", CheckedPC.PC, CheckedPC.Domain);
                }
            }
        }

        #endregion

        public PCSearch()
        {
            this.DefaultStyleKey = typeof(PCSearch);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            bSearch = GetTemplateChild("bSearch") as Button;
            _tKeyword = GetTemplateChild("tKeyword") as TextBox;

            PCGrid = GetTemplateChild("userGrid") as DataGrid;
            _pager = GetTemplateChild("pager") as DataPager;

            _rbPC = GetTemplateChild("rbPC") as RadioButton;
            _rbDomain = GetTemplateChild("rbDomain") as RadioButton;

            _waiting = GetTemplateChild("waiting") as WaitingPanel;

            BeginDownload();
        }

        protected PagedCollectionView _view;
        protected PCInfo[] _allPCs;

        protected TextBox _tKeyword;
        protected DataPager _pager;

        protected RadioButton _rbPC;
        protected RadioButton _rbDomain;

        protected WaitingPanel _waiting;
    }
}
