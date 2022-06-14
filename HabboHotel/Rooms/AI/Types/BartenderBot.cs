using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.AI.Types;

internal class BartenderBot : BotAi
{
    private readonly int _virtualId;
    private int _actionTimer;
    private int _speechTimer;

    public BartenderBot(int virtualId)
    {
        _virtualId = virtualId;
    }

    public override void OnSelfEnterRoom() { }

    public override void OnSelfLeaveRoom(bool kicked) { }

    public override void OnUserEnterRoom(RoomUser user) { }

    public override void OnUserLeaveRoom(GameClient client)
    {
        //if ()
    }

    public override void OnUserSay(RoomUser user, string message)
    {
        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
            return;
        if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 8)
            return;
        var response = PlusEnvironment.GetGame().GetBotManager().GetResponse(GetBotData().AiType, message);
        if (response == null)
            return;
        switch (response.ResponseType.ToLower())
        {
            case "say":
                GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                break;
            case "shout":
                GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                break;
            case "whisper":
                user.GetClient().SendPacket(new WhisperComposer(GetRoomUser().VirtualId, response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username), 0, 0));
                break;
        }
        if (response.BeverageIds.Count > 0) user.CarryItem(response.BeverageIds[Random.Shared.Next(0, response.BeverageIds.Count)]);
    }

    public override void OnUserShout(RoomUser user, string message)
    {
        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
            return;
        if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 8)
            return;
        var response = PlusEnvironment.GetGame().GetBotManager().GetResponse(GetBotData().AiType, message);
        if (response == null)
            return;
        switch (response.ResponseType.ToLower())
        {
            case "say":
                GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                break;
            case "shout":
                GetRoomUser().Chat(response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username));
                break;
            case "whisper":
                user.GetClient().SendPacket(new WhisperComposer(GetRoomUser().VirtualId, response.ResponseText.Replace("{username}", user.GetClient().GetHabbo().Username), 0, 0));
                break;
        }
        if (response.BeverageIds.Count > 0) user.CarryItem(response.BeverageIds[Random.Shared.Next(0, response.BeverageIds.Count)]);
    }

    public override void OnTimerTick()
    {
        if (GetBotData() == null)
            return;
        if (_speechTimer <= 0)
        {
            if (GetBotData().RandomSpeech.Count > 0)
            {
                if (GetBotData().AutomaticChat == false)
                    return;
                var speech = GetBotData().GetRandomSpeech();
                var @string = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(speech.Message);
                if (@string.Contains("<img src") || @string.Contains("<font ") || @string.Contains("</font>") || @string.Contains("</a>") || @string.Contains("<i>"))
                    @string = "I really shouldn't be using HTML within bot speeches.";
                GetRoomUser().Chat(@string, GetBotData().ChatBubble);
            }
            _speechTimer = GetBotData().SpeakingInterval;
        }
        else
            _speechTimer--;
        if (_actionTimer <= 0)
        {
            Point nextCoord;
            switch (GetBotData().WalkingMode.ToLower())
            {
                default:
                case "stand":
                    // (8) Why is my life so boring?
                    break;
                case "freeroam":
                    if (GetBotData().ForcedMovement)
                    {
                        if (GetRoomUser().Coordinate == GetBotData().TargetCoordinate)
                        {
                            GetBotData().ForcedMovement = false;
                            GetBotData().TargetCoordinate = new Point();
                            GetRoomUser().MoveTo(GetBotData().TargetCoordinate.X, GetBotData().TargetCoordinate.Y);
                        }
                    }
                    else if (GetBotData().ForcedUserTargetMovement > 0)
                    {
                        var target = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(GetBotData().ForcedUserTargetMovement);
                        if (target == null)
                        {
                            GetBotData().ForcedUserTargetMovement = 0;
                            GetRoomUser().ClearMovement(true);
                        }
                        else
                        {
                            var sq = new Point(target.X, target.Y);
                            if (target.RotBody == 0)
                                sq.Y--;
                            else if (target.RotBody == 2)
                                sq.X++;
                            else if (target.RotBody == 4)
                                sq.Y++;
                            else if (target.RotBody == 6) sq.X--;
                            GetRoomUser().MoveTo(sq);
                        }
                    }
                    else if (GetBotData().TargetUser == 0)
                    {
                        nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                        GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                    }
                    break;
                case "specified_range":
                    break;
            }
            _actionTimer = Random.Shared.Next(5, 15);
        }
        else
            _actionTimer--;
    }
}