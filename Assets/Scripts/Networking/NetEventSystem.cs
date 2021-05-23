using System;
using System.Collections.Generic;
using UnityEngine;

public class NetEventSystem : MonoBehaviour
{
    public static NetEventSystem Instance;

    private void Awake()
    {
        Instance = this;
    }


    public Action OnResetSingle;
    public void ResetSingle()
    {
        OnResetSingle?.Invoke();
    }
    public Action<byte, byte> OnPassPlayerId;
    public void PassPlayerId(byte colorIndex, byte id)
    {
        OnPassPlayerId?.Invoke(colorIndex, id);
    }

    public Action<byte, (int, int)> OnMovePlayer;
    public void MovePlayer(byte id, (int, int) pos)
    {
        OnMovePlayer?.Invoke(id, pos);
    }

    public Action OnGameStarted;
    public void GameStarted()
    {
        OnGameStarted?.Invoke();
    }

    #region NetTileMode
    #region GridGeneration

    public event Action<int, int, NetTile.OrientType> OnTileCreated;
    public void TileCreated(int row, int column, NetTile.OrientType orientaion)
    {
        OnTileCreated?.Invoke(row, column, orientaion);
    }

    public event Action OnTilesInitialized;
    public void TilesInitialized()
    {
        OnTilesInitialized?.Invoke();
    }
    #endregion

    #region GridManipulation

    public event Action OnTileDeselected;
    public void TileDeselected()
    {
        OnTileDeselected?.Invoke();
    }
    #endregion

    #region NetGridSwapper


    #endregion
    public event Action OnSpectatorOn;
    public void SpectatorOn()
    {
        OnSpectatorOn?.Invoke();
    }

    public event Action OnRequestSpectatorOff;
    public void RequestSpectatorOff()
    {
        OnRequestSpectatorOff?.Invoke();
    }

    public event Action OnSpectatorOff;
    public void SpectatorOff()
    {
        OnSpectatorOff?.Invoke();
    }
    public event Action<NetGridSwapper.GridType, NetGridSwapper.GridType> OnGridSwapped;
    public void GridSwapped(NetGridSwapper.GridType oldValue, NetGridSwapper.GridType newValue)
    {
        OnGridSwapped?.Invoke(oldValue, newValue);
    }
    public event Action<NetGridSwapper.GridType> OnSetGridType;
    public void SetGridType(NetGridSwapper.GridType newValue)
    {
        OnSetGridType?.Invoke(newValue);
    }
    public event Action OnSwitchGridType;
    public void SwitchGridType()
    {
        OnSwitchGridType?.Invoke();
    }
    public event Action OnMakeNewPlayer;
    public void MakeNewPlayer()
    {
        OnMakeNewPlayer?.Invoke();
    }

    public event Action<byte> OnReduceTrailIntensity;
    public void ReduceTrailIntensity(byte colorIndex)
    {
        OnReduceTrailIntensity?.Invoke(colorIndex);
    }
    #endregion
}
