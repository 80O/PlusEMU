using Dapper;
using Plus.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForum
    {
        private readonly IDatabase _database;
        public int GroupId;
        public Group Group;
        public GroupForumSettings Settings;
        public List<GroupForumThread> Threads;

        public int Id => GroupId;

        public string Name => Group.Name;

        public string Description => Group.Description;


        public GroupForum(Group group, IDatabase database)
        {
            _database = database;
            GroupId = group.Id;
            Group = group;
            Settings = new GroupForumSettings(database, this);
            Threads = new List<GroupForumThread>();

            LoadThreads();
        }

        private async Task LoadThreads()
        {
            using var connection = _database.Connection();
            var table = await connection.QueryAsync("SELECT * FROM group_forums_threads WHERE forum_id = @id ORDER BY id DESC", new { id = Id });
            foreach (var row in table)
            {
                Threads.Add(new GroupForumThread(this, _database, Convert.ToInt32(row.id), Convert.ToInt32(row.user_id), Convert.ToInt32(row.timestamp), row.caption.ToString(), Convert.ToInt32(row.pinned) == 1, Convert.ToInt32(row.locked) == 1, Convert.ToInt32(row.deleted_level), Convert.ToInt32(row.deleter_user_id)));
            }
        }

        public int MessagesCount => Threads.SelectMany(c => c.Posts).Count();

        public int UnreadMessages(int userid)
        {
            int i = 0;
            Threads.ForEach(c => i += c.GetUnreadMessages(userid));
            return i;
        }

        public GroupForumThreadPost GetLastPost() => Threads.SelectMany(c => c.Posts).OrderByDescending(c => c.Timestamp).FirstOrDefault();

        public GroupForumThread GetThread(int ThreadId) => Threads.FirstOrDefault(c => c.Id == ThreadId);

        public async Task <GroupForumThread?> CreateThread(int Creator, string Caption)
        {
            var timestamp = (int)PlusEnvironment.GetUnixTimestamp();
            var Thread = new GroupForumThread(this, _database, 0, Creator, (int)timestamp, Caption, false, false, 0, 0);

            using var connection = _database.Connection();

            var sqlStatement = @"INSERT INTO group_forums_threads (forum_id, user_id, caption, timestamp) VALUES (@a, @b, @c, @d); SELECT LAST_INSERT_ID()";
            
            Thread.Id = await connection.ExecuteScalarAsync<int>(sqlStatement, new {
                a = Id, b = Creator, c = Caption, d = timestamp
            });

            this.Threads.Add(Thread);
            return Thread;
        }

        public GroupForumThreadPost GetPost(int postid) => Threads.SelectMany(c => c.Posts).FirstOrDefault(c => c.Id == postid);
    }
}
