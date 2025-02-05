using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterServerToolkit.MasterServer
{
    public interface IFriendsInfoData
    {
        string UserId { get; set; }
        string[] UsersIds { get; set; }
        DateTime LastUpdate { get; set; }
        Dictionary<string, string> Properties { get; set; }
    }
}