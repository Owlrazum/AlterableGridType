using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public abstract class GridController : MonoBehaviour
{
    public static List<List<Tile>> tiles;
    protected static int numOfRows;
    protected static int numOfColumns;

    protected static Stack<Move> moves;
    protected static bool isTileSelected;
    protected static bool isTileGroupSelected;

    protected static Stack<Tile> selectedTiles;
    protected static Stack<Tile> placedStack;
    protected static List<Tile> placedList;

    protected static List<List<(int, int)>> indexesOfPossibleMovesGroup;

    //protected static bool isFirstPlaced;

    public enum TileCheckResult
    {
        P1,
        P2, 
        None
    }
    protected TileCheckResult tileCheckResult; // modified through the event OnTileCheckValid

    private void Awake()
    {
        //isFirstPlaced = false;
        tileCheckResult = TileCheckResult.None;
    }
    private void Start()
    {
        PlayerInput.Instance.OnSelectGroupButtonDown += HandleGroupSelection;
        EventSystem.Instance.OnTileCheckResult += GetTileCheckResult;
        ProcessStart();
    }

    private void GetTileCheckResult(TileCheckResult result)
    {
        tileCheckResult = result;
    }


    protected virtual void ProcessStart()
    { 
    
    }

    private void OnDisable()
    {
        if (PlayerInput.Instance != null)
        { 
            PlayerInput.Instance.OnClick -= HandleClickEvent;
        }
    }
    private void OnEnable()
    {
        if (PlayerInput.Instance != null)
        {
            PlayerInput.Instance.OnClick += HandleClickEvent;
        }
    }

    protected const int layerMaskTiles = 1 << 8;
    /*protected virtual void HandleFirstClickEvent(Vector3 mousePos)
    {
        Debug.Log("Click");
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        int layerMask = 1 << 8;
        RaycastHit rayHit;

        Tile clickedTile;
        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
        {
            clickedTile = rayHit.collider.GetComponentInParent<Tile>();
            if (clickedTile == null)
            {
                return;
            }
            clickedTile.ItsState = Tile.State.Placed;
            AddPlacedTile(clickedTile);
            PlayerInput.Instance.OnClick -= HandleFirstClickEvent;
            PlayerInput.Instance.OnClick -= HandleClickEvent;
            PlayerInput.Instance.OnClick += HandleClickEvent;
            isFirstPlaced = true;
        }
    }*/


    protected virtual void HandleClickEvent(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit rayHit;
        Tile clickedTile;
        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMaskTiles, QueryTriggerInteraction.Collide))
        {
            clickedTile = rayHit.collider.GetComponentInParent<Tile>();
            EventSystem.Instance.CheckTileValidity(clickedTile);
            if (clickedTile.ItsState == Tile.State.Selected)
            {
                Unselect();
            }
            if (tileCheckResult == TileCheckResult.None)
            {
                if (clickedTile.ItsState == Tile.State.Highlight)
                {
                    if (tileCheckResult == TileCheckResult.P1)
                    {
                        EventSystem.Instance.TilePlaced(PlayersSystem.Side.White, clickedTile);
                        clickedTile.ItsState = Tile.State.PlacedFirst;
                    }
                    else
                    {
                        EventSystem.Instance.TilePlaced(PlayersSystem.Side.Black, clickedTile);
                        clickedTile.ItsState = Tile.State.PlacedSecond;
                    }
                    ProcessTilePlacement(clickedTile);

                }
                Unselect();
            }
            else if (tileCheckResult == TileCheckResult.P1)
            {
                if (clickedTile.ItsState == Tile.State.PlacedFirst && !isTileSelected)
                {
                    clickedTile.ItsState = Tile.State.Selected;
                    ProcessTileSelection(clickedTile);
                }
                else 
                {
                    Unselect();
                }
            }
            else 
            {
                if (clickedTile.ItsState == Tile.State.PlacedSecond && !isTileSelected)
                {
                    clickedTile.ItsState = Tile.State.Selected;
                    ProcessTileSelection(clickedTile);
                }
                else
                {
                    Unselect();
                }
            }
        }
        else
        {
            Unselect();
        }
    }

    protected virtual void ProcessTileSelection(Tile clickedTile)
    {
        int r = clickedTile.Row;
        int c = clickedTile.Column;
        Tile.OrientType orient = clickedTile.Orientation;

        isTileSelected = true;

        bool rowPlus = r < numOfRows - 1;
        bool rowMinus = r > 0;
        bool colPlus = c < numOfColumns - 1;
        bool colMinus = c > 0;
        bool colPlusTwo = c < numOfColumns - 2;
        bool colMinusTwo = c > 1;

        if (rowPlus)
        {
            if (tiles[r + 1][c].ItsState == Tile.State.Default)
                tiles[r + 1][c].ItsState = Tile.State.Highlight;
        }
        if (rowMinus)
        {
            if (tiles[r - 1][c].ItsState == Tile.State.Default)
                tiles[r - 1][c].ItsState = Tile.State.Highlight;
        }
        if (colPlus)
        {
            if (tiles[r][c + 1].ItsState == Tile.State.Default)
                tiles[r][c + 1].ItsState = Tile.State.Highlight;
        }
        if (colMinus)
        {
            if (tiles[r][c - 1].ItsState == Tile.State.Default)
                tiles[r][c - 1].ItsState = Tile.State.Highlight;
        }
        if (rowMinus && colPlusTwo)
        {
            if (tiles[r - 1][c + 2].ItsState == Tile.State.Default)
                tiles[r - 1][c + 2].ItsState = Tile.State.Highlight;
        }
        if (rowMinus && colMinusTwo)
        {
            if (tiles[r - 1][c - 2].ItsState == Tile.State.Default)
                tiles[r - 1][c - 2].ItsState = Tile.State.Highlight;
        }
        if (rowPlus && colPlusTwo)
        {
            if (tiles[r + 1][c + 2].ItsState == Tile.State.Default)
                tiles[r + 1][c + 2].ItsState = Tile.State.Highlight;
        }
        if (rowPlus && colMinusTwo)
        {
            if (tiles[r + 1][c - 2].ItsState == Tile.State.Default)
                tiles[r + 1][c - 2].ItsState = Tile.State.Highlight;
        }
    }
    protected virtual void ProcessGroupSelection()
    { 
    
    }
    protected virtual void ProcessTilePlacement(Tile clickedTile)
    {
        AddPlacedTile(clickedTile);
    }
    protected virtual void ProcessGroupPlacement(Tile clickedTile)
    { 
    
    }
    private void Unselect()
    {
        isTileSelected = false;
        isTileGroupSelected = false;
        selectedTiles.Clear();

        for (int i = 0; i < 8; i++)
        {
            indexesOfPossibleMovesGroup[i].Clear();
            indexesOfPossibleMovesGroup[i].Add((-1, -1));
        }

        EventSystem.Instance.TileDeselected();
    }
    protected void AddPlacedTile(Tile tile)
    {
        placedList.Add(tile);
        placedStack.Push(tile);
    }
    private void HandleGroupSelection()
    {
        if (isTileGroupSelected)
        {
            Unselect();
            isTileGroupSelected = false;
        }
        else
        {
            if (isTileSelected)
            {
                Unselect();
            }
            ProcessGroupSelection();
        }
    }
}
