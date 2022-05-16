namespace Plus.HabboHotel.Groups.Forums
{
    public class GroupForumThreadPostView
    {
        public int Id;
        public int UserId;
        //public int Timestamp;
        public int Count;

        public GroupForumThreadPostView(int id, int userid, int count)
        {
            Id = id;
            UserId = userid;
            //Timestamp = timestamp;
            Count = count;
        }
    }
}
