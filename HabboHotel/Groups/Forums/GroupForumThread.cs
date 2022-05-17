using Dapper;
using Plus.Communication.Packets.Outgoing;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumThread
    {
        public int Id;
        public int UserId;
        public string Caption;
        public int Timestamp;

        //Stats
        public bool Pinned;
        public bool Locked;
        public int DeletedLevel;
        public int DeleterUserId;
        public int DeletedTimestamp;

        private readonly IDatabase _database;

        public GroupForum ParentForum;
        public List<GroupForumThreadPost> Posts;

        public List<GameClient> UsersOnThread;
        public List<GroupForumThreadPostView> Views;


        public GroupForumThread(GroupForum parent, IDatabase database, int id, int userid, int timestamp, string caption, bool pinned, bool locked, int deletedlevel, int deleterid)
        {
            _database = database;
            Views = new List<GroupForumThreadPostView>(); ;
            UsersOnThread = new List<GameClient>();
            ParentForum = parent;

            Id = id;
            UserId = userid;
            Timestamp = timestamp;
            Caption = caption;
            Posts = new List<GroupForumThreadPost>();

            Pinned = pinned;
            Locked = locked;
            DeletedLevel = deletedlevel;
            DeleterUserId = deleterid;
            DeletedTimestamp = (int)PlusEnvironment.GetUnixTimestamp();

            using var connection = _database.Connection();
            var results = connection.Query("SELECT * FROM group_forums_thread_posts WHERE thread_id = @id", new { id = Id });

            foreach (var result in results)
            {
                Posts.Add(new GroupForumThreadPost(_database, this, Convert.ToInt32(result.id), Convert.ToInt32(result.user_id), Convert.ToInt32(result.timestamp), result.message.ToString(), Convert.ToInt32(result.deleted_level), Convert.ToInt32(result.deleter_user_id)));
            }

            var results2 = connection.Query("SELECT * FROM group_forums_thread_views WHERE thread_id = @id", new { id = Id });

            foreach (var result2 in results2)
            {
                Views.Add(new GroupForumThreadPostView(Convert.ToInt32(result2.id), Convert.ToInt32(result2.userid), Convert.ToInt32(result2.count)));
            }
        }

        public async void AddView(int userid, int count = -1)
        {
            GroupForumThreadPostView view;
            var now = (int)PlusEnvironment.GetUnixTimestamp();

            if ((view = GetView(userid)) != null)
            {
                view.Count = count >= 0 ? count : Posts.Count;
                using var connection = _database.Connection();
                await connection.ExecuteAsync("UPDATE group_forums_thread_views SET count = @c, timestamp = @timestamp WHERE thread_id = @p AND user_id = @u", new { c = view.Count, timestamp = now, p = Id, u = userid });
            }
            else
            {
                view = new GroupForumThreadPostView(0, userid, Posts.Count);

                using var connection = _database.Connection();
                var sqlStatement = @"INSERT INTO group_forums_thread_views (thread_id, timestamp, user_id, count) VALUES (@threadId, @timestamp, @userId, @count); SELECT LAST_INSERT_ID()";

                view.Id = await connection.ExecuteScalarAsync<int>(sqlStatement, new
                {
                    threadId = Id,
                    timestamp = now,
                    userId = userid,
                    count = view.Count
                });

                Views.Add(view);
            }
        }

        public GroupForumThreadPostView GetView(int userid) => Views.FirstOrDefault(count => count.UserId == userid);

        public int GetUnreadMessages(int userid)
        {
            GroupForumThreadPostView view;
            foreach (var item in Views)
            {
                item.UserId = userid;
            }
            return (view = GetView(userid)) != null ? Posts.Count - view.Count : Posts.Count;
        }

        public List<GroupForumThreadPost> GetUserPosts(int userid) => Posts.Where(c => c.UserId == userid).ToList();

        public Habbo GetAuthor() => PlusEnvironment.GetHabboById(UserId);

        public Habbo GetDeleter() => PlusEnvironment.GetHabboById(DeleterUserId);

        public async Task<GroupForumThreadPost?> CreatePost(int userid, string message)
        {
            var now = (int)PlusEnvironment.GetUnixTimestamp();
            var post = new GroupForumThreadPost(_database, this, 0, userid, now, message, 0, 0);

            using var connection = _database.Connection();

            var sqlStatement = @"INSERT INTO group_forums_thread_posts (thread_id, user_id, message, timestamp) VALUES (@a, @b, @c, @d); SELECT LAST_INSERT_ID()";

            post.Id = await connection.ExecuteScalarAsync<int>(sqlStatement, new
            {
                a = Id,
                b = userid,
                c = message,
                d = now
            });
           
            Posts.Add(post);
            return post;
        }

        public GroupForumThreadPost GetLastMessage() => Posts.LastOrDefault();

        public void SerializeData(GameClient Session, ServerPacket Packet)
        {
            var lastpost = GetLastMessage();
            var isn = lastpost == null;
            Packet.WriteInteger(Id); //Thread ID
            Packet.WriteInteger(GetAuthor().Id);
            Packet.WriteString(GetAuthor().Username); //Thread Author
            Packet.WriteString(Caption); //Thread Title
            Packet.WriteBoolean(Pinned); //Pinned
            Packet.WriteBoolean(Locked); //Locked
            Packet.WriteInteger((int)(PlusEnvironment.GetUnixTimestamp() - Timestamp)); //Created Secs Ago
            Packet.WriteInteger(Posts.Count); //Message count
            Packet.WriteInteger(GetUnreadMessages(Session.GetHabbo().Id)); //Unread message count
            Packet.WriteInteger(1); // Message List Lentgh

            Packet.WriteInteger(!isn ? lastpost.GetAuthor().Id : 0);// Las user to message id
            Packet.WriteString(!isn ? lastpost.GetAuthor().Username : ""); //Last user to message name
            Packet.WriteInteger(!isn ? (int)(PlusEnvironment.GetUnixTimestamp() - lastpost.Timestamp) : 0); //Last message timestamp

            Packet.WriteByte(DeletedLevel * 10); //thread Deleted Level

            var deleter = GetDeleter();
            if (deleter != null)
            {
                Packet.WriteInteger(deleter.Id);// deleter user id
                Packet.WriteString(deleter.Username); //deleter user name
                Packet.WriteInteger((int)(PlusEnvironment.GetUnixTimestamp() - DeletedTimestamp));//deleted secs ago
            }
            else
            {
                Packet.WriteInteger(1);// deleter user id
                Packet.WriteString("unknow"); //deleter user name
                Packet.WriteInteger(0);//deleted secs ago
            }
        }

        public GroupForumThreadPost GetPost(int postId) => Posts.FirstOrDefault(c => c.Id == postId);

        public async Task Save()
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("UPDATE `group_forums_threads` SET `pinned` = @pinned, `locked` = @locked, `deleted_level` = @dl, `deleter_user_id` = @duid WHERE `id` = @id;",
                new { pinned = Pinned ? 1 : 0, locked = Locked ? 1 : 0, dl = DeletedLevel, duid = DeleterUserId, id = Id });
        }
    }
}
