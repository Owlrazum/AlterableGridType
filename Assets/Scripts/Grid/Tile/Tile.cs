using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour, IComparable<Tile>
{
    //[SerializeField]
    private GameObject squareGb;
   // [SerializeField]
    private GameObject[] hexGbs;

    private GameObject hexGb;

    private GameObject defaultGb;
    private GameObject selectedGb;
    private GameObject highlightGb;

    private GameObject placedGb_P1;
    private GameObject placedGb_P2;

    private GameObject currentGb;

    private void Awake()
    {
        hexGbs = new GameObject[2];

        isSquarePartEnabled = false;
        isHexPartEnabled = false;
        areReflinesEnabled = true;

        defaultGb   = transform.GetChild(0).gameObject;
        selectedGb  = transform.GetChild(1).gameObject;
        highlightGb = transform.GetChild(2).gameObject;
        placedGb_P1 = transform.GetChild(3).gameObject;
        placedGb_P2 = transform.GetChild(4).gameObject;

        currentGb = defaultGb;
        ItsState = State.Default;
        isMain = false;
    }

    public void UpdtateSqrHexGb ()
    {
        squareGb = currentGb.transform.GetChild(1).gameObject;
        hexGbs[0] = currentGb.transform.GetChild(2).gameObject;
        hexGbs[1] = currentGb.transform.GetChild(3).gameObject;
        if (Orientation == OrientType.Up)
        {
            hexGbs[0].SetActive(true);
            hexGbs[1].SetActive(false);
            hexGb = hexGbs[0];
        }
        else
        {
            hexGbs[0].SetActive(false);
            hexGbs[1].SetActive(true);
            hexGb = hexGbs[1];
        }
        if (isSquarePartEnabled)
            squareGb.SetActive(true);
        else
            squareGb.SetActive(false);
        if (isHexPartEnabled)
            hexGb.SetActive(true);
        else
            hexGb.SetActive(false);
    }

    public enum OrientType
    {
        Up,
        Down
    }
    public OrientType Orientation { get; set; }

    //[SerializeField]
    //private DelTrig delTrig;

    private bool isSquarePartEnabled;
    private bool isHexPartEnabled;
    private bool areReflinesEnabled;


    public void EnableSqrPart()
    {
        isSquarePartEnabled = true;
        squareGb.SetActive(true);
        ItsState = ItsState;
    }
    public void DisableSqrPart()
    {
        isSquarePartEnabled = false;
        squareGb.SetActive(false);
    }

    public void EnableHexPart()
    {
        isHexPartEnabled = true;
        hexGb.SetActive(true);
        ItsState = ItsState;
    }

    public void DisableHexPart()
    {
        isHexPartEnabled = false;
        hexGb.SetActive(false);
    }

    public void SwitchHexPart()
    { 
        
    }

    #region TileMode
    public void Listen()
    {
        EventSystem.Instance.OnTileCreated += Created;
        EventSystem.Instance.OnTileDeselected += TileDeselected;
    }

    #region State
    public enum State
    {
        Default,
        Highlight,
        HighlightGroup,
        Selected, // does not used ?
        PlacedFirst,
        PlacedSecond,
    }
    private State state;

    public State ItsState
    {
        get { return state; }
        set
        {
            switch (value)
            {
                case State.Default:
                    currentGb.SetActive(false);
                    currentGb = defaultGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                   /* trianglePart.color = defaultMaterial.color;
                    if (isSquarePartEnabled) squarePart.color = defaultMaterial.color;
                    if (isHexPartEnabled)
                    { 
                        hexPart.color = defaultMaterial.color;
                    }*/
                    state = value;
                    break;
                case State.Highlight:
                    currentGb.SetActive(false);
                    currentGb = highlightGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    /*trianglePart.color = highlightMaterial.color;
                    if (isSquarePartEnabled) squarePart.color = highlightMaterial.color;
                    if (isHexPartEnabled)
                    {
                        hexPart.color = highlightMaterial.color;
                    }*/
                    state = value;
                    break;
                case State.HighlightGroup:
                    /*trianglePart.color = groupHighlightMaterials[HighlightGroup].color;
                    if (isSquarePartEnabled) squarePart.color = groupHighlightMaterials[HighlightGroup].color;
                    if (isHexPartEnabled)
                    {
                        hexPart.color = groupHighlightMaterials[HighlightGroup].color;
                    }*/
                    state = value;
                    break;
                case State.Selected:
                    currentGb.SetActive(false);
                    currentGb = selectedGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    /*trianglePart.color = selectedMaterial.color;
                    if (isSquarePartEnabled) squarePart.color = selectedMaterial.color;
                    if (isHexPartEnabled)
                    {
                        hexPart.color = selectedMaterial.color;
                    }*/
                    prevState = state;
                    state = value;
                    break;
                /*case State.Placed:
                    if (!isPlaced)
                    {
                        isPlaced = true;
                        //delTrig.OnPlacement();
                    }
                    currentGb.SetActive(false);
                    currentGb = placedGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    *//*trianglePart.color = placedMaterial.color;
                    if (isSquarePartEnabled) squarePart.color = placedMaterial.color;
                    if (isHexPartEnabled)
                    {
                        hexPart.color = placedMaterial.color;
                    }*//*
                    state = value;
                    break;*/
                case State.PlacedFirst:
                    currentGb.SetActive(false);
                    currentGb = placedGb_P1;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    state = value;
                    break;
                case State.PlacedSecond:
                    currentGb.SetActive(false);
                    currentGb = placedGb_P2;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    state = value;
                    break;
            }
        }
    }
    #endregion

    public bool isMain;
    public void MakeMain(PlayersSystem.Side side, Material[] mats)
    {
        isMain = true;
        MeshRenderer[] renders;
        if (side == PlayersSystem.Side.White)
        { 
            renders = placedGb_P1.GetComponentsInChildren<MeshRenderer>(true);
            ItsState = State.PlacedFirst;
        }
        else 
        {
            renders = placedGb_P2.GetComponentsInChildren<MeshRenderer>(true);
            ItsState = State.PlacedSecond;
        }
        EventSystem.Instance.TilePlaced(side, this);
        for (int i = 0; i < 3; i++)
        {
            renders[i].material = mats[i];
        }
        renders[3].material = mats[2];
        renders[4].material = mats[2];
        renders[5].material = mats[2];
        renders[6].material = mats[2];
        renders[7].material = mats[2];
    }

    public int HighlightGroup { get; set; }

    #region RefLines
    private int horizFirstRefLineIndex = -1;
    private int horizSecondRefLineIndex = -1;
    public (int, int) HorizRefLines
    {
        get { return (horizFirstRefLineIndex, horizSecondRefLineIndex); }
        set
        {
            if (horizFirstRefLineIndex == -1)
            {
                horizFirstRefLineIndex = value.Item1;
            }
            if (horizSecondRefLineIndex == -1)
            {
                horizSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int vertFirstRefLineIndex = -1;
    private int vertSecondRefLineIndex = -1;
    public (int, int) VertRefLines
    {
        get { return (vertFirstRefLineIndex, vertSecondRefLineIndex); }
        set
        {
            if (vertFirstRefLineIndex == -1)
            {
                vertFirstRefLineIndex = value.Item1;
            }
            if (vertSecondRefLineIndex == -1)
            {
                vertSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int topLeftFirstRefLineIndex = -1;
    private int topLeftSecondRefLineIndex = -1;

    public (int, int) TopLeftRefLines
    {
        get { return (topLeftFirstRefLineIndex, topLeftSecondRefLineIndex); }
        set
        {
            if (topLeftFirstRefLineIndex == -1)
            {
                topLeftFirstRefLineIndex = value.Item1;
            }
            if (topLeftSecondRefLineIndex == -1)
            {
                topLeftSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int botLeftFirstRefLineIndex = -1;
    private int botLeftSecondRefLineIndex = -1;
    public (int, int) BotLeftRefLines
    {
        get { return (botLeftFirstRefLineIndex, botLeftSecondRefLineIndex); }
        set
        {
            if (botLeftFirstRefLineIndex == -1)
            {
                botLeftFirstRefLineIndex = value.Item1;
            }
            if (botLeftSecondRefLineIndex == -1)
            {
                botLeftSecondRefLineIndex = value.Item2;
            }
        }
    }
    #endregion

    private int row;
    private int column;

    public int Row { get; private set; }
    public int Column { get; private set; }

    public void IncRow()
    {
        Row++;
    }
    public void DecRow()
    {
        Row--;
    }
    public void IncCol()
    {
        Column++;
    }
    public void DecCol()
    {
        Column--;
    }


    private void Created(int r, int c, OrientType orient)
    {
        Row = r;
        Column = c;
        Orientation = orient;
        EventSystem.Instance.OnTileCreated -= Created;
    }
    private void TileDeselected()
    {
        switch (state)
        {
            case State.Highlight:
            case State.HighlightGroup:
                ItsState = State.Default;
                break;
            case State.Selected:
                ItsState = prevState;
                break;
        }
    }

    public void Log()
    {
        Debug.Log("Tile " + row + " " + column + ".");
    }

    #endregion

    private State prevState; 

    public int CompareTo(Tile other)
    {
        int comp = Column.CompareTo(other.Column);
        if (comp != 0)
        {
            return comp;
        }
        else
        {
            comp = Row.CompareTo(other.Row);
            return comp;
        }
    }
}
