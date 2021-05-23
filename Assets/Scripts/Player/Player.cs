using System.Collections;
using UnityEngine;
using Cinemachine;
using System;

[RequireComponent(typeof(NetTile))]
public class Player : MonoBehaviour, IEquatable<Player>, IComparable<Player>
{
    [SerializeField]
    private NetTile baseTile;
    private PlayersSystem ps;
    private ICinemachineCamera cam;

    public void Init(PlayersSystem psRef, byte colorIndex)
    {
        ps = psRef;
        int rnd1, rnd2;
        do
        {
            rnd1 = UnityEngine.Random.Range(0, NetGrid.Instance.tiles.Count);
            rnd2 = UnityEngine.Random.Range(0, NetGrid.Instance.tiles[0].Count);

        } while (!ps.CheckMove((rnd1, rnd2)));
        Row = rnd1;
        Col = rnd2;
        var mats = GridGenerator.Instance.GetMaterials(colorIndex);
        PlayersSystem.Side side = colorIndex <= 2 ? PlayersSystem.Side.White : PlayersSystem.Side.Black;
        id = ps.GetNewPlayerId(side, (row, col));
        baseTile.MakeMain(mats);
    }
    private int row;
    private int col;
    private bool isSelected;
    private byte id;
    public byte Id
    { 
        get { return id; }
        set { id = value; }
    }

    public int Row
    { 
        get { return row; }
        set 
        {
            if (value == row)
            {
                return;
            }
            if (value < 0 || value >= NetGrid.Instance.tiles.Count)
            {
                return;
            }
            row = value;
            ProcessPosChange();
        }
    }
    public int Col
    {
        get { return col; }
        set
        {
            if (value == col)
            {
                return;
            }
            if (value < 0 || value >= NetGrid.Instance.tiles[0].Count)
            {
                return;
            }
            col = value;
            ProcessPosChange();
        }
    }
    public bool IsSelected 
    {
        get { return isSelected; }
        set
        {
            if (value)
            {
                cam.Follow = transform;
                cam.LookAt = transform;
            }
            isSelected = value;
        }
    }
    private void ProcessPosChange()
    {
        Transform newTransform = NetGrid.Instance.tiles[row][col].transform;
        baseTile.Orientation = NetGrid.Instance.tiles[row][col].Orientation;
        transform.SetParent(newTransform, false);
        baseTile.UpdtateSqrHexGb();
    }
    private bool isSpectatorOn;
    public NetTile GetBaseTile()
    {
        return baseTile;
    }
    private void Awake()
    {
        baseTile.IsPlayerOwned = true;
        isSpectatorOn = false;
    }

    private void OnEnable()
    {
        NetEventSystem.Instance.OnGridSwapped += ProcessGridSwap;
        NetEventSystem.Instance.OnSpectatorOff += SpectatorOff;
        NetEventSystem.Instance.OnSpectatorOn += SpectatorOn;
        NetEventSystem.Instance.OnResetSingle += DestroyThis;
        CinemachineBrain b = Camera.main.GetComponent<CinemachineBrain>();
        cam = b.ActiveVirtualCamera;
    }
    private void DestroyThis()
    {
        Destroy(gameObject);
        NetEventSystem.Instance.OnResetSingle -= DestroyThis;
    }
    private void OnDisable()
    {
        NetEventSystem.Instance.OnGridSwapped -= ProcessGridSwap;
        NetEventSystem.Instance.OnSpectatorOff -= SpectatorOff;
        NetEventSystem.Instance.OnSpectatorOn -= SpectatorOn;
        NetEventSystem.Instance.SetGridType(NetGridSwapper.GridType.Tri);
    }

    public void SpectatorOn()
    {
        isSpectatorOn = true;
    }
    public void SpectatorOff()
    {
        isSpectatorOn = false;
    }
    #region Input
    private enum KeyCont
    {
        D,
        A,
        E,
        Q
    }
    /*    private bool wasKeyD = false;
        private bool wasKeyA = false;
        private bool wasKeyE = false;
        private bool wasKeyQ = false;*/

    private float deltaTimeCont = 0.3f;
    private float contTimer;
    private void Update()
    {

        if (isSpectatorOn || !IsSelected)
        {
            return;
        }
        if (Input.GetButtonDown("HorizontalLower"))
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (ps.CheckMove((row, col + 1)))
                { 
                    Col++;
                    ps.ProcessPlayerMove(id, (row, col));
                }
            }
            else
            {
                if (ps.CheckMove((row, col - 1)))
                {
                    Col--;
                    ps.ProcessPlayerMove(id, (row, col));
                }
            }
        }
        if (Input.GetButtonDown("Vertical"))
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                if (ps.CheckMove((row + 1, col)))
                {
                    Row++;
                    ps.ProcessPlayerMove(id, (row, col));
                }
            }
            else
            {
                if (ps.CheckMove((row - 1, col)))
                {
                    Row--;
                    ps.ProcessPlayerMove(id, (row, col));
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            NetEventSystem.Instance.SwitchGridType();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            NetEventSystem.Instance.MakeNewPlayer();
        }
    }
    #endregion
    public void ProcessGridSwap(NetGridSwapper.GridType oldValue, NetGridSwapper.GridType newValue)
    {
        switch (newValue)
        {
            case NetGridSwapper.GridType.Tri:
                baseTile.DisableSqrPart();
                baseTile.DisableHexPart();
                break;
            case NetGridSwapper.GridType.Sqr:
                baseTile.EnableSqrPart();
                baseTile.DisableHexPart();
                break;
            case NetGridSwapper.GridType.Hex:
                baseTile.EnableSqrPart();
                baseTile.EnableHexPart();
                break;
        }
        baseTile.UpdtateSqrHexGb();
    }
    #region MoveLogic
    public enum Direction
    {
        Up,
        Down,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        Zero
    }


    #endregion

    #region Interfaces
    public bool Equals(Player other)
    {
        return id == other.id;
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
            Player t = (Player)obj;
            return Equals(t);
        }
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public static bool operator ==(Player t1, Player t2)
    {
        return ReferenceEquals(t1, t2);
    }
    public static bool operator !=(Player t1, Player t2)
    {
        return !ReferenceEquals(t1, t2);
    }

    public int CompareTo(Player other)
    {
        return id.CompareTo(other.id);
    }
    #endregion

}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Tile baseTile;
    private int row;
    private int col;
    public Player(Tile tile)
    {
        baseTile = tile;
        row = tile.Row;
        col = tile.Column;
        isSpectatorOn = false;
    }

    private bool isSpectatorOn;

    public void SpectatorOn()
    {
        isSpectatorOn = true;
    }
    public void SpectatorOff()
    {
        isSpectatorOn = false;
    }
    public enum Direction
    {
        Up,
        Down,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        Zero
    }
    public void RecordMoving(Direction dir, bool isWorldStays = false)
    {
        if (dir == Direction.Zero || isSpectatorOn)
        {
            return;
        }
        GridController.tiles[row][col].ItsState = Tile.State.PlacedFirst;
        switch (dir)
        {
            case Direction.Up:
                MoveUp();
                break;
            case Direction.Down:
                MoveDown();
                break;
            case Direction.LeftUp:
                MoveLeftUp();
                break;
            case Direction.LeftDown:
                MoveLeftDown();
                break;
            case Direction.RightUp:
                MoveRightUp();
                break;
            case Direction.RightDown:
                MoveRightDown();
                break;
        }
        Transform newParent = GridController.tiles[row][col].transform;
        baseTile.transform.SetParent(newParent, isWorldStays);
        baseTile.Orientation = GridController.tiles[row][col].Orientation;
        baseTile.UpdtateSqrHexGb();
    }
    public void ProcessGridSwap(NetGridSwapper.GridType type)
    {
        switch (type)
        {
            case NetGridSwapper.GridType.Tri:
                baseTile.DisableSqrPart();
                baseTile.DisableHexPart();
                break;
            case NetGridSwapper.GridType.Sqr:
                baseTile.EnableSqrPart();
                baseTile.DisableHexPart();
                break;
            case NetGridSwapper.GridType.Hex:
                baseTile.EnableSqrPart();
                baseTile.EnableHexPart();
                break;
        }

        baseTile.UpdtateSqrHexGb();
    }

    private Vector3 basePos = new Vector3(0, 1, 0);
    private Vector3 baseRot = Vector3.zero;
    private float minDelta = 0.1f;
    public bool MoveToTile()
    {
        Vector3 delta = basePos - baseTile.transform.localPosition;
        baseTile.transform.localPosition += delta * 0.05f;
        if (GridSwapper.currentGrid == GridSwapper.GridType.Tri)
        {
            delta = baseRot - baseTile.transform.localRotation.eulerAngles;
            Quaternion newRot = baseTile.transform.localRotation;
            newRot.eulerAngles += delta * 0.05f;
            baseTile.transform.localRotation = newRot;
        }
        delta = basePos - baseTile.transform.localPosition;
        if (Mathf.Abs(delta.x) < minDelta && Mathf.Abs(delta.z) < minDelta)
        {
            baseTile.transform.localPosition = basePos;
            baseTile.transform.localRotation = Quaternion.Euler(baseRot);
            return true;
        }
        else 
        {
            return false;
        }
    }
    public void SetRotationTile()
    {
        baseTile.transform.localRotation = Quaternion.Euler(baseRot);
    }
    #region MoveLogic
    public void MoveUp()
    {
        if (row + 1 >= GridController.tiles.Count)
        {
            return;
        }
        if (GridSwapper.currentGrid == GridSwapper.GridType.Tri)
        {
            if (baseTile.Orientation == Tile.OrientType.Up
                && GridController.tiles[row + 1][col].Orientation == Tile.OrientType.Down)
            {
                return;
            }
        }
        row++;
    }
    public void MoveDown()
    {
        if (row - 1 < 0)
        {
            return;
        }
        if (GridSwapper.currentGrid == GridSwapper.GridType.Tri)
        {
            if (baseTile.Orientation == Tile.OrientType.Down
                && GridController.tiles[row - 1][col].Orientation == Tile.OrientType.Up)
            {
                return;
            }
        }
        row--;
    }
    public void MoveLeftUp()
    {
        if (GridSwapper.currentGrid != GridSwapper.GridType.Hex)
        {
            return;
        }
        if (col % 2 == 0)
        {
            if (row + 1 >= GridController.tiles.Count || col - 1 < 0)
            {
                return;
            }
            row++;
            col--;

        }
        else
        {
            if (col - 1 < 0)
            {
                return;
            }
            col--;
        }
    }
    public void MoveLeftDown()
    {
        if (GridSwapper.currentGrid != GridSwapper.GridType.Hex)
        {
            if (col - 1 < 0)
            {
                return;
            }
            col--;
        }
        else
        {
            if (col % 2 == 1)
            {
                if (row - 1 < 0 || col - 1 < 0)
                {
                    return;
                }
                row--;
                col--;

            }
            else
            {
                if (col - 1 < 0)
                {
                    return;
                }
                col--;
            }
        }
    }
    public void MoveRightUp()
    {
        if (GridSwapper.currentGrid != GridSwapper.GridType.Hex)
        {
            return;
        }
        if (col % 2 == 0)
        {
            if (row + 1 >= GridController.tiles.Count || col + 1 >= GridController.tiles[row].Count)
            {
                return;
            }
            row++;
            col++;

        }
        else
        {
            if (col + 1 >= GridController.tiles[row].Count)
            {
                return;
            }
            col++;
        }
    }
    public void MoveRightDown()
    {

        if (GridSwapper.currentGrid != GridSwapper.GridType.Hex)
        {
            if (col + 1 >= GridController.tiles[row].Count)
            {
                return;
            }
            col++;
        }
        else
        {
            if (col % 2 == 1)
            {
                if (row - 1 < 0
                    || col + 1 >= GridController.tiles[row].Count)
                {
                    return;
                }
                row--;
                col++;

            }
            else
            {
                if (col + 1 >= GridController.tiles[row].Count)
                {
                    return;
                }
                col++;
            }
        }
    }

    #endregion
}
*/