using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Filter;

namespace Plus.Communication.Packets.Incoming.Catalog;

public class CheckPetNameEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;

    public CheckPetNameEvent(IWordFilterManager wordFilterManager)
    {
        _wordFilterManager = wordFilterManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var petName = packet.PopString();
        if (petName.Length < 2)
        {
            session.SendPacket(new CheckPetNameComposer(2, "2"));
            return Task.CompletedTask;
        }
        if (petName.Length > 15)
        {
            session.SendPacket(new CheckPetNameComposer(1, "15"));
            return Task.CompletedTask;
        }
        if (!PetUtility.CheckPetName(petName))
        {
            session.SendPacket(new CheckPetNameComposer(3, string.Empty));
            return Task.CompletedTask;
        }

        if (_wordFilterManager.IsFiltered(petName))
        {
            session.SendPacket(new CheckPetNameComposer(4, string.Empty));
            return Task.CompletedTask;
        }
        session.SendPacket(new CheckPetNameComposer(0, string.Empty));
        return Task.CompletedTask;
    }
}