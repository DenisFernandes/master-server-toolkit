﻿using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using CommandTerminal;
using System;
using System.Text;
using UnityEngine;
using System.Linq;

namespace MasterServerToolkit.Client.Utilities
{
    public static class ClientChatTerminalCommands
    {
        static string tempMessage = string.Empty;

        [RegisterCommand(Name = "cl.chat.msg", Help = "Send the chat message to all clients. 1 Message", MinArgCount = 1)]
        private static void SendMessage(CommandArg[] args)
        {
            string[] argsArray = args.Select(i => i.String).ToArray();

            tempMessage = Mst.Helper.JoinCommandArgs(argsArray, 1);
            Mst.Client.Chat.SendPrivateMessage(args[0].String, tempMessage, OnSuccess);
        }

        [RegisterCommand(Name = "cl.chat.msgto", Help = "Send the chat message to client. 1 Username, 2 Message", MinArgCount = 2)]
        private static void SendPrivateMessage(CommandArg[] args)
        {
            string[] argsArray = args.Select(i => i.String).ToArray();

            tempMessage = Mst.Helper.JoinCommandArgs(argsArray, 1);
            Mst.Client.Chat.SendPrivateMessage(args[0].String, tempMessage, OnSuccess);
        }

        private static void OnSuccess(bool isSuccessful, string error)
        {
            if (isSuccessful)
            {
                Logs.Info($"Message: {tempMessage}");
            }
        }
    }
}