using Dapper;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetForumsListEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public GetForumsListEvent(IDatabase database) => _database = database;

    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        var viewOrderID = Packet.PopInt();
        var forumListIndex = Packet.PopInt();
        int forumListLength = Packet.PopInt();

        /*
         * My groups = 2
         * Most Active = 0
         * Most views = 1
         */

        List<GroupForum> forums = new List<GroupForum>();

        switch (viewOrderID)
        {
            case 2:
                List<GroupForum> Forums = PlusEnvironment.GetGame().GetGroupForumManager().GetForumsByUserId(Session.GetHabbo().Id);

                if (Forums.Count - 1 >= forumListIndex)
                {
                    Forums = Forums.GetRange(forumListIndex, Math.Min(forumListLength, Forums.Count));
                }
                Session.SendPacket(new ForumsListDataComposer(Forums, Session, viewOrderID, forumListIndex, forumListLength));
                return;

            case 0:
                using (var connection = _database.Connection())
                {
                    var results = await connection.QueryAsync("SELECT g.id FROM groups as g INNER JOIN group_forums_thread_posts as posts, group_forums_threads as threads WHERE posts.thread_id = threads.id AND @now - posts.`timestamp`<= @sdays AND threads.forum_id = g.id GROUP BY g.id ORDER BY posts.`timestamp` DESC LIMIT @index, @limit", 
                        new { limit = forumListLength, index = forumListIndex, now = (int)PlusEnvironment.GetUnixTimestamp(), sdays = (60 * 60 * 24 * 7) });

                    foreach (var result in results)
                    {
                        if (PlusEnvironment.GetGame().GetGroupForumManager().TryGetForum(Convert.ToInt32(result.id), out GroupForum forum))
                            forums.Add(forum);
                    }
                    break;
                }

            case 1:
                using (var connection = _database.Connection())
                {
                    var results = await connection.QueryAsync("SELECT g.id FROM groups as g INNER JOIN group_forums_thread_views as v, group_forums_threads as threads WHERE v.thread_id = threads.id AND threads.forum_id = g.id AND  @now - v.`timestamp` <= @sdays GROUP BY g.id ORDER BY v.`timestamp` DESC LIMIT @index, @limit",
                        new { limit = forumListLength, index = forumListIndex, now = (int)PlusEnvironment.GetUnixTimestamp(), sdays = (60 * 60 * 24 * 7) });

                    foreach (var result in results)
                    {
                        if (PlusEnvironment.GetGame().GetGroupForumManager().TryGetForum(Convert.ToInt32(result.id), out GroupForum forum))
                            forums.Add(forum);
                    }
                    break;
                }
        }

        Session.SendPacket(new ForumsListDataComposer(forums, Session, viewOrderID, forumListIndex, forumListLength));
    }
}
