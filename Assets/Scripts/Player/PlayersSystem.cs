using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Logging;
using System;

public class PlayersSystem : MonoBehaviour
{
    [SerializeField]
    private int rowSize;
    [SerializeField]
    private int colSize;

    private void Awake()
    {
        playerCount = 0;
        blackPlayers = new Dictionary<byte, int>();
        whitePlayers= new Dictionary<byte, int>();
        blackTiles = new SortedSet<int>();
        whiteTiles = new SortedSet<int>();
    }
    void Start()
    {
        NetEventSystem.Instance.OnResetSingle += Reset;
/*        NetEventSystem.Instance.OnSpectatorOn += ProcessSpectatorOn;
        NetEventSystem.Instance.OnSpectatorOff += ProcessSpectatorOff;*/
    }
    public void Reset()
    {
        playerCount = 0;
        blackPlayers = new Dictionary<byte, int>();
        whitePlayers = new Dictionary<byte, int>();
        blackTiles = new SortedSet<int>();
        whiteTiles = new SortedSet<int>();
    }

    public enum Side
    { 
        White,
        Black
    }

    private byte playerCount;
    Dictionary<byte, int> blackPlayers;
    Dictionary<byte, int> whitePlayers;

    SortedSet<int> blackTiles;
    SortedSet<int> whiteTiles;
    public byte GetNewPlayerId(Side side, (int, int) index)
    {
        int tilePos = One(index);
        if (side == Side.White)
        {
            whitePlayers.Add(playerCount, tilePos);
            whiteTiles.Add(tilePos);
        }
        else 
        {
            blackPlayers.Add(playerCount, tilePos);
            blackTiles.Add(tilePos);
        }
        return playerCount++;
    }
    public void TilePlaced(bool isWhite, (int, int) index)
    {
        int tile = One(index);
        (isWhite ? whiteTiles : blackTiles).Add(tile);
    }
    public bool CheckMove((int, int) index)
    {
        if (!IsIndexValid(index))
        {
            return false;
        }
        int tile = One(index);
        if (blackTiles.Contains(tile) || whiteTiles.Contains(tile))
        {
            return false;
        }
        return true;
    }
    public void ProcessPlayerMove(byte playerId, (int, int) index) // should checkMove first
    {
        int tilePos = One(index);
        if (whitePlayers.TryGetValue(playerId, out int whitePos))
        {

            whitePlayers[playerId] = tilePos;
            whiteTiles.Add(tilePos);
            whiteTiles.Remove(whitePos);
            return;
        }
        if (blackPlayers.TryGetValue(playerId, out int blackPos))
        {
            blackPlayers[playerId] = tilePos;
            blackTiles.Add(tilePos);
            blackTiles.Remove(blackPos);
            return;
        }
        Debug.Log("PlayerId was not found");
    }
    private int One((int, int) index)
    {
        var (row, col) = index;
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
}
