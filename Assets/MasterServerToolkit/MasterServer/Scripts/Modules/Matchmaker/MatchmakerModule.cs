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

            AddOptionalDependency<LobbiesModule>();
            AddDependency<SpawnersModule>();
        }

        public override void Initialize(IServer server)
        {
            GameProviders = new HashSet<IGamesProvider>();

            var roomsModule = server.GetModule<RoomsModule>();
            var lobbiesModule = server.GetModule<LobbiesModule>();
            spawnersModule = server.GetModule<SpawnersModule>();

            if (!spawnersModule)
                logger.Error($"{GetType().Name} was set to use {nameof(SpawnersModule)}, but {nameof(SpawnersModule)} was not found." +
                    $"In this case, you will not be able to get regions list");

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
            server.RegisterMessageHandler((ushort)MstOpCodes.FindGamesRequest, FindGamesRequestHandler);
            server.RegisterMessageHandler((ushort)MstOpCodes.GetRegionsRequest, GetRegionsRequestHandler);
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

                foreach (var game in GameProviders.SelectMany(pr => pr.GetPublicGames(message.Peer, filters), (provider, game) => game))
                {
                    list.Add(game);
                }

                if (list.Count == 0)
                {
                    throw new MstMessageHandlerException("No game found. Try to create your own game", ResponseStatus.Default);
                }

                // Convert to generic list and serialize to bytes
                var bytes = list.Select(game => (ISerializablePacket)game).ToBytes();
                message.Respond(bytes, ResponseStatus.Success);
            }
            // If we got system exception
            catch (MstMessageHandlerException e)
            {
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
                if (!spawnersModule)
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
                else
                {
                    message.Respond(new RegionsPacket()
                    {
                        Regions = new List<RegionInfo>()
                    }, ResponseStatus.Success);
                }
            }
            // If we got system exception
            catch (MstMessageHandlerException e)
            {
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