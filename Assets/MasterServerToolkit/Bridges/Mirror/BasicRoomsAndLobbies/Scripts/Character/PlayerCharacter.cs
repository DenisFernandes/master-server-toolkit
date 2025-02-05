﻿#if MIRROR
using MasterServerToolkit.MasterServer;
using Mirror;
using System;

namespace MasterServerToolkit.Bridges.MirrorNetworking.Character
{
    public class PlayerCharacter : PlayerCharacterBehaviour
    {
        public static event Action<PlayerCharacter> OnServerCharacterSpawnedEvent;
        public static event Action<PlayerCharacter> OnClientCharacterSpawnedEvent;
        public static event Action<PlayerCharacter> OnLocalCharacterSpawnedEvent;

        public static event Action<PlayerCharacter> OnCharacterDestroyedEvent;

        private void OnDestroy()
        {
            OnCharacterDestroyedEvent?.Invoke(this);
        }

        #region SERVER

        public override void OnStartServer()
        {
            base.OnStartServer();
            OnServerCharacterSpawnedEvent?.Invoke(this);
        }

        #endregion

        #region CLIENT

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            OnLocalCharacterSpawnedEvent?.Invoke(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            OnClientCharacterSpawnedEvent?.Invoke(this);
        }

        #endregion
    }
}
#endif