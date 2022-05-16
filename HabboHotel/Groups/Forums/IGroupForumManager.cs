using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums;

public interface IGroupForumManager
{
    GroupForum GetForum(int GroupId);
    Task<GroupForum> CreateGroupForum(Group Gp);
    bool TryGetForum(int Id, out GroupForum Forum);
    List<GroupForum> GetForumsByUserId(int Userid);
    Task RemoveGroup(Group Group);
    int GetUnreadThreadForumsByUserId(int Id);
}
