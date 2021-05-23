using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public class TriangularGrid : GridController
{
    private enum HighlightMode
    {
        HorizVerticals,
        Diagonals
    }

    private HighlightMode highMode;

/*    private Stack<Move> moves;
    private bool isTileSelected;
    private bool isTileGroupSelected;

    private Stack<Tile> selectedTiles;
    private Stack<Tile> placedStack;
    private List<Tile> placedList;*/
    private Stack<Tile> deletedTiles;

    private List<RefLine> horizRefLines;
    private List<int> horizTilesAssigned;
    private List<RefLine> topLeftRefLines;
    private List<int> topLeftTilesAssigned;
    private List<RefLine> botLeftRefLines;
    private List<int> botLeftTilesAssigned;
    private List<int> vertTilesAssigned;

/*    private List<List<Tile>> tiles;
    private int numOfRows;
    private int numOfColumns;*/

    private (int, int) currentHorizs;
    private (int, int) currentVerts;
    private (int, int) currentTopLefts;
    private (int, int) currentBotLefts;

    private List<Tile> currentAffectedTiles;

    private BitArray currentRefLinesExistence;

    //private List<List<(int, int)>> indexesOfPossibleMovesGroup;

    // Start is called before the first frame update
    protected override void ProcessStart()
    {
        highMode = HighlightMode.HorizVerticals;

        DataSender.Instance.OnSendedContainers += InitializeContainers;
        EventSystem.Instance.GridControllerReady();
    }

    public void DisableReflines()
    {
        foreach (RefLine r in horizRefLines)
        {
            r.gameObject.SetActive(false);
        }

        foreach (RefLine r in topLeftRefLines)
        {
            r.gameObject.SetActive(false);
        }

        foreach (RefLine r in botLeftRefLines)
        {
            r.gameObject.SetActive(false);
        }
    }

    public void EnableReflines()
    {
        foreach (RefLine r in horizRefLines)
        {
            r.gameObject.SetActive(true);
        }

        foreach (RefLine r in topLeftRefLines)
        {
            r.gameObject.SetActive(true);
        }

        foreach (RefLine r in botLeftRefLines)
        {
            r.gameObject.SetActive(true);
        }
    }
    private void InitializeContainers(List<List<Tile>> genTiles, List<List<RefLine>> genRefLines, (int, int) dimensions)
    {
        tiles = genTiles;

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
                tiles[i][j].DisableHexPart();
            }
        }

        EventSystem.Instance.TilesInitialized();
        numOfRows = dimensions.Item1;
        numOfColumns = dimensions.Item2;

        horizRefLines = genRefLines[0];
        topLeftRefLines = genRefLines[1]; ;
        botLeftRefLines = genRefLines[2]; ;

        horizTilesAssigned = new List<int>();
        topLeftTilesAssigned = new List<int>();
        botLeftTilesAssigned = new List<int>();
        vertTilesAssigned = new List<int>();
        for (int i = 0; i < horizRefLines.Count; i++)
        {
            horizTilesAssigned.Add(0);
        }
        for (int i = 0; i < topLeftRefLines.Count; i++)
        {
            topLeftTilesAssigned.Add(0);
            botLeftTilesAssigned.Add(0);
        }
        for (int i = 0; i < numOfColumns - 1; i++)
        {
            vertTilesAssigned.Add(0);
        }


        selectedTiles = new Stack<Tile>();
        placedList = new List<Tile>();
        placedStack = new Stack<Tile>();
        deletedTiles = new Stack<Tile>();

        currentAffectedTiles = new List<Tile>();

        indexesOfPossibleMovesGroup = new List<List<(int, int)>>();
        for (int i = 0; i < 8; i++)
        {
            indexesOfPossibleMovesGroup.Add(new List<(int, int)>()); // hor1 hor2, ver1, ver2, topL1, topL2, botL1, botL2
            indexesOfPossibleMovesGroup[i].Add((-1, -1));
            // if list contains at least one element, then such group is possible, otherwise no addition to this list would be made
        }

        moves = new Stack<Move>();
    }
  
    #region Handles
    /*protected override void HandleFirstClickEvent(Vector3 mousePos)
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
            PlayerInput.Instance.OnClick += HandleClickEvent;

            bool isRefLineExists;
            bool[] areReflinesExist = new bool[6];

            currentHorizs.Item1 = clickedTile.HorizRefLines.Item1;
            isRefLineExists = currentHorizs.Item1 >= 0 && currentHorizs.Item1 < horizRefLines.Count;
            areReflinesExist[0] = isRefLineExists;
            currentHorizs.Item2 = clickedTile.HorizRefLines.Item2;
            isRefLineExists = currentHorizs.Item2 >= 0 && currentHorizs.Item2 < horizRefLines.Count;
            areReflinesExist[1] = isRefLineExists;

            currentTopLefts.Item1 = clickedTile.TopLeftRefLines.Item1;
            isRefLineExists = currentTopLefts.Item1 >= 0 && currentTopLefts.Item1 < topLeftRefLines.Count;
            areReflinesExist[2] = isRefLineExists;
            currentTopLefts.Item2 = clickedTile.TopLeftRefLines.Item2;
            isRefLineExists = currentTopLefts.Item2 >= 0 && currentTopLefts.Item2 < topLeftRefLines.Count;
            areReflinesExist[3] = isRefLineExists;

            currentBotLefts.Item1 = clickedTile.BotLeftRefLines.Item1;
            isRefLineExists = currentBotLefts.Item1 >= 0 && currentBotLefts.Item1 < botLeftRefLines.Count;
            areReflinesExist[4] = isRefLineExists;
            currentBotLefts.Item2 = clickedTile.BotLeftRefLines.Item2;
            isRefLineExists = currentBotLefts.Item1 >= 0 && currentBotLefts.Item1 < botLeftRefLines.Count;
            areReflinesExist[5] = isRefLineExists;

            currentVerts.Item1 = clickedTile.VertRefLines.Item1;
            currentVerts.Item2 = clickedTile.VertRefLines.Item2;

            currentRefLinesExistence = new BitArray(areReflinesExist);
            bool[] areReflinesModified = new bool[8];
            for (int i = 0; i < 6; i++)
            {
                areReflinesModified[i] = areReflinesExist[i];
            }
            areReflinesModified[6] = currentVerts.Item1 >= 0 ? true : false;
            areReflinesModified[7] = currentVerts.Item2 >= 0 ? true : false;
            // AssignPlacedTileToReflines(areReflinesModified);
            DebugCurrentReflines(true);
            currentAffectedTiles.Add(clickedTile);
            RecordMove(Move.Type.TilePlacement);
            //isFirstPlaced = true;
        }
    }*/
    protected override void HandleClickEvent(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit rayHit;
        Tile clickedTile;
        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMaskTiles, QueryTriggerInteraction.Collide))
        {
            clickedTile = rayHit.collider.GetComponentInParent<Tile>();
            if (clickedTile.ItsState == Tile.State.Selected)
            {
                Unselect();
                return;
            }
            EventSystem.Instance.CheckTileValidity(clickedTile);
            if (tileCheckResult == TileCheckResult.None)
            {
                if (clickedTile.ItsState == Tile.State.Highlight)
                {
                    ProcessTilePlacement(clickedTile);
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
                    Unselect();
                }
                else if (clickedTile.ItsState == Tile.State.HighlightGroup)
                {
                    ProcessGroupPlacement(clickedTile);
                    Unselect();
                }
            }
            else if (tileCheckResult == TileCheckResult.P1)
            {
                if (clickedTile.ItsState == Tile.State.PlacedFirst && !isTileSelected)
                {
                    clickedTile.ItsState = Tile.State.Selected;
                    ProcessTileSelection(clickedTile);
                }
            }
            else
            {
                if (clickedTile.ItsState == Tile.State.PlacedSecond && !isTileSelected)
                {
                    clickedTile.ItsState = Tile.State.Selected;
                    ProcessTileSelection(clickedTile);
                }
            }
            
        }
        else
        {
            Unselect();
        }
    }

    private void HandleHighlightModeChange()
    {
        highMode = highMode == HighlightMode.HorizVerticals ?
            HighlightMode.Diagonals : HighlightMode.HorizVerticals;
        Unselect();
        ProcessGroupSelection();
    }
    private void HandleDelete()
    {
        if (isTileSelected)
        {
            Tile deletedTile = selectedTiles.Pop();
            deletedTile.ItsState = Tile.State.Default;
            deletedTiles.Push(deletedTile);
           // ProcessTileDeletion(deletedTile);
            RecordMove(Move.Type.Deletion);
            Unselect();
        }
        else if (isTileGroupSelected)
        {
            while (selectedTiles.Count != 0)
            {
                Tile deletedTile = selectedTiles.Pop();
                deletedTile.ItsState = Tile.State.Default;
                deletedTiles.Push(deletedTile);
            }
            RecordMove(Move.Type.Deletion);
        }
        else
        {
            return;
        }
    }
    #endregion

    #region Selection
    protected override void ProcessTileSelection(Tile clickedTile)
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
        if (orient == Tile.OrientType.Up)
        {
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
        }
        else
        {
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
        selectedTiles.Push(tiles[r][c]);
    }
    protected override void ProcessGroupSelection()
    {
        isTileGroupSelected = true;
        for (int i = 0; i < placedList.Count; i++)
        {
            placedList[i].ItsState = Tile.State.Selected;
            selectedTiles.Push(placedList[i]);
        }
        if (highMode == HighlightMode.HorizVerticals)
        {
            ProcessHorVerSelectionGroup();
        }
        else
        {
            ProcessDiagonalSelectionGroup();
        }
    }
    private void ProcessHorVerSelectionGroup()
    {
        for (int i = 0; i < placedList.Count; i++)
        {
            Tile tile = placedList[i];
            bool isUp = tile.Orientation == Tile.OrientType.Up ? true : false;

            int refLine = tile.HorizRefLines.Item1;
            int delta;

            int rowHigh = tile.Row;
            int colHigh = tile.Column;

            int group = 0;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                delta = currentHorizs.Item1 - refLine;
                rowHigh += delta * 2 - 1;

                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                refLine = tile.HorizRefLines.Item2;
                rowHigh = tile.Row;

                delta = currentHorizs.Item2 - refLine;
                rowHigh += delta * 2 + 1;
                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            // verts
            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                rowHigh = tile.Row;
                colHigh = tile.Column;

                delta = currentVerts.Item1 - colHigh - 1;
                colHigh += delta * 2 + 2;
                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }

            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                rowHigh = tile.Row;
                colHigh = tile.Column;

                delta = currentVerts.Item2 - colHigh - 1;
                colHigh += delta * 2 + 4;
                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
        }
    }
    private void ProcessDiagonalSelectionGroup()
    {
        for (int i = 0; i < placedList.Count; i++)
        {
            Tile tile = placedList[i];
            bool isUp = tile.Orientation == Tile.OrientType.Up ? true : false;

            int rowHigh = tile.Row;
            int colHigh = tile.Column;

            #region FirstTopLeft
            //---------------------------------------------------------------------------------------------------

            int refLine = tile.TopLeftRefLines.Item1;
            int delta = refLine - currentTopLefts.Item1;

            int rowModUp = -1;
            int rowModDown = 0;
            int colModUp = -2;
            int colModDown = -1;
            if (isUp)
            {
                rowHigh += rowModUp * (delta + 1) + rowModDown * delta;
                colHigh += colModUp * (delta + 1) + colModDown * delta;
            }
            else
            {
                rowHigh += rowModDown * (delta + 1) + rowModUp * delta;
                colHigh += colModDown * (delta + 1) + colModUp * delta;
            }
            int group = 4;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

            #region SecondTopLeft
            //---------------------------------------------------------------------------------------------------
            rowHigh = tile.Row;
            colHigh = tile.Column;
            refLine = tile.TopLeftRefLines.Item2;

            delta = currentTopLefts.Item2 - refLine;

            rowModUp = 0;
            rowModDown = 1;
            colModUp = 1;
            colModDown = 2;
            if (isUp)
            {
                rowHigh += rowModUp * (delta + 1) + rowModDown * delta;
                colHigh += colModUp * (delta + 1) + colModDown * delta;
            }
            else
            {
                rowHigh += rowModDown * (delta + 1) + rowModUp * delta;
                colHigh += colModDown * (delta + 1) + colModUp * delta;
            }
            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            #endregion

            #region FirstBotLeft
            //---------------------------------------------------------------------------------------------------
            rowHigh = tile.Row;
            colHigh = tile.Column;
            refLine = tile.BotLeftRefLines.Item1;

            delta = refLine - currentBotLefts.Item1;

            rowModUp = 0;
            rowModDown = 1;
            colModUp = -1;
            colModDown = -2;
            if (isUp)
            {
                rowHigh += rowModUp * (delta + 1) + rowModDown * delta;
                colHigh += colModUp * (delta + 1) + colModDown * delta;
            }
            else
            {
                rowHigh += rowModDown * (delta + 1) + rowModUp * delta;
                colHigh += colModDown * (delta + 1) + colModUp * delta;
            }
            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

            #region SecondBotLeft
            //---------------------------------------------------------------------------------------------------
            rowHigh = tile.Row;
            colHigh = tile.Column;
            refLine = tile.BotLeftRefLines.Item2;

            delta = currentBotLefts.Item2 - refLine;

            rowModUp = -1;
            rowModDown = 0;
            colModUp = 2;
            colModDown = 1;
            if (isUp)
            {
                rowHigh += rowModUp * (delta + 1) + rowModDown * delta;
                colHigh += colModUp * (delta + 1) + colModDown * delta;
            }
            else
            {
                rowHigh += rowModDown * (delta + 1) + rowModUp * delta;
                colHigh += colModDown * (delta + 1) + colModUp * delta;
            }
            group++;
            if (indexesOfPossibleMovesGroup[group].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = group;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[group].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(group);
                }
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

        }
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
    #endregion

    #region GridManipulation
    private void HighlightTileFromGroup(int r, int c)
    {
        if (tiles[r][c].ItsState == Tile.State.Default)
        {
            tiles[r][c].ItsState = Tile.State.HighlightGroup;
        }
    }
    private void DisableHighlightGroup(int index)
    {
        for (int i = 1; i < indexesOfPossibleMovesGroup[index].Count - 1; i++)
        {
            (int, int) tileIndex = indexesOfPossibleMovesGroup[index][i];
            if ((tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.PlacedFirst
                ||
                tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.PlacedSecond)
                &&
                tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.Selected
                )
            {
                tiles[tileIndex.Item1][tileIndex.Item2].ItsState = Tile.State.Default;
            }
        }
        indexesOfPossibleMovesGroup[index].Clear();
        DebugCurrentReflines(false);
        if (index < 6)
        {
            currentRefLinesExistence[index] = false;
        }
        DebugCurrentReflines(true);
    }
    protected override void ProcessTilePlacement(Tile tile)
    {
        bool[] areReflinesModified = new bool[8];
        int index = 0;

        DebugCurrentReflines(false);
        int smaller = tile.HorizRefLines.Item1;
        int bigger = tile.HorizRefLines.Item2;

        int curRefLine = currentHorizs.Item1;
        #region Horizs
        if (bigger <= curRefLine)
        {
            if (smaller >= 0 && smaller < horizRefLines.Count)
            {
                currentHorizs.Item1 = smaller;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        curRefLine = currentHorizs.Item2;
        if (smaller >= curRefLine)
        {
            if (bigger < horizRefLines.Count && bigger >= 0)
            {
                currentHorizs.Item2 = bigger;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        #endregion

        #region Verts
        smaller = tile.VertRefLines.Item1;
        bigger = tile.VertRefLines.Item2;
        curRefLine = currentVerts.Item1;
        if (bigger <= curRefLine)
        {
            if (smaller >= 0 && smaller < numOfColumns - 1)
            {
                currentVerts.Item1 = smaller;
                areReflinesModified[index] = true;
            }
        }
        index++;
        curRefLine = currentVerts.Item2;
        if (smaller >= curRefLine)
        {
            if (bigger < numOfColumns - 1 && bigger >= 0)
            {
                currentVerts.Item2 = bigger;
                areReflinesModified[index] = true;
            }
        }
        index++;
        #endregion

        #region TopLefts
        smaller = tile.TopLeftRefLines.Item1;
        bigger = tile.TopLeftRefLines.Item2;
        curRefLine = currentTopLefts.Item1;
        if (bigger <= curRefLine)
        {
            if (smaller >= 0 && smaller < topLeftRefLines.Count)
            {
                currentTopLefts.Item1 = smaller;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        curRefLine = currentTopLefts.Item2;
        if (smaller >= curRefLine)
        {
            if (bigger < topLeftRefLines.Count && bigger >= 0)
            {
                currentTopLefts.Item2 = bigger;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        #endregion

        #region botLefts
        smaller = tile.BotLeftRefLines.Item1;
        bigger = tile.BotLeftRefLines.Item2;
        curRefLine = currentBotLefts.Item1;
        if (bigger <= curRefLine)
        {
            if (smaller >= 0 && smaller < botLeftRefLines.Count)
            {
                currentBotLefts.Item1 = smaller;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        curRefLine = currentBotLefts.Item2;
        if (smaller >= curRefLine)
        {
            if (bigger < botLeftRefLines.Count && bigger >= 0)
            {
                currentBotLefts.Item2 = bigger;
                areReflinesModified[index] = true;
            }
            else currentRefLinesExistence[index] = false;
        }
        index++;
        #endregion
        AddPlacedTile(tile);
        currentAffectedTiles.Add(tile);
        //AssignPlacedTileToReflines(areReflinesModified);
        DebugCurrentReflines(true);

    }

    private int placedHighlightGroupCount = 0;
    protected override void ProcessGroupPlacement(Tile clickedTile)
    {
        int highlightGroup = clickedTile.HighlightGroup;
        bool isFinal = true;
        placedHighlightGroupCount++;
        List<(int, int)> indexesOfPlacement = indexesOfPossibleMovesGroup[highlightGroup];
        for (int i = 1; i < indexesOfPlacement.Count; i++)
        {
            int rowIndex = indexesOfPlacement[i].Item1;
            int colIndex = indexesOfPlacement[i].Item2;
            //tiles[rowIndex][colIndex].ItsState = Tile.State.Placed;
            ProcessTilePlacement(tiles[rowIndex][colIndex]);
        }
        RecordMove(Move.Type.GroupPlacement);
        if (isFinal || placedHighlightGroupCount >= 7)
        {
            for (int i = 0; i < 8; i++)
            {
                indexesOfPossibleMovesGroup[i].Clear();
                indexesOfPossibleMovesGroup[i].Add((-1, -1));
            }
        }


    }
/*    private void ProcessTileDeletion(Tile tile)
    {
        DebugCurrentReflines(false);

        int smaller = tile.HorizRefLines.Item1;
        int bigger = tile.HorizRefLines.Item2;
        #region hor
        if (smaller == currentHorizs.Item1)
        {
            horizTilesAssigned[currentHorizs.Item1]--;
            if (horizTilesAssigned[currentHorizs.Item1] == 0)
            {
                int index = currentHorizs.Item1 + 1;
                while (index < currentHorizs.Item2)
                {
                    if (horizTilesAssigned[index] > 0)
                    {
                        currentHorizs.Item1 = index;
                    }
                    index++;
                }
            }
        }
        if (bigger == currentHorizs.Item2)
        {
            horizTilesAssigned[currentHorizs.Item2]--;
            if (horizTilesAssigned[currentHorizs.Item2] == 0)
            {
                int index = currentHorizs.Item2 - 1;
                while (index < currentHorizs.Item2)
                {
                    if (horizTilesAssigned[index] > 0)
                    {
                        currentHorizs.Item2 = index;
                    }
                    index--;
                }
            }
        }
        #endregion
        DebugCurrentReflines(true);

    }
    private void AssignPlacedTileToReflines(bool[] areReflinesModified)
    {
        if (areReflinesModified[0]) horizTilesAssigned[currentHorizs.Item1]++;
        if (areReflinesModified[1]) horizTilesAssigned[currentHorizs.Item2]++;
        if (areReflinesModified[2]) topLeftTilesAssigned[currentVerts.Item1]++;
        if (areReflinesModified[3]) topLeftTilesAssigned[currentVerts.Item2]++;
        if (areReflinesModified[4]) botLeftTilesAssigned[currentTopLefts.Item1]++;
        if (areReflinesModified[5]) botLeftTilesAssigned[currentTopLefts.Item2]++;
        if (areReflinesModified[6]) vertTilesAssigned[currentBotLefts.Item1]++;
        if (areReflinesModified[7]) vertTilesAssigned[currentBotLefts.Item2]++;
    }*/

    private void DebugCurrentReflines(bool color)
    {
        if (color)
        {
            if (currentRefLinesExistence[0]) horizRefLines[currentHorizs.Item1].IsSelected = true;
            if (currentRefLinesExistence[1]) horizRefLines[currentHorizs.Item2].IsSelected = true;

            if (currentRefLinesExistence[2]) topLeftRefLines[currentTopLefts.Item1].IsSelected = true;
            if (currentRefLinesExistence[3]) topLeftRefLines[currentTopLefts.Item2].IsSelected = true;

            if (currentRefLinesExistence[4]) botLeftRefLines[currentBotLefts.Item1].IsSelected = true;
            if (currentRefLinesExistence[5]) botLeftRefLines[currentBotLefts.Item2].IsSelected = true;
        }
        else
        {
            if (currentRefLinesExistence[0]) horizRefLines[currentHorizs.Item1].IsDefault = true;
            if (currentRefLinesExistence[1]) horizRefLines[currentHorizs.Item2].IsDefault = true;

            if (currentRefLinesExistence[2]) topLeftRefLines[currentTopLefts.Item1].IsDefault = true;
            if (currentRefLinesExistence[3]) topLeftRefLines[currentTopLefts.Item2].IsDefault = true;

            if (currentRefLinesExistence[4]) botLeftRefLines[currentBotLefts.Item1].IsDefault = true;
            if (currentRefLinesExistence[5]) botLeftRefLines[currentBotLefts.Item2].IsDefault = true;
        }

    }
    #endregion

    private void RemoveTile(RowCol rc)
    {

    }
    private void RemoveLastPlacedTile()
    {
        int index = placedList.Count - 1;
        placedList[index].ItsState = Tile.State.Default;
        placedList.RemoveAt(placedList.Count - 1);
        placedStack.Pop();
    }
    private void RecordMove(Move.Type type)
    {
        Move move = new Move(type);
        switch (type)
        {
            case Move.Type.TilePlacement:
                move.Horizs = currentHorizs;
                move.Verts = currentVerts;
                move.TopLefts = currentTopLefts;
                move.BotLefts = currentBotLefts;
                //moves.Push(move);
                move.AffectedTiles = currentAffectedTiles;
                MoveSystem.Instance.MoveCompleted(move);
                break;
            case Move.Type.GroupPlacement:
                goto case Move.Type.TilePlacement;
            case Move.Type.Deletion:

                break;
        }
        currentAffectedTiles.Clear();
    }

}

