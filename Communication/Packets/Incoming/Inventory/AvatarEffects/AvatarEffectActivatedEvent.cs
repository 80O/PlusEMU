using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Inventory.AvatarEffects;

internal class AvatarEffectActivatedEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var effectId = packet.PopInt();
        var effect = session.GetHabbo().Effects().GetEffectNullable(effectId, false, true);
        if (session.GetHabbo().Effects().HasEffect(effectId, true)) return Task.CompletedTask;
        if (effect.Activate()) session.SendPacket(new AvatarEffectActivatedComposer(effect));
        return Task.CompletedTask;
    }
}