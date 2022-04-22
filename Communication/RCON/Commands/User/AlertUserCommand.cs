﻿using System;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Rcon.Commands.User
{
    class AlertUserCommand : IRconCommand
    {
        public string Description
        {
            get { return "This command is used to alert a user."; }
        }

        public string Parameters
        {
            get { return "%userId% %message%"; }
        }

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out var userId))
                return false;

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return false;

            // Validate the message
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
                return false;

            var message = Convert.ToString(parameters[1]);

            client.SendPacket(new BroadcastMessageAlertComposer(message));
            return true;
        }
    }
}