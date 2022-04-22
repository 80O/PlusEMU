﻿using System.Text;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room"; }
        }

        public string Parameters
        {
            get { return "push/pull/enables/respect"; }
        }

        public string Description
        {
            get { return "Gives you the ability to enable or disable basic room commands."; }
        }

        public void Execute(GameClients.GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Oops, you must choose a room option to disable.");
                return;
            }

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Oops, only the room owner or staff can use this command.");
                return;
            }

            var option = @params[1];
            switch (option)
            {
                case "list":
                {
                    var list = new StringBuilder("");
                    list.AppendLine("Room Command List");
                    list.AppendLine("-------------------------");
                    list.AppendLine("Pet Morphs: " + (room.PetMorphsAllowed == true ? "enabled" : "disabled"));
                    list.AppendLine("Pull: " + (room.PullEnabled == true ? "enabled" : "disabled"));
                    list.AppendLine("Push: " + (room.PushEnabled == true ? "enabled" : "disabled"));
                    list.AppendLine("Super Pull: " + (room.SuperPullEnabled == true ? "enabled" : "disabled"));
                    list.AppendLine("Super Push: " + (room.SuperPushEnabled == true ? "enabled" : "disabled"));
                    list.AppendLine("Respect: " + (room.RespectNotificationsEnabled == true ? "enabled" : "disabled"));
                    list.AppendLine("Enables: " + (room.EnablesEnabled == true ? "enabled" : "disabled"));
                    session.SendNotification(list.ToString());
                    break;
                }

                case "push":
                    {
                        room.PushEnabled = !room.PushEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + room.Id +"' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(room.PushEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Push mode is now " + (room.PushEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "spush":
                    {
                        room.SuperPushEnabled = !room.SuperPushEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(room.SuperPushEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Super Push mode is now " + (room.SuperPushEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "spull":
                    {
                        room.SuperPullEnabled = !room.SuperPullEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(room.SuperPullEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Super Pull mode is now " + (room.SuperPullEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "pull":
                    {
                        room.PullEnabled = !room.PullEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pull_enabled` = @PullEnabled WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(room.PullEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Pull mode is now " + (room.PullEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "enable":
                case "enables":
                    {
                        room.EnablesEnabled = !room.EnablesEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnablesEnabled", PlusEnvironment.BoolToEnum(room.EnablesEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Enables mode set to " + (room.EnablesEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "respect":
                    {
                        room.RespectNotificationsEnabled = !room.RespectNotificationsEnabled;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RespectNotificationsEnabled", PlusEnvironment.BoolToEnum(room.RespectNotificationsEnabled));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Respect notifications mode set to " + (room.RespectNotificationsEnabled == true ? "enabled!" : "disabled!"));
                        break;
                    }

                case "pets":
                case "morphs":
                    {
                        room.PetMorphsAllowed = !room.PetMorphsAllowed;
                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PetMorphsAllowed", PlusEnvironment.BoolToEnum(room.PetMorphsAllowed));
                            dbClient.RunQuery();
                        }

                        session.SendWhisper("Human pet morphs notifications mode set to " + (room.PetMorphsAllowed == true ? "enabled!" : "disabled!"));
                        
                        if (!room.PetMorphsAllowed)
                        {
                            foreach (var user in room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                                    continue;

                                user.GetClient().SendWhisper("The room owner has disabled the ability to use a pet morph in this room.");
                                if (user.GetClient().GetHabbo().PetId > 0)
                                {
                                    //Tell the user what is going on.
                                    user.GetClient().SendWhisper("Oops, the room owner has just disabled pet-morphs, un-morphing you.");
                                    
                                    //Change the users Pet Id.
                                    user.GetClient().GetHabbo().PetId = 0;

                                    //Quickly remove the old user instance.
                                    room.SendPacket(new UserRemoveComposer(user.VirtualId));

                                    //Add the new one, they won't even notice a thing!!11 8-)
                                    room.SendPacket(new UsersComposer(user));
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
