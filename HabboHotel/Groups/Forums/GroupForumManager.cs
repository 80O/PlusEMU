using Dapper;
using Plus.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumManager : IGroupForumManager
    {
        private readonly IDatabase _database;
        readonly List<GroupForum> Forums;

        public GroupForumManager(IDatabase database)
        {
            Forums = new List<GroupForum>();
            _database = database;
        }

        public GroupForum? GetForum(int GroupId) => TryGetForum(GroupId, out GroupForum f) ? f : null;

        public async Task<GroupForum> CreateGroupForum(Group Gp)
        {
            if (TryGetForum(Gp.Id, out GroupForum GF))
                return GF;

            using (var connection = _database.Connection())
            {
                await connection.ExecuteAsync("REPLACE INTO group_forums_settings (group_id) VALUES (@gp)", new { gp = Gp.Id });
                await connection.ExecuteAsync("UPDATE groups SET forum_enabled = '1' WHERE id = @id", new { id = Gp.Id });
            }

            GF = new GroupForum(Gp, _database);
            Gp.HasForum = true;
            Forums.Add(GF);
            return GF;
        }

        public bool TryGetForum(int Id, out GroupForum Forum)
        {
            if ((Forum = Forums.FirstOrDefault(c => c.Id == Id)) == null)
            {
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Id, out Group group))
                    return false;

                if (!group.HasForum)
                    return false;

                Forum = new GroupForum(group, _database);
                Forums.Add(Forum);
                return true;
            }
            return true;
        }

        public List<GroupForum?> GetForumsByUserId(int Userid) => PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Userid).Where(c => TryGetForum(c.Id, out GroupForum forum)).Select(c => GetForum(c.Id)).ToList();

        public async Task RemoveGroup(Group Group)
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("DELETE FROM `group_forums_settings` WHERE `group_id` = @group_id", new { group_id = Group.Id });
            await connection.ExecuteAsync("DELETE post FROM group_forums_thread_posts post INNER JOIN group_forums_threads threads ON threads.forum_id = @group_id WHERE threads.id = post.thread_id", new { group_id = Group.Id });
            await connection.ExecuteAsync("DELETE v FROM group_forums_thread_views v INNER JOIN group_forums_threads threads ON threads.forum_id = @group_id WHERE v.thread_id = threads.id", new { group_id = Group.Id });
            await connection.ExecuteAsync("DELETE FROM group_forums_threads WHERE forum_id = @group_id", new { group_id = Group.Id });
        }

        public int GetUnreadThreadForumsByUserId(int Id) => GetForumsByUserId(Id).Where(c => c.UnreadMessages(Id) > 0).Count();
    }
}
