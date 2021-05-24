using MLAPI;
using MLAPI.Messaging;
using MLAPI.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.NetworkVariable;
using TMPro;
using UnityEngine.UI;

public class NetColorSelect : NetworkBehaviour
{
    [SerializeField]
    private NetPlayerSelection[] selectImage;
    byte selectCount;
    public Queue<byte> orderOfColors;
    public Queue<ulong> orderOfPlayers; 
    private Color opaque;
    private void Awake()
    {
        opaque = new Color(1, 1, 1, 1);
        orderOfColors = new Queue<byte>();
        orderOfPlayers= new Queue<ulong>();
        selectCount = 0;
    }
    private void Start()
    {
        NetEventSystem.Instance.OnResetSingle += ProcessReset;
    }

    private void ProcessReset()
    {
        orderOfColors = new Queue<byte>();
        orderOfPlayers = new Queue<ulong>();
        selectCount = 0;
    }

    public bool TrySelectColor(byte colorIndex)
    {
        Debug.Log(selectImage[colorIndex].CheckAlpha());
        if (!selectImage[colorIndex].CheckAlpha())
        {
            return false;
        }
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("Calling Server");
        SelectServerRpc(colorIndex, clientId);
        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SelectServerRpc(byte colorIndex, ulong clientId)
    {
        Debug.Log("Server");
        orderOfColors.Enqueue(colorIndex);
        orderOfPlayers.Enqueue(clientId);
        selectCount++;
        SelectClientRpc(colorIndex, selectCount);
    }
    [ClientRpc]
    public void SelectClientRpc(byte colorIndex, byte serverSelectCount)
    {
        Debug.Log("visible");
        selectImage[colorIndex].SetAlphaServerRpc(1);
        selectImage[colorIndex].SetTextServerRpc("P" + serverSelectCount);
    }
}
