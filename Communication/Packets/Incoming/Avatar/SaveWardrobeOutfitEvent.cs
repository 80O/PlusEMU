using System.Threading.Tasks;
using Plus.Core.FigureData;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Wardrobe;

namespace Plus.Communication.Packets.Incoming.Avatar;

internal class SaveWardrobeOutfitEvent : IPacketEvent
{
    private readonly IFigureDataManager _figureDataManager;
    private readonly IUserWardrobeManager _userWardrobeManager;

    public SaveWardrobeOutfitEvent(IFigureDataManager figureDataManager, IUserWardrobeManager userWardrobeManager)
    {
        _figureDataManager = figureDataManager;
        _userWardrobeManager = userWardrobeManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var slotId = packet.PopInt();
        var look = packet.PopString();
        var gender = packet.PopString();
        look = _figureDataManager.ProcessFigure(look, gender, session.GetHabbo().GetClothing().GetClothingParts, true);

        _userWardrobeManager.SaveOutfit(session.GetHabbo().Id, slotId, gender, look);

        return Task.CompletedTask;
    }
}