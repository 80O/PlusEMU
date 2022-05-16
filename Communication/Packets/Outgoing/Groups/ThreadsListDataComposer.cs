using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class ThreadsListDataComposer : ServerPacket
{
    public ThreadsListDataComposer(GroupForum Forum, GameClient Session, int StartIndex = 0, int MaxLength = 20)
            : base(ServerPacketHeader.ThreadsListDataMessageComposer)
    {
        base.WriteInteger(Forum.GroupId);
        base.WriteInteger(StartIndex);

        var Threads = Forum.Threads;
        if (Threads.Count - 1 >= StartIndex)
            Threads = Threads.GetRange(StartIndex, Math.Min(MaxLength, Threads.Count - StartIndex));

        base.WriteInteger(Threads.Count);

        var UnPinneds = new List<GroupForumThread>();

        foreach (var Thread in Threads)
        {
            if (!Thread.Pinned)
            {
                UnPinneds.Add(Thread);
                continue;
            }

            Thread.SerializeData(Session, this);
        }

        foreach (var unPinned in UnPinneds)
        {
            unPinned.SerializeData(Session, this);
        }
    }
}
