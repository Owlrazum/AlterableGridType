using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StateOfTheGame
{
    Menu,
    Game,
    End
}

public class GameManager : MonoBehaviour
{

    #region UI
    [SerializeField]
    private GameObject Menu;

    [SerializeField]
    private GameObject WinPanel;
    [SerializeField]
    private TextMeshProUGUI WinMessage;
    [SerializeField]
    private AudioSource winSound;
    [SerializeField]
    private AudioSource gameSound;
    #endregion
    private StateOfTheGame state;
    private void Start()
    {
        state = StateOfTheGame.Game;
        //PlayerInput.Instance.OnCancelButtonDown += ProcessCanselButtonDown;
    }

    private void ProcessCanselButtonDown()
    {
        if (state == StateOfTheGame.Game)
        {
            state = StateOfTheGame.Menu;
            //Menu.SetActive(true);
            Application.Quit();
        }
        else
        {
            state = StateOfTheGame.Game;
            Menu.SetActive(false);
        }
    }

    private void EndGame(bool isWhitesWin)
    {
        state = StateOfTheGame.End;
        string message = isWhitesWin ? "WHITES WIN!" : "BLACKS WIN!";
        WinMessage.SetText(message);
        WinPanel.SetActive(true);
        winSound.Play();
        gameSound.Stop();
    }

}
    #region prevs
    /* 
    private void InitializeContainers(List<List<Tile>> genTiles, List<List<RefLine>> genRefLines, (int, int) dimensions)
    {
        tiles = genTiles;
        numOfRows = dimensions.Item1;
        numOfColumns = dimensions.Item2;

        horizRefLines = genRefLines[0];
        topLeftRefLines = genRefLines[1]; ;
        botLeftRefLines = genRefLines[2]; ;

        placedTiles = new List<Tile>();

        indexesOfPossibleMovesGroup = new List<List<(int, int)>>();
        for (int i = 0; i < 6; i++)
        {
            indexesOfPossibleMovesGroup.Add(new List<(int, int)>()); // hor1 hor2, topL1, topL2, botL1, botL2
            indexesOfPossibleMovesGroup[i].Add((-1, -1));
            // if list contains at least one element, then such group is possible, otherwise no addition to this list would be made
        }
    }
    #region ProcessClicks
    private void HandleFirstClickEvent(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        int layerMask = 1 << 8;
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide))
        {
            clickedTile = rayHit.collider.GetComponent<Tile>();
            clickedTile.ItsState = Tile.State.Placed;
            placedTiles.Add(clickedTile);
            PlayerInput.Instance.OnClick -= HandleFirstClickEvent;
            PlayerInput.Instance.OnClick += HandleClickEvent;


            bool isRefLineExists;
            bool[] areReflineExist = new bool[6];


            currentHorizs.Item1 = clickedTile.HorizRefLines.Item1;
            isRefLineExists = currentHorizs.Item1 >= 0 && currentHorizs.Item1 < horizRefLines.Count;
            areReflineExist[0] = isRefLineExists;
            currentHorizs.Item2 = clickedTile.HorizRefLines.Item2;
            isRefLineExists = currentHorizs.Item2 >= 0 && currentHorizs.Item2 < horizRefLines.Count;
            areReflineExist[1] = isRefLineExists;

            currentTopLefts.Item1 = clickedTile.TopLeftRefLines.Item1;
            isRefLineExists = currentTopLefts.Item1 >= 0 && currentTopLefts.Item1 < topLeftRefLines.Count;
            areReflineExist[2] = isRefLineExists;
            currentTopLefts.Item2 = clickedTile.TopLeftRefLines.Item2;
            isRefLineExists = currentTopLefts.Item2 >= 0 && currentTopLefts.Item2 < topLeftRefLines.Count;
            areReflineExist[3] = isRefLineExists;

            currentBotLefts.Item1 = clickedTile.BotLeftRefLines.Item1;
            isRefLineExists = currentBotLefts.Item1 >= 0 && currentBotLefts.Item1 < botLeftRefLines.Count;
            areReflineExist[4] = isRefLineExists;
            currentBotLefts.Item2 = clickedTile.BotLeftRefLines.Item2;
            isRefLineExists = currentBotLefts.Item1 >= 0 && currentBotLefts.Item1 < botLeftRefLines.Count;
            areReflineExist[5] = isRefLineExists;

            currentRefLinesExistence = new BitArray(areReflineExist);
            DebugCurrentReflines(true);
        }
    }
    int layerMaskTiles = 1 << 8;
    private void HandleClickEvent(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMaskTiles, QueryTriggerInteraction.Collide))
        {
            clickedTile = rayHit.collider.GetComponent<Tile>();
            if (clickedTile.ItsState == Tile.State.Placed && !isTileSelected)
            {
                clickedTile.ItsState = Tile.State.Selected;
                ProcessTileSelection(clickedTile.Row, clickedTile.Column, clickedTile.Orientation);
            }
            else if (clickedTile.ItsState == Tile.State.Highlight)
            {
                clickedTile.ItsState = Tile.State.Placed;
                ProcessTilePlacement(clickedTile);

                Unselect();
            }
            else if (clickedTile.ItsState == Tile.State.HighlightGroup)
            {
                PlaceHighlightGroup(clickedTile.HighlightGroup);
                Unselect();
            }
            else if (clickedTile.ItsState == Tile.State.Selected || clickedTile.ItsState == Tile.State.SelectedGroup)
            {
                Unselect();
            }
        }
    }
    #endregion
    private void ProcessTileSelection(int r, int c, Tile.OrientType orient)
    {
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
    }
    private void ProcessTilePlacement(Tile tile)
    {

        DebugCurrentReflines(false);
        int smaller = tile.HorizRefLines.Item1;
        int bigger = tile.HorizRefLines.Item2;

        int curRefLine = currentHorizs.Item1;
        #region Horizs
        if (bigger == curRefLine)
        {
            if (smaller >= 0 && smaller < horizRefLines.Count) currentHorizs.Item1 = smaller;
            else currentRefLinesExistence[0] = false;
        }
        curRefLine = currentHorizs.Item2;
        if (smaller == curRefLine)
        {
            if (bigger < horizRefLines.Count && bigger >= 0) currentHorizs.Item2 = bigger;
            else currentRefLinesExistence[1] = false;
        }
        #endregion

        #region TopLefts
        smaller = tile.TopLeftRefLines.Item1;
        bigger = tile.TopLeftRefLines.Item2;
        curRefLine = currentTopLefts.Item1;
        if (bigger == curRefLine)
        {
            if (smaller >= 0 && smaller < topLeftRefLines.Count) currentTopLefts.Item1 = smaller;
            else currentRefLinesExistence[2] = false;
        }
        curRefLine = currentTopLefts.Item2;
        if (smaller == curRefLine)
        {
            if (bigger < topLeftRefLines.Count && bigger >= 0) currentTopLefts.Item2 = bigger;
            else currentRefLinesExistence[3] = false;
        }
        #endregion

        #region botLefts
        smaller = tile.BotLeftRefLines.Item1;
        bigger = tile.BotLeftRefLines.Item2;
        curRefLine = currentBotLefts.Item1;
        if (bigger == curRefLine)
        {
            if (smaller >= 0 && smaller < botLeftRefLines.Count) currentBotLefts.Item1 = smaller;
            else currentRefLinesExistence[4] = false;
        }
        curRefLine = currentBotLefts.Item2;
        if (smaller == curRefLine)
        {
            if (bigger < botLeftRefLines.Count && bigger >= 0) currentBotLefts.Item2 = bigger;
            else currentRefLinesExistence[5] = false;
        }
        #endregion
        placedTiles.Add(tile);
        DebugCurrentReflines(true);
    }
    private void HandleGroupSelectionEvent()
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
            SelectAllPlacedTiles();
        }
    }
    private void SelectAllPlacedTiles()
    {
        isTileGroupSelected = true;
        for (int i = 0; i < placedTiles.Count; i++)
        {
            placedTiles[i].ItsState = Tile.State.Selected;
        }
        ProcessSelectionGroup();
    }
    private void ProcessSelectionGroup()
    {
        for (int i = 0; i < placedTiles.Count; i++)
        {
            Tile tile = placedTiles[i];
            bool isUp = tile.Orientation == Tile.OrientType.Up ? true : false;

            int refLine = tile.HorizRefLines.Item1;

            int rowHigh = tile.Row;
            int colHigh = tile.Column;
            int delta;

            if (indexesOfPossibleMovesGroup[0].Count != 0)
            {
                delta = currentHorizs.Item1 - refLine;
                rowHigh += delta * 2 - 1;

                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[0].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = 0;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(0);
                }
            }

            if (indexesOfPossibleMovesGroup[1].Count != 0)
            {
                refLine = tile.HorizRefLines.Item2;
                rowHigh = tile.Row;

                delta = currentHorizs.Item2 - refLine;
                rowHigh += delta * 2 + 1;
                if (!(rowHigh < 0 || colHigh < 0
                    || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    indexesOfPossibleMovesGroup[1].Add((rowHigh, colHigh));
                    tiles[rowHigh][colHigh].HighlightGroup = 1;
                    HighlightTileFromGroup(rowHigh, colHigh);
                }
                else
                {
                    DisableHighlightGroup(1);
                }
            }



            #region FirstTopLeft
            //---------------------------------------------------------------------------------------------------
            rowHigh = tile.Row;
            colHigh = tile.Column;

            refLine = tile.TopLeftRefLines.Item1;
            delta = refLine - currentTopLefts.Item1;

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
            if (indexesOfPossibleMovesGroup[2].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = 2;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[2].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(2);
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
            if (indexesOfPossibleMovesGroup[3].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = 3;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[3].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(3);
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
            if (indexesOfPossibleMovesGroup[4].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = 4;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[4].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(4);
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

            if (indexesOfPossibleMovesGroup[5].Count != 0)
            {
                if (!(rowHigh < 0 || colHigh < 0
                || rowHigh >= numOfRows || colHigh >= numOfColumns))
                {
                    tiles[rowHigh][colHigh].HighlightGroup = 5;
                    HighlightTileFromGroup(rowHigh, colHigh);
                    indexesOfPossibleMovesGroup[5].Add((rowHigh, colHigh));
                }
                else
                {
                    DisableHighlightGroup(5);
                }
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

        }
    }
    private void DisableHighlightGroup(int index)
    {
        for (int i = 1; i < indexesOfPossibleMovesGroup[index].Count - 1; i++)
        {
            (int, int) tileIndex = indexesOfPossibleMovesGroup[index][i];
            if (tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.Placed
                && tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.Selected
                && tiles[tileIndex.Item1][tileIndex.Item2].ItsState != Tile.State.SelectedGroup)
            {
                tiles[tileIndex.Item1][tileIndex.Item2].ItsState = Tile.State.Default;
            }
        }
        indexesOfPossibleMovesGroup[index].Clear();
        DebugCurrentReflines(false);
        currentRefLinesExistence[index] = false;
        DebugCurrentReflines(true);
    }
    private void Unselect()
    {
        isTileSelected = false;
        isTileGroupSelected = false;
        EventSystem.Instance.TileDeselected();
    }
    private void PlaceHighlightGroup(int highlightGroup)
    {
        List<(int, int)> indexesOfPlacement = indexesOfPossibleMovesGroup[highlightGroup];

        for (int i = 1; i < indexesOfPlacement.Count; i++)
        {
            int rowIndex = indexesOfPlacement[i].Item1;
            int colIndex = indexesOfPlacement[i].Item2;
            tiles[rowIndex][colIndex].ItsState = Tile.State.Placed;
            ProcessTilePlacement(tiles[rowIndex][colIndex]);
        }
    }
    private void HighlightTileFromGroup(int r, int c)
    {
        if (tiles[r][c].ItsState == Tile.State.Default)
        {
            tiles[r][c].ItsState = Tile.State.HighlightGroup;
        }
    }
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


    // row of Tile relates to the upperhorizRefLine

    }

    */

    #endregion
/*
 
  private void ProcessSelectionGroup()
    {
        isTileSelected = true;
        int indexOfHighlighting;
        for (int i = 0; i < placedTiles.Count; i++)
        {
            Tile tile = placedTiles[i];
            int row = tile.Row;
            int col = tile.Column;

            int delta = currentHorizs.Item1 - row;
            indexOfHighlighting = (r + delta) * numOfColumns + col;

            tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
            tiles[indexOfHighlighting].HighlightGroup = 0;
            indexesOfPossibleMovesGroup[0].Add(indexOfHighlighting);

            delta = currentHorizs.Item2 - row + 1;
            indexOfHighlighting = (r + delta) * numOfColumns + col;

            tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
            tiles[indexOfHighlighting].HighlightGroup = 1;
            indexesOfPossibleMovesGroup[1].Add(indexOfHighlighting);

            #region FirstTopLeft
            //---------------------------------------------------------------------------------------------------
            int deltaLeft = tile.TopLeftRefLines.Item1 - currentTopLefts.Item1 + 1;

            int firstMod = -1;
            int secondMod = -2 - numOfColumns;
            bool IsOrientationUp = tile.Orientation == Tile.OrientType.Up ? true : false;
            indexOfHighlighting = placedTiles[i].Row * numOfColumns + placedTiles[i].Column;
            //delta = Mathf.Abs(delta); // deltaLeft and Right should be positive (not sure)
            while (deltaLeft > 0)
            {

                indexOfHighlighting += IsOrientationUp == true ? secondMod : firstMod;
                if (Mathf.Abs(indexOfHighlighting % numOfColumns - c) > 2
                    || indexOfHighlighting < 0 || indexOfHighlighting > numOfRows * numOfColumns)
                {
                    break;
                }
                deltaLeft--;
                if (deltaLeft == 0)
                {
                    tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
                    tiles[indexOfHighlighting].HighlightGroup = 2;
                    indexesOfPossibleMovesGroup[2].Add(indexOfHighlighting);
                }
                IsOrientationUp = IsOrientationUp ? false : true;
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

            #region SecondTopLeft
            //---------------------------------------------------------------------------------------------------
            int deltaRight = currentTopLefts.Item2 - tile.TopLeftRefLines.Item1;

            firstMod = 1;
            secondMod = 2 + numOfColumns;
            IsOrientationUp = tile.Orientation == Tile.OrientType.Up ? true : false;
            indexOfHighlighting = placedTiles[i].Row * numOfColumns + placedTiles[i].Column;
            while (deltaRight > 0)
            {
                indexOfHighlighting += IsOrientationUp == true ? firstMod : secondMod;
                if (Mathf.Abs(indexOfHighlighting % numOfColumns - c) > 2
                    || indexOfHighlighting < 0 || indexOfHighlighting > numOfRows * numOfColumns)
                {
                    break;
                }
                deltaRight--;
                if (deltaRight == 0)
                {
                    tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
                    tiles[indexOfHighlighting].HighlightGroup = 3;
                    indexesOfPossibleMovesGroup[3].Add(indexOfHighlighting);
                }
                IsOrientationUp = IsOrientationUp ? false : true;
            }
            #endregion

            #region FirstBotLeft
            //---------------------------------------------------------------------------------------------------
            deltaLeft = tile.BotLeftRefLines.Item1 - currentBotLefts.Item1 + 1;

            firstMod = -1;
            secondMod = -2 + numOfColumns;
            IsOrientationUp = tile.Orientation == Tile.OrientType.Up ? true : false;
            indexOfHighlighting = placedTiles[i].Row * numOfColumns + placedTiles[i].Column;
            //delta = Mathf.Abs(delta); // deltaLeft and Right should be positive (not sure)
            while (deltaLeft > 0)
            {
                indexOfHighlighting += IsOrientationUp == true ? firstMod : secondMod;
                if (Mathf.Abs(indexOfHighlighting % numOfColumns - c) > 2
                    || indexOfHighlighting < 0 || indexOfHighlighting > numOfRows * numOfColumns)
                {
                    break;
                }
                deltaLeft--;
                if (deltaLeft == 0)
                {
                    tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
                    tiles[indexOfHighlighting].HighlightGroup = 4;
                    indexesOfPossibleMovesGroup[4].Add(indexOfHighlighting);
                }
                IsOrientationUp = IsOrientationUp ? false : true;
            }
            //---------------------------------------------------------------------------------------------------
            #endregion

            #region SecondBotLeft
            //---------------------------------------------------------------------------------------------------
            deltaRight = currentBotLefts.Item2 - tile.BotLeftRefLines.Item1;

            firstMod = 1;
            secondMod = 2 - numOfColumns;
            IsOrientationUp = tile.Orientation == Tile.OrientType.Up ? true : false;
            indexOfHighlighting = placedTiles[i].Row * numOfColumns + placedTiles[i].Column;
            //delta = Mathf.Abs(delta); // deltaLeft and Right should be positive (not sure)
            while (deltaRight > 0)
            {
                indexOfHighlighting += IsOrientationUp == true ? secondMod : firstMod;
                if (Mathf.Abs(indexOfHighlighting % numOfColumns - c) > 2
                    || indexOfHighlighting < 0 || indexOfHighlighting > numOfRows * numOfColumns)
                {
                    break;
                }
                deltaRight--;
                if (deltaRight == 0)
                {
                    tiles[indexOfHighlighting].ItsState = Tile.State.Highlight;
                    tiles[indexOfHighlighting].HighlightGroup = 5;
                    indexesOfPossibleMovesGroup[5].Add(indexOfHighlighting);
                }
                IsOrientationUp = IsOrientationUp ? false : true;
            }
            //---------------------------------------------------------------------------------------------------
            #endregion
        }
    }
 
 */
