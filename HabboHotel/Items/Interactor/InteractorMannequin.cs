using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor;

internal class InteractorMannequin : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item) { }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
        {
            var stuff = item.ExtraData.Split(Convert.ToChar(5));
            session.GetHabbo().Gender = stuff[0].ToUpper();
            var newFig = new Dictionary<string, string>();
            newFig.Clear();
            foreach (var man in stuff[1].Split('.'))
            {
                foreach (var fig in session.GetHabbo().Look.Split('.'))
                {
                    if (fig.Split('-')[0] == man.Split('-')[0])
                    {
                        if (newFig.ContainsKey(fig.Split('-')[0]) && !newFig.ContainsValue(man))
                        {
                            newFig.Remove(fig.Split('-')[0]);
                            newFig.Add(fig.Split('-')[0], man);
                        }
                        else if (!newFig.ContainsKey(fig.Split('-')[0]) && !newFig.ContainsValue(man)) newFig.Add(fig.Split('-')[0], man);
                    }
                    else
                    {
                        if (!newFig.ContainsKey(fig.Split('-')[0])) newFig.Add(fig.Split('-')[0], fig);
                    }
                }
            }
            var final = "";
            foreach (var str in newFig.Values) final += str + ".";
            session.GetHabbo().Look = final.TrimEnd('.');
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE id = '" + session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", session.GetHabbo().Look);
                dbClient.AddParameter("gender", session.GetHabbo().Gender);
                dbClient.RunQuery();
            }
            var room = session.GetHabbo().CurrentRoom;
            if (room != null)
            {
                var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
                if (user != null)
                {
                    session.SendPacket(new UserChangeComposer(user, true));
                    session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(user, false));
                }
            }
        }
    }

    public void OnWiredTrigger(Item item) { }
}