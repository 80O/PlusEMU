using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Data.Toner;

namespace Plus.HabboHotel.Items;

internal static class ItemBehaviourUtility
{
    public static void GenerateExtradata(Item item, ServerPacket packet)
    {
        switch (item.GetBaseItem().InteractionType)
        {
            default:
                packet.WriteInteger(1);
                packet.WriteInteger(0);
                packet.WriteString(item.GetBaseItem().InteractionType != InteractionType.FootballGate ? item.ExtraData : string.Empty);
                break;
            case InteractionType.GnomeBox:
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                packet.WriteString("");
                break;
            case InteractionType.PetBreedingBox:
            case InteractionType.PurchasableClothing:
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                packet.WriteString("0");
                break;
            case InteractionType.Stacktool:
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                packet.WriteString("");
                break;
            case InteractionType.Wallpaper:
                packet.WriteInteger(2);
                packet.WriteInteger(0);
                packet.WriteString(item.ExtraData);
                break;
            case InteractionType.Floor:
                packet.WriteInteger(3);
                packet.WriteInteger(0);
                packet.WriteString(item.ExtraData);
                break;
            case InteractionType.Landscape:
                packet.WriteInteger(4);
                packet.WriteInteger(0);
                packet.WriteString(item.ExtraData);
                break;
            case InteractionType.GuildItem:
            case InteractionType.GuildGate:
            case InteractionType.GuildForum:
                Group group = null;
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(item.GroupId, out group))
                {
                    packet.WriteInteger(1);
                    packet.WriteInteger(0);
                    packet.WriteString(item.ExtraData);
                }
                else
                {
                    packet.WriteInteger(0);
                    packet.WriteInteger(2);
                    packet.WriteInteger(5);
                    packet.WriteString(item.ExtraData);
                    packet.WriteString(group.Id.ToString());
                    packet.WriteString(group.Badge);
                    packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
                    packet.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
                }
                break;
            case InteractionType.Background:
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                if (!string.IsNullOrEmpty(item.ExtraData))
                {
                    packet.WriteInteger(item.ExtraData.Split(Convert.ToChar(9)).Length / 2);
                    for (var i = 0; i <= item.ExtraData.Split(Convert.ToChar(9)).Length - 1; i++) packet.WriteString(item.ExtraData.Split(Convert.ToChar(9))[i]);
                }
                else
                    packet.WriteInteger(0);
                break;
            case InteractionType.Gift:
                {
                    var extraData = item.ExtraData.Split(Convert.ToChar(5));
                    if (extraData.Length != 7)
                    {
                        packet.WriteInteger(0);
                        packet.WriteInteger(0);
                        packet.WriteString(item.ExtraData);
                    }
                    else
                    {
                        var style = int.Parse(extraData[6]) * 1000 + int.Parse(extraData[6]);
                        var purchaser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(extraData[2]));
                        if (purchaser == null)
                        {
                            packet.WriteInteger(0);
                            packet.WriteInteger(0);
                            packet.WriteString(item.ExtraData);
                        }
                        else
                        {
                            packet.WriteInteger(style);
                            packet.WriteInteger(1);
                            packet.WriteInteger(6);
                            packet.WriteString("EXTRA_PARAM");
                            packet.WriteString("");
                            packet.WriteString("MESSAGE");
                            packet.WriteString(extraData[1]);
                            packet.WriteString("PURCHASER_NAME");
                            packet.WriteString(purchaser.Username);
                            packet.WriteString("PURCHASER_FIGURE");
                            packet.WriteString(purchaser.Look);
                            packet.WriteString("PRODUCT_CODE");
                            packet.WriteString("A1 KUMIANKKA");
                            packet.WriteString("state");
                            packet.WriteString(item.MagicRemove ? "1" : "0");
                        }
                    }
                }
                break;
            case InteractionType.Mannequin:
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                packet.WriteInteger(3);
                if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                {
                    var stuff = item.ExtraData.Split(Convert.ToChar(5));
                    packet.WriteString("GENDER");
                    packet.WriteString(stuff[0]);
                    packet.WriteString("FIGURE");
                    packet.WriteString(stuff[1]);
                    packet.WriteString("OUTFIT_NAME");
                    packet.WriteString(stuff[2]);
                }
                else
                {
                    packet.WriteString("GENDER");
                    packet.WriteString("");
                    packet.WriteString("FIGURE");
                    packet.WriteString("");
                    packet.WriteString("OUTFIT_NAME");
                    packet.WriteString("");
                }
                break;
            case InteractionType.Toner:
                if (item.RoomId != 0)
                {
                    if (item.GetRoom().TonerData == null)
                        item.GetRoom().TonerData = new TonerData(item.Id);
                    packet.WriteInteger(0);
                    packet.WriteInteger(5);
                    packet.WriteInteger(4);
                    packet.WriteInteger(item.GetRoom().TonerData.Enabled);
                    packet.WriteInteger(item.GetRoom().TonerData.Hue);
                    packet.WriteInteger(item.GetRoom().TonerData.Saturation);
                    packet.WriteInteger(item.GetRoom().TonerData.Lightness);
                }
                else
                {
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString(string.Empty);
                }
                break;
            case InteractionType.BadgeDisplay:
                packet.WriteInteger(0);
                packet.WriteInteger(2);
                packet.WriteInteger(4);
                var badgeData = item.ExtraData.Split(Convert.ToChar(9));
                if (item.ExtraData.Contains(Convert.ToChar(9).ToString()))
                {
                    packet.WriteString("0"); //No idea
                    packet.WriteString(badgeData[0]); //Badge name
                    packet.WriteString(badgeData[1]); //Owner
                    packet.WriteString(badgeData[2]); //Date
                }
                else
                {
                    packet.WriteString("0"); //No idea
                    packet.WriteString("DEV"); //Badge name
                    packet.WriteString("Sledmore"); //Owner
                    packet.WriteString("13-13-1337"); //Date
                }
                break;
            case InteractionType.Television:
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                packet.WriteInteger(1);
                packet.WriteString("THUMBNAIL_URL");
                //Message.WriteString("http://img.youtube.com/vi/" + PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList.OrderBy(x => Guid.NewGuid()).FirstOrDefault().YouTubeId + "/3.jpg");
                packet.WriteString("");
                break;
            case InteractionType.Lovelock:
                if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                {
                    var eData = item.ExtraData.Split((char)5);
                    var I = 0;
                    packet.WriteInteger(0);
                    packet.WriteInteger(2);
                    packet.WriteInteger(eData.Length);
                    while (I < eData.Length)
                    {
                        packet.WriteString(eData[I]);
                        I++;
                    }
                }
                else
                {
                    packet.WriteInteger(0);
                    packet.WriteInteger(0);
                    packet.WriteString("0");
                }
                break;
            case InteractionType.MonsterplantSeed:
                packet.WriteInteger(0);
                packet.WriteInteger(1);
                packet.WriteInteger(1);
                packet.WriteString("rarity");
                packet.WriteString("1"); //Leve should be dynamic.
                break;
        }
    }

    public static void GenerateWallExtradata(Item item, ServerPacket message)
    {
        switch (item.GetBaseItem().InteractionType)
        {
            default:
                message.WriteString(item.ExtraData);
                break;
            case InteractionType.Postit:
                message.WriteString(item.ExtraData.Split(' ')[0]);
                break;
        }
    }
}