using System.Threading.Tasks;
using Plus.Communication.Attributes;
using Plus.Communication.Encryption;
using Plus.Communication.Encryption.Crypto.Prng;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake;

[NoAuthenticationRequired]
public class GenerateSecretKeyEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var cipherPublickey = packet.PopString();
        var sharedKey = HabboEncryptionV2.CalculateDiffieHellmanSharedKey(cipherPublickey);
        if (sharedKey != 0)
        {
            session.Rc4Client = new Arc4(sharedKey.getBytes());
            session.SendPacket(new SecretKeyComposer(HabboEncryptionV2.GetRsaDiffieHellmanPublicKey()));
        }
        else
            session.SendNotification("There was an error logging you in, please try again!");
        return Task.CompletedTask;
    }
}