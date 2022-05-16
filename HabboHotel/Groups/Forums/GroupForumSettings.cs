using Dapper;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumSettings
    {
        public GroupForum ParentForum { get; private set; }

        public int WhoCanRead;
        public int WhoCanPost;
        public int WhoCanInitDiscussions;
        public int WhoCanModerate;
        private readonly IDatabase _database;

        public GroupForumSettings(IDatabase database, GroupForum Forum)
        {
            _database = database;
            ParentForum = Forum;

            using var connection = _database.Connection();

            var getForumSettingsSQL = "SELECT * FROM group_forums_settings WHERE group_id = @id";
            var forumSettings = connection.QueryFirstOrDefault(getForumSettingsSQL, new { id = Forum.Id });

            if (forumSettings is null)
            {
                connection.Execute("REPLACE INTO group_forums_settings (group_id) VALUES (@id);SELECT * FROM group_forums_settings WHERE group_id = @id", new { id = Forum.Id });
            }

            WhoCanRead = forumSettings?.who_can_read;
            WhoCanPost = forumSettings?.who_can_post;
            WhoCanInitDiscussions = forumSettings?.who_can_init_discussions;
            WhoCanModerate = forumSettings?.who_can_mod;
        }

        public async Task Save()
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("UPDATE group_forums_settings SET who_can_read = @a, who_can_post = @b, who_can_init_discussions = @c, who_can_mod = @d WHERE group_id = @id",
                new { id = ParentForum.Id, a = WhoCanRead, b = WhoCanPost, c = WhoCanInitDiscussions, d = WhoCanModerate });
        }

        public GroupForumPermissionLevel GetLevel(int n)
        {
            switch (n)
            {
                case 0:
                default:
                    return GroupForumPermissionLevel.ANYONE;

                case 1:
                    return GroupForumPermissionLevel.JUST_MEMBERS;

                case 2:
                    return GroupForumPermissionLevel.JUST_ADMIN;

                case 3:
                    return GroupForumPermissionLevel.JUST_OWNER;
            }
        }

        public string GetReasonForNot(GameClient Session, int PermissionType)
        {
            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return "";

            switch (GetLevel(PermissionType))
            {
                default:
                case GroupForumPermissionLevel.ANYONE:
                    return "";

                case GroupForumPermissionLevel.JUST_ADMIN:
                    return ParentForum.Group.IsAdmin(Session.GetHabbo().Id) ? "" : "not_admin";

                case GroupForumPermissionLevel.JUST_MEMBERS:
                    return ParentForum.Group.IsMember(Session.GetHabbo().Id) ? "" : "not_member";

                case GroupForumPermissionLevel.JUST_OWNER:
                    return ParentForum.Group.CreatorId == Session.GetHabbo().Id ? "" : "not_owner";
            }
        }
    }

    public enum GroupForumPermissionLevel
    {
        ANYONE,
        JUST_MEMBERS,
        JUST_ADMIN,
        JUST_OWNER
    }
}
