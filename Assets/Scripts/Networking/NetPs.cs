using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Logging;
using System;
using MLAPI.Messaging;

public class NetPs : NetworkBehaviour
{
    [SerializeField]
    private int rowSize;
    [SerializeField]
    private int colSize;

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += ProcessServerStart;
    }

    private void Start()
    {
        NetEventSystem.Instance.OnRequestSpectatorOff += ProcessSpectatorOffRequest;
    }

    private void ProcessServerStart()
    {
        if (!IsServer)
        {
            return;
        }
        playerCount = 0;
        readyPlayerCount = 0;
        players = new Dictionary<byte, int>();
        placedTiles = new SortedSet<int>();
        playerTrails = new Dictionary<byte, Queue<int>>();
    }

    public enum Side
    {
        White,
        Black
    }

    private byte playerCount;
    private byte readyPlayerCount;
    private bool isSpectatorOffRequestSent;
    Dictionary<byte, int> players;
    SortedSet<int> placedTiles;
    Dictionary<byte, Queue<int>> playerTrails;
    Queue<int> currentTrail;

    public void RequestNewPlayerId(byte colorIndex, int row, int col, bool isSelectedPlayer)
    {
        SendNewPlayerDataServerRpc(colorIndex, row, col, isSelectedPlayer);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendNewPlayerDataServerRpc(byte colorIndex, int row, int col, bool isSelectedPlayer)
    {
        int tilePos = One(row, col);
        byte playerId = playerCount;
        if (isSelectedPlayer)
        {
            players.Add(playerCount, tilePos);
            playerTrails.Add(playerCount, new Queue<int>());
            placedTiles.Add(tilePos);
            playerCount++;
        }
        SendNewPlayerDataClientRpc(colorIndex, playerId);
    }
    [ClientRpc]
    public void SendNewPlayerDataClientRpc(byte colorIndex, byte playerId)
    {
        NetEventSystem.Instance.PassPlayerId(colorIndex, playerId);
    }
    public void RequestMove(int row, int col, byte id)
    {
        RequestMoveServerRpc(row, col, id);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveServerRpc(int row, int col, byte id)
    {
        if (!IsIndexValid((row, col)))
        {
            return;
        }
        int tilePos = One(row, col);
        if (placedTiles.Contains(tilePos))
        {
            return;
        }
        NetworkLog.LogInfoServer("Request server check");
        if (!players.TryGetValue(id, out int prevPos))
        {
            return;
        }
        NetworkLog.LogInfoServer("player Move");
        players[id] = tilePos;
        placedTiles.Add(tilePos);
        //placedTiles.Remove(prevPos);
        if (!playerTrails.TryGetValue(id, out currentTrail))
        {
            NetworkLog.LogErrorServer("player trail was not found");
        }
        playerTrails[id].Enqueue(prevPos);
        if (currentTrail.Count > 4)
        {
            int deleteTrail = playerTrails[id].Dequeue();
            placedTiles.Remove(deleteTrail);
        }
        MakeMoveClientRpc(row, col, id);
    }
    [ClientRpc]
    private void MakeMoveClientRpc(int row, int col, byte id)
    {
        NetEventSystem.Instance.MovePlayer(id, (row, col));
    }

    public void RequestGridChange()
    {
        RequestGridChangeServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestGridChangeServerRpc()
    {
        ChangeGridClientRpc();
    }
    [ClientRpc]
    private void ChangeGridClientRpc()
    {
        NetEventSystem.Instance.SwitchGridType();
    }

    private void ProcessSpectatorOffRequest()
    {
        if (!isSpectatorOffRequestSent)
        {
            SpectatorOffServerRpc();
            isSpectatorOffRequestSent = true;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpectatorOffServerRpc()
    {
        readyPlayerCount++;
        //NetworkLog.LogInfoServer("ss " + readyPlayerCount + " " + playerCount);
        if (readyPlayerCount == playerCount)
        {
            //NetworkLog.LogInfoServer("specOff");
            readyPlayerCount = 0;
            SpectatorOffClientRpc();   
        }
    }
    [ClientRpc]
    private void SpectatorOffClientRpc()
    {
        NetEventSystem.Instance.SpectatorOff();
        isSpectatorOffRequestSent = false;
    }


    private int One((int, int) index)
    {
        var (row, col) = index;
        return row * rowSize + col;
    }
    private int One(int row, int col)
    {
        return row * rowSize + col;
    }

    private bool IsIndexValid((int, int) index)
    {
        var (col, row) = index;
        if (row < 0 || row >= rowSize)
        {
            return false;
        }
        if (col < 0 || col >= colSize)
        {
            return false;
        }
        return true;
    }

    private bool isColorForFirstTeam(byte colorIndex)
    {
        return colorIndex < 2;
    }

}
