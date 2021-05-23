using System;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance;
    private void Awake()
    {
        Instance = this;
        gridGenerator = false;
        gridController = false;
    }

    #region TileMode
    #region GridGeneration
    bool gridGenerator;
    bool gridController;
    bool gridSwapper;
    public void GridControllerReady()
    {
        gridController = true;
        if (gridGenerator && gridSwapper)
        {
            ReadyGenerateGrid();
        }
    }
    public void GridGeneratorReady()
    {
        gridGenerator = true;
        if (gridController && gridSwapper)
        {
            ReadyGenerateGrid();
        }
    }
    public void GridSwapperReady()
    {
        gridSwapper = true;
        if (gridController && gridGenerator)
        {
            ReadyGenerateGrid();
        }
    }
    public event Action OnReadyGenerateGrid;
    public void ReadyGenerateGrid()
    {
        OnReadyGenerateGrid?.Invoke();
    }

    public event Action<int, int, Tile.OrientType> OnTileCreated;
    public void TileCreated(int row, int column, Tile.OrientType orientaion)
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
    public event Action<int, int, Tile.OrientType> OnTileSelected;
    public void TileSelected(int row, int column, Tile.OrientType orient)
    {
        OnTileSelected?.Invoke(row, column, orient);
    }

    public event Action OnAllTilesSelected;
    public void AllTilesSelected()
    {
        OnAllTilesSelected?.Invoke();
    }

    public event Action<Tile> OnCheckTileValid;
    public void CheckTileValidity(Tile clickedTile)
    {
        OnCheckTileValid?.Invoke(clickedTile);
    }

    public event Action<GridController.TileCheckResult> OnTileCheckResult;
    public void SendTileCheckResult(GridController.TileCheckResult result)
    {
        OnTileCheckResult?.Invoke(result);
    }

    public event Action<PlayersSystem.Side, Tile> OnTilePlaced;
    public void TilePlaced(PlayersSystem.Side side, Tile placedTile)
    {
        OnTilePlaced?.Invoke(side, placedTile);
    }

    public event Action OnTileDeselected;
    public void TileDeselected()
    {
        OnTileDeselected?.Invoke();
    }
    #endregion

    #region GridSwapper


    #endregion
    public event Action OnSpectatorOn;
    public void SpectatorOn()
    {
        OnSpectatorOn?.Invoke();
    }

    public event Action OnSpectatorOff;
    public void SpectatorOff()
    {
        OnSpectatorOff?.Invoke();
    }
    public event Action<GridSwapper.GridType> OnGridSwap;
    public void GridSwapped(GridSwapper.GridType type)
    {
        OnGridSwap?.Invoke(type);
    }
    #endregion
}
