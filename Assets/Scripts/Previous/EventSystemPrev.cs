using System;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemPrev : MonoBehaviour
{
    public static EventSystemPrev Instance;
    private void Awake()
    {
        Instance = this;
    }

    public event Action<List<Vector3>> OnPositionsReady;
    public void PositionsReady(List<Vector3> positions)
    {
        PlayerInputPrev.Instance.TheDisplayPlaceStarted();
        OnPositionsReady?.Invoke(positions);
    }
    #region GridGeneration
    /*
    bool gridGenerator;
    bool gridController;
    public void GridControllerReady()
    {
        gridController = true;
        if (gridGenerator)
        {
            ReadyGenerateGrid();
        }
    }
    public void GridGeneratorReady()
    {
        gridGenerator = true;
        if (gridController)
        {
            ReadyGenerateGrid();
        }
    }
    public event Action OnReadyGenerateGrid;
    public void ReadyGenerateGrid()
    {
        OnReadyGenerateGrid?.Invoke();
    }

    public event Action<List<List<Tile>>, List<List<RefLine>>, (int, int)> OnSendedContainers;
    public void SendContainers(List<List<Tile>> tiles, List<List<RefLine>> reflines, (int, int) dimensions)
    {
        OnSendedContainers?.Invoke(tiles, reflines, dimensions);
    }

    public event Action<int, int, Tile.OrientType> OnTileCreated;
    public void TileCreated(int row, int column, Tile.OrientType orientaion)
    {
        OnTileCreated?.Invoke(row, column, orientaion);
    }
    */
    #endregion
    /*
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

    public event Action OnTileDeselected;
    public void TileDeselected()
    {
        OnTileDeselected?.Invoke();
    }
    #endregion

    public event Action OnTilesBehindScreen;

    public void TileBehindScreen() 
    {
        OnTilesBehindScreen?.Invoke();
    }
    */

}
