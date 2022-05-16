using Dapper;
using Plus.Communication.Packets.Outgoing;
using Plus.Database;
using Plus.HabboHotel.Users;
using System;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumThreadPost
    {
        private readonly IDatabase _database;
        public int Id;
        public int UserId;
        public int Timestamp;
        public string Message;

        public int DeleterId;
        public int DeletedLevel;

        public GroupForumThread ParentThread;
        public GroupForumThreadPost(IDatabase database, GroupForumThread parent, int id, int userid, int timestamp, string message, int deletedlevel, int deleterid)
        {
            _database = database;
            ParentThread = parent;
            Id = id;
            UserId = userid;
            Timestamp = timestamp;
            Message = message;

            DeleterId = deleterid;
            DeletedLevel = deletedlevel;

        }

        public Habbo GetDeleter() => PlusEnvironment.GetHabboById(DeleterId);

        public Habbo GetAuthor() => PlusEnvironment.GetHabboById(UserId);

        public void SerializeData(ServerPacket Packet)
        {

            var User = GetAuthor();
            var oculterData = GetDeleter();
            Packet.WriteInteger(Id); //Post Id
            Packet.WriteInteger(ParentThread.Posts.IndexOf(this)); //Post Index

            Packet.WriteInteger(User.Id); //User id
            Packet.WriteString(User.Username); //Username
            Packet.WriteString(User.Look); //User look

            Packet.WriteInteger((int)(PlusEnvironment.GetUnixTimestamp() - Timestamp)); //User message timestamp
            Packet.WriteString(Message); // Message text
            Packet.WriteByte(DeletedLevel * 10); // User message oculted by - level
            Packet.WriteInteger(oculterData != null ? oculterData.Id : 0); // User that oculted message ID
            Packet.WriteString(oculterData != null ? oculterData.Username : "Unknown"); //Oculted message user name
            Packet.WriteInteger(242342340);
            Packet.WriteInteger(ParentThread.GetUserPosts(User.Id).Count); //User messages count
        }

        public async Task Save()
        {
            using var connection = _database.Connection();
            await connection.ExecuteAsync("UPDATE group_forums_thread_posts SET deleted_level = @dl, deleter_user_id = @duid WHERE id = @id", new { dl = DeletedLevel, duid = DeleterId, id = Id });
        }
    }
}
