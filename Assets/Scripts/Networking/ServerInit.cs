using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInit : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
    }
    private void HandleServerStarted()
    {
        NetEventSystem.Instance.SetGridType(NetGridSwapper.GridType.Tri);
    } 
}
