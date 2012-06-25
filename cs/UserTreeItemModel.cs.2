using System;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;

namespace UserTreeLib
{
    public class UserTreeItemModel : NotifyPropertyChangedBase
    {
        public const int IndentUnit = 25;

        public int ID { get; set; }
        public UserTreeItemModel Parent { get; set; }
        public List<UserTreeItemModel> Children { get; set; }

        #region Expand

        public static bool IsSilent = false;
        public static Action<UserTreeItemModel> ExpandChecked = null;

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    OnPropertyChanged("IsExpanded");

                    if (!IsSilent)
                    {
                        Debug.Assert(ExpandChecked != null, "ExpandChecked != null");
                        ExpandChecked(this);
                    }
                }
            }
        }

        #endregion

        #region Check

        public static bool IsGroupCheckable = false;
        public static Action<UserTreeItemModel> GroupChecked = null;
        public static Action<UserTreeItemModel> UserChecked = null;

        public bool IsGroupEnable
        {
            get
            {
                if (Level == 0)
                    return false; // Rootをチェックさせない
                else
                    return UserTreeItemModel.IsGroupCheckable;
            }
        }

        private bool? _IsGroupChecked = false;
        public bool? IsGroupChecked
        {
            get { return _IsGroupChecked; }
            set
            {
                if (_IsGroupChecked != value)
                {
                    _IsGroupChecked = value;
                    OnPropertyChanged("IsGroupChecked");

                    if (!IsSilent)
                    {
                        Debug.Assert(GroupChecked != null, "GroupChecked != null");
                        Debug.Assert(IsGroup == true, "IsGroup == true");
                        GroupChecked(this);
                    }
                }
            }
        }

        private bool? _IsUserChecked = false;
        public bool? IsUserChecked
        {
            get { return _IsUserChecked; }
            set
            {
                if (_IsUserChecked != value)
                {
                    _IsUserChecked = value;
                    OnPropertyChanged("IsUserChecked");

                    if (!IsSilent)
                    {
                        Debug.Assert(UserChecked != null, "UserChecked != null");
                        Debug.Assert(IsGroup == false, "IsGroup == false");
                        UserChecked(this);
                    }
                }
            }
        }

        #endregion

        #region Indent

        public int Level { get; set; }
        public int Indent { get { return Level * IndentUnit; } }

        #endregion

        #region UI Visibility

        public bool IsGroup { get; set; }
        public Visibility GroupVisb
        {
            get
            {
                if (IsGroup)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility UserVisb
        {
            get
            {
                if (!IsGroup)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        #endregion

        #region Code/Name

        public string Code { get; set; }
        public string Name { get; set; }

        public string UserDisplayName
        {
            get
            {
                if (!IsGroup && !string.IsNullOrEmpty(Name))
                    return string.Format(StrRes.UserDisplayName, Code, Name);
                else
                    return string.Format(StrRes.UnknownUserDisplayName, Code);
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0}:{1}({2})", IsGroup ? "Group" : "User", Code, ID);
        }
    }
}
