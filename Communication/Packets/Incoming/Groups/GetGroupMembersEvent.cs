using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class GetGroupMembersEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly ICacheManager _cacheManager;

    public GetGroupMembersEvent(IGroupManager groupManager, ICacheManager cacheManager)
    {
        _groupManager = groupManager;
        _cacheManager = cacheManager;
    }
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        var page = packet.PopInt();
        var searchVal = packet.PopString();
        var requestType = packet.PopInt();
        if (!_groupManager.TryGetGroup(groupId, out var group))
            return Task.CompletedTask;
        var members = new List<UserCache>();
        switch (requestType)
        {
            case 0:
            {
                var memberIds = group.GetAllMembers;
                foreach (var id in memberIds.ToList())
                {
                    var groupMember = _cacheManager.GenerateUser(id);
                    if (groupMember == null)
                        continue;
                    if (!members.Contains(groupMember))
                        members.Add(groupMember);
                }
                break;
            }
            case 1:
            {
                var adminIds = group.GetAdministrators;
                foreach (var id in adminIds.ToList())
                {
                    var groupMember = _cacheManager.GenerateUser(id);
                    if (groupMember == null)
                        continue;
                    if (!members.Contains(groupMember))
                        members.Add(groupMember);
                }
                break;
            }
            case 2:
            {
                var requestIds = group.GetRequests;
                foreach (var id in requestIds.ToList())
                {
                    var groupMember = _cacheManager.GenerateUser(id);
                    if (groupMember == null)
                        continue;
                    if (!members.Contains(groupMember))
                        members.Add(groupMember);
                }
                break;
            }
        }
        if (!string.IsNullOrEmpty(searchVal))
            members = members.Where(x => x.Username.StartsWith(searchVal)).ToList();
        var startIndex = (page - 1) * 14 + 14;
        var finishIndex = members.Count;
        session.SendPacket(new GroupMembersComposer(group, members.Skip(startIndex).Take(finishIndex - startIndex).ToList(), members.Count, page,
            group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id), requestType, searchVal));
        return Task.CompletedTask;
    }
}