using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Serialization;
using System;
using UnityEngine;

public class NetTile : MonoBehaviour, IEquatable<NetTile>, IComparable<NetTile>
{
    private GameObject squareGb;
    private GameObject[] hexGbs;

/*
    public int Row { get; private set; }
    public int Col { get; private set; }*/

    public int Row { get; private set; }
    public int Col { get; private set; }
    public bool IsPlayerOwned { get; set; }

    private GameObject hexGb;

    private GameObject defaultGb;
    private GameObject trailGb;
    private GameObject highlightGb;

    private GameObject placedGb;

    private GameObject currentGb;

    private void Awake()
    {
        hexGbs = new GameObject[2];

        isSquarePartEnabled = false;
        isHexPartEnabled = false;

        defaultGb = transform.GetChild(0).gameObject;
        trailGb = transform.GetChild(1).gameObject;
        highlightGb = transform.GetChild(2).gameObject;
        placedGb = transform.GetChild(3).gameObject;

        currentGb = defaultGb;
        UpdtateSqrHexGb();
        ItsState = State.Default;
        isMain = false;
    }

    public void UpdtateSqrHexGb()
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
    private bool isSquarePartEnabled;
    private bool isHexPartEnabled;

    public void EnableSqrPart()
    {
        isSquarePartEnabled = true;
        squareGb.SetActive(true);
        UpdtateSqrHexGb();
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
        UpdtateSqrHexGb();
    }
    public void DisableHexPart()
    {
        isHexPartEnabled = false;
        hexGb.SetActive(false);
    }
    public void Listen()
    {
        NetEventSystem.Instance.OnTileCreated += Created;
    }

    #region State
    public enum State
    {
        Default,
        Highlight,
        Trail, // does not used ?
        Placed,
    }
    private State state;

    public State ItsState
    {
        get { return state; }
        set
        {
            Debug.Log("State changed " + value + " from " + state);
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
                case State.Trail:
                    currentGb.SetActive(false);
                    currentGb = trailGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
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
                case State.Placed:
                    currentGb.SetActive(false);
                    currentGb = placedGb;
                    currentGb.SetActive(true);
                    UpdtateSqrHexGb();
                    state = value;
                    break;
            }
        }
    }
    #endregion

    public bool isMain;
    public void MakeMain(Material[] mats)
    {
        isMain = true;
        MeshRenderer[] renders;
        renders = placedGb.GetComponentsInChildren<MeshRenderer>(true);
        ItsState = State.Placed;
        renders[0].material = mats[0];
        renders[1].material = mats[1];
        for (int i = 2; i < 8; i++) // 0 1 are for tri and sqr. three on up and three on down.
        {
            renders[i].material = mats[2];
        }
    }
    private (byte, byte) intensityData;
    MeshRenderer[] trailRenderers;
    public void DisplayTrail(byte colorIndex, byte intensityArg)
    {
        if (isMain)
        {
            return;
        }
        if (intensityArg == 0)
        {
            ItsState = State.Default;
        }
        ItsState = State.Trail;
        var mats = GridGenerator.Instance.GetMaterials(colorIndex, intensityArg);
        trailRenderers = trailGb.GetComponentsInChildren<MeshRenderer>(true);
        trailRenderers[0].material = mats[0];
        trailRenderers[1].material = mats[1];
        for (int i = 2; i < 8; i++) // 0 1 are for tri and sqr. three on up and three on down.
        {
            trailRenderers[i].material = mats[2];
        }
        intensityData = (colorIndex, intensityArg);
        NetEventSystem.Instance.OnReduceTrailIntensity += HandleReduceIntensityEvent;
    }

    private void HandleReduceIntensityEvent(byte colorIndex)
    {
        if (isMain)
        {
            return;
        }
        if (intensityData.Item1 == colorIndex)
        {
            intensityData.Item2--;
            if (intensityData.Item2 == 0)
            {
                ItsState = State.Default;
                NetEventSystem.Instance.OnReduceTrailIntensity -= HandleReduceIntensityEvent;
            }
            var mats = GridGenerator.Instance.GetMaterials(intensityData.Item1, intensityData.Item2);
            trailRenderers[0].material = mats[0];
            trailRenderers[1].material = mats[1];
            for (int i = 2; i < 8; i++) // 0 1 are for tri and sqr. three on up and three on down.
            {
                trailRenderers[i].material = mats[2];
            }
        }
    }
    private void Created(int r, int c, OrientType orient)
    {
        Row = r;
        Col = c;
        Orientation = orient;
        IsPlayerOwned = false;
        NetEventSystem.Instance.OnTileCreated -= Created;
    }
    #region Interfaces
    public int CompareTo(NetTile other)
    {
        if (IsPlayerOwned == other.IsPlayerOwned)
        {
            int comp = Col.CompareTo(other.Col);
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
        else
        {
            return IsPlayerOwned ? 1 : -1;
        }
    }

    public bool Equals(NetTile other)
    {
        return Row == other.Row && Col == other.Col && IsPlayerOwned == other.IsPlayerOwned;
    }
    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            NetTile t = (NetTile)obj;
            return Equals(t);
        }
    }

    public override int GetHashCode()
    {
        return (Row, Col).GetHashCode();
    }

    public static bool operator ==(NetTile t1, NetTile t2)
    {
        return t1.Equals(t2);
    }
    public static bool operator !=(NetTile t1, NetTile t2)
    {
        return !t1.Equals(t2);
    }
    #endregion
}

