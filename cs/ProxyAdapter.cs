using System;
using System.Diagnostics;
using UserInfoCommonLib;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using WebConsoleCommonLib;

namespace UserTreeLib
{
    public class ProxyAdapter : ProxyAdapterBase
    {
        public UserTreeItemModel Root { get; protected set; }
        public Dictionary<int, UserTreeItemModel> GroupDic { get; protected set; }
        public Dictionary<int, UserTreeItemModel> UserDic { get; protected set; }

        public IUserInfoProxy Proxy { get; protected set; }

        protected override void OnBeginDownload()
        {
            Proxy = IoC.Current.Resolve<IUserInfoProxy>();
            Debug.Assert(Proxy != null);

            Proxy.AdminName = AdminName;
            Proxy.Type = Type;

            Proxy.GetGroups(
                g =>
                {
                    if (HasError(Proxy))
                        return;

                    Proxy.GetUsers(
                        u =>
                        {
                            if (HasError(Proxy))
                                return;

                            GroupDic = new Dictionary<int, UserTreeItemModel>();
                            UserDic = new Dictionary<int, UserTreeItemModel>();

                            UserTreeItemModel.IsSilent = true;
                            Root = CreateHrchModelRoot(g, u);
                            GetUserCountAndRemoveEmptyGroup(Root);
                            UserTreeItemModel.IsSilent = false;

                            Finish();
                        });
                });
        }

        protected int GetUserCountAndRemoveEmptyGroup(UserTreeItemModel model)
        {
            if (model.Children == null || model.Children.Count == 0)
                return 0;

            List<int> childIDToRemove = new List<int>();

            var children = model.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.IsGroup)
                {
                    if (GetUserCountAndRemoveEmptyGroup(child) == 0)
                        childIDToRemove.Add(i);
                }
                else
                {
                    RemoveChildren(children, childIDToRemove);
                    return int.MaxValue; // Groups, Users順で並び替えたはず; 大量ユーザのため、正確にユーザ数をカウントしない。
                }
            }

            RemoveChildren(children, childIDToRemove);
            return children.Count;
        }

        private void RemoveChildren(List<UserTreeItemModel> children, List<int> childIDToRemove)
        {
            for (int i = childIDToRemove.Count - 1; i >= 0; i--)
            {
                children.RemoveAt(childIDToRemove[i]);
            }
        }

        protected UserTreeItemModel CreateHrchModelRoot(GroupInfo[] groups, UserInfo[] users)
        {
            UserTreeItemModel root = new UserTreeItemModel
            {
                ID = 0,
                Level = 0,
                IsExpanded = true,
                IsGroup = true,
                Code = StrRes.TreeRoot,
            };

            if (groups != null && groups.Length > 0)
                _dicGroupByParentID = (from c in groups
                                       group c by c.ParentID into g
                                       select g).ToDictionary(c => c.Key);

            if (users != null && users.Length > 0)
                _dicUserByParentID = (from c in users
                                      group c by c.ParentID into g
                                      select g).ToDictionary(c => c.Key);

            AddUnidentifiedUser();
            CreateTreeItemModels(root);
            return root;
        }

        private void AddUnidentifiedUser()
        {
            if (SettingManager.Current.Version != "1.5")
            {
                UserInfo uid = new UserInfo { ID = 0, Code = StrRes.UnidentifiedUserCode, ParentID = 0 };
                var ig = (new UserInfo[] { uid }).GroupBy(c => c.ParentID).Single();

                if (_dicUserByParentID == null)
                    _dicUserByParentID = new Dictionary<int, IGrouping<int, UserInfo>>();
                _dicUserByParentID.Add(0, ig);
            }
        }

        protected void CreateTreeItemModels(UserTreeItemModel parent)
        {
            List<UserTreeItemModel> list = new List<UserTreeItemModel>();

            AddGroup(parent, list);
            AddUser(parent, list);

            parent.Children = list;
            for (int i = 0; i < parent.Children.Count; i++)
            {
                UserTreeItemModel model = parent.Children[i];
                if (model.IsGroup)
                    CreateTreeItemModels(model);
            }
        }

        protected void AddGroup(UserTreeItemModel parent, List<UserTreeItemModel> list)
        {
            Debug.Assert(parent != null);

            if (_dicGroupByParentID == null || _dicGroupByParentID.Count == 0)
                return;

            if (_dicUserByParentID == null || _dicUserByParentID.Count == 0)
                return;

            if (_dicGroupByParentID.ContainsKey(parent.ID))
            {
                int currentLevel = parent.Level + 1;
                var groups = _dicGroupByParentID[parent.ID];

                foreach (GroupInfo group in groups)
                {
                    int groupID = group.ID;

                    if (!GroupDic.ContainsKey(groupID))
                    {
                        UserTreeItemModel model = new UserTreeItemModel
                        {
                            IsGroup = true,
                            Level = currentLevel,
                            Parent = parent,

                            ID = groupID,
                            Code = group.Code,
                        };

                        GroupDic.Add(groupID, model);
                        list.Add(model);
                    }
                }
            }
        }

        protected void AddUser(UserTreeItemModel parent, List<UserTreeItemModel> list)
        {
            Debug.Assert(parent != null);

            if (_dicUserByParentID == null || _dicUserByParentID.Count == 0)
                return;

            if (_dicUserByParentID.ContainsKey(parent.ID))
            {
                int currentLevel = parent.Level + 1;
                var users = _dicUserByParentID[parent.ID];

                foreach (var user in users)
                {
                    int userID = user.ID;

                    if (!UserDic.ContainsKey(userID))
                    {
                        UserTreeItemModel model = new UserTreeItemModel
                        {
                            Level = currentLevel,
                            Parent = parent,

                            ID = user.ID,
                            Code = user.Code,
                            Name = user.Name,
                        };

                        UserDic.Add(userID, model);
                        list.Add(model);
                    }
                }
            }
        }

        protected Dictionary<int, IGrouping<int, GroupInfo>> _dicGroupByParentID = null;
        protected Dictionary<int, IGrouping<int, UserInfo>> _dicUserByParentID = null;
    }
}
