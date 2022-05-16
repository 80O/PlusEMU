using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class ThreadsListDataComposer : ServerPacket
{
    public ThreadsListDataComposer(GroupForum forum, GameClient session, int startIndex = 0, int maxLength = 20)
            : base(ServerPacketHeader.ThreadsListDataMessageComposer)
    {
        base.WriteInteger(forum.GroupId);
        base.WriteInteger(startIndex);

        var threads = forum.Threads;
        if (threads.Count - 1 >= startIndex)
            threads = threads.GetRange(startIndex, Math.Min(maxLength, threads.Count - startIndex));

        base.WriteInteger(threads.Count);

        var unpinnedList = new List<GroupForumThread>();

        foreach (var Thread in threads)
        {
            if (!Thread.Pinned)
            {
                unpinnedList.Add(Thread);
                continue;
            }

            Thread.SerializeData(session, this);
        }

        foreach (var unPinned in unpinnedList)
        {
            unPinned.SerializeData(session, this);
        }
    }
}
