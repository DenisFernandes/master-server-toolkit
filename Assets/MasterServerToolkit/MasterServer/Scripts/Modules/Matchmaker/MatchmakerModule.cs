﻿using MasterServerToolkit.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterServerToolkit.MasterServer
{
    public class MatchmakerModule : BaseServerModule
    {
        /// <summary>
        /// List of game providers
        /// </summary>
        public HashSet<IGamesProvider> GameProviders { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        protected SpawnersModule spawnersModule;

        protected override void Awake()
        {
            base.Awake();

            AddOptionalDependency<RoomsModule>();
            AddOptionalDependency<LobbiesModule>();
            AddDependency<SpawnersModule>();
        }

        public override void Initialize(IServer server)
        {
            GameProviders = new HashSet<IGamesProvider>();

            var roomsModule = server.GetModule<RoomsModule>();
            var lobbiesModule = server.GetModule<LobbiesModule>();
            spawnersModule = server.GetModule<SpawnersModule>();

            if (!spawnersModule) throw new Exception("SpawnersModule not found");

            // Dependencies
            if (roomsModule != null)
            {
                AddProvider(roomsModule);
            }

            if (lobbiesModule != null)
            {
                AddProvider(lobbiesModule);
            }

            // Add handlers
            server.RegisterMessageHandler((short)MstMessageCodes.FindGamesRequest, FindGamesRequestHandler);
            server.RegisterMessageHandler((short)MstMessageCodes.GetRegionsRequest, GetRegionsRequestHandler);
            server.RegisterMessageHandler((short)MstMessageCodes.GetPlayersRequest, GetPlayersRequestHandler);
        }

        /// <summary>
        /// Add given provider to list
        /// </summary>
        /// <param name="provider"></param>
        public void AddProvider(IGamesProvider provider)
        {
            GameProviders.Add(provider);
        }

        #region INCOMING MESSAGES HANDLERS

        protected virtual void FindGamesRequestHandler(IIncomingMessage message)
        {
            try
            {
                var list = new List<GameInfoPacket>();

                var filters = MstProperties.FromBytes(message.AsBytes());

                foreach (var provider in GameProviders)
                {
                    list.AddRange(provider.GetPublicGames(message.Peer, filters));
                }

                if (list.Count == 0)
                {
                    throw new MstMessageHandlerException("Not game found. Try to create your own game", ResponseStatus.Default);
                }

                // Convert to generic list and serialize to bytes
                var bytes = list.Select(l => (ISerializablePacket)l).ToBytes();
                message.Respond(bytes, ResponseStatus.Success);
            }
            // If we got system exception
            catch (MstMessageHandlerException e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, e.Status);
            }
            // If we got another exception
            catch (Exception e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, ResponseStatus.Error);
            }
        }

        protected virtual void GetRegionsRequestHandler(IIncomingMessage message)
        {
            try
            {
                var list = spawnersModule.GetRegions();

                if (list.Count == 0)
                {
                    throw new MstMessageHandlerException("No regions found. Please start spawner to get regions", ResponseStatus.Default);
                }

                message.Respond(new RegionsPacket()
                {
                    Regions = list
                }, ResponseStatus.Success);
            }
            // If we got system exception
            catch (MstMessageHandlerException e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, e.Status);
            }
            // If we got another exception
            catch (Exception e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, ResponseStatus.Error);
            }
        }

        protected virtual void GetPlayersRequestHandler(IIncomingMessage message)
        {
            try
            {
                // Trying to get room module
                var roomsModule = Server.GetModule<RoomsModule>();

                if (roomsModule == null)
                    throw new MstMessageHandlerException("No rooms module found on master server", ResponseStatus.Failed);

                // Parse room id
                int roomId = message.AsInt();

                // Find room
                var room = roomsModule.GetRoom(roomId);

                if (room == null)
                    throw new MstMessageHandlerException($"No room found with id:{roomId} on master server", ResponseStatus.Failed);

                // Check if player is not a member of this room
                if (!room.Players.ContainsKey(message.Peer.Id))
                    throw new MstMessageHandlerException($"You are not a member of the room with id:{roomId}", ResponseStatus.Unauthorized);

                var list = room.Players.Values
                    .Where(pl => pl.HasExtension<IUserPeerExtension>())
                    .Select(pl => pl.GetExtension<IUserPeerExtension>().Username)
                    .ToList();

                if (list.Count == 0)
                {
                    throw new MstMessageHandlerException("No player found", ResponseStatus.Default);
                }

                message.Respond(new PlayersPacket()
                {
                    Players = list
                }, ResponseStatus.Success);
            }
            // If we got system exception
            catch (MstMessageHandlerException e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, e.Status);
            }
            // If we got another exception
            catch (Exception e)
            {
                logger.Error(e.Message);
                message.Respond(e.Message, ResponseStatus.Error);
            }
        }

        #endregion
    }
}