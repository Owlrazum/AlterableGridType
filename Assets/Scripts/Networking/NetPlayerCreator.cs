using MLAPI;
using MLAPI.Messaging;
using MLAPI.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.NetworkVariable;

public class NetPlayerCreator : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private NetPs netPs;
    private SortedSet<(int, int)> placedTiles = new SortedSet<(int, int)>();
    public void NewPlayer(byte colorIndex, ulong owner)
    {
        int rnd1, rnd2;
        do
        {
            rnd1 = Random.Range(0, NetGrid.Instance.tiles.Count);
            rnd2 = Random.Range(0, NetGrid.Instance.tiles[0].Count);
        } while (placedTiles.Contains((rnd1, rnd2)));
        placedTiles.Add((rnd1, rnd2));
        NewPlayerServerRpc(colorIndex, rnd1, rnd2, owner);
    }
    [ServerRpc(RequireOwnership = false)] 
    public void NewPlayerServerRpc(byte colorIndex, int row, int col, ulong owner)
    {
        NewPlayerClientRpc(colorIndex, row, col, owner);
    }
    [ClientRpc]
    public void NewPlayerClientRpc(byte colorIndex, int row, int col, ulong owner)
    {
        NetPlayer player = Instantiate(playerPrefab).GetComponent<NetPlayer>();
        GameObject NetPsGb = GameObject.FindWithTag("NetPs");
        NetPs ps = NetPsGb.GetComponent<NetPs>();
        if (NetworkManager.LocalClientId == owner)
        {
            player.IsSelected = true;
        }
        player.Init(ps, colorIndex, (row, col));
    }
}
