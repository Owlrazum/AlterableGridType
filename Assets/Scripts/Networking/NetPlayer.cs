using MLAPI;
using MLAPI.Messaging;
using MLAPI.Logging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAPI.Spawning;
using System;

[RequireComponent(typeof(NetTile))]
public class NetPlayer : MonoBehaviour, IEquatable<NetPlayer>, IComparable<NetPlayer>
{
    [SerializeField]
    private NetTile baseTile;
    private NetPs ps;
    private ICinemachineCamera cam;

    private int row;
    private int col;
    private byte id;
    private byte colorIndex;
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
        NetEventSystem.Instance.OnMovePlayer += HandleMoveEvent;
        NetEventSystem.Instance.OnPassPlayerId += InitPlayerId;
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
        NetEventSystem.Instance.OnMovePlayer -= HandleMoveEvent;
    }
    public void Init(NetPs psRef, byte colorIndexArg, (int, int) index)
    {
        ps = psRef;
        (Row, Col) = index;
        colorIndex = colorIndexArg;
        var mats = GridGenerator.Instance.GetMaterials(colorIndex);
        ps.RequestNewPlayerId(colorIndex, row, col, IsSelected);
        baseTile.MakeMain(mats);
        TileMaterial.Instance.AddMaterials(mats);
    }

    private void InitPlayerId(byte colorIndexArg, byte playerId)
    {
        if (colorIndex != colorIndexArg)
        {
            return;
        }
        id = playerId;
        NetEventSystem.Instance.OnPassPlayerId -= InitPlayerId;
    }

    private void HandleMoveEvent(byte receiveId, (int, int) pos)
    { 
        if (id != receiveId)
        {
            return;
        }
        NetGrid.Instance.tiles[row][col].DisplayTrail(colorIndex, 5);
        Row = pos.Item1;
        Col = pos.Item2;
        NetEventSystem.Instance.ReduceTrailIntensity(colorIndex);
    }

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
    private void ProcessPosChange()
    {
        Transform newTransform = NetGrid.Instance.tiles[row][col].transform;
        baseTile.Orientation = NetGrid.Instance.tiles[row][col].Orientation;
        //transform.SetParent(newTransform, false);
        transform.position = newTransform.position + new Vector3(0, 1, 0);
        transform.rotation = newTransform.rotation;
        baseTile.UpdtateSqrHexGb();
    }

    private bool isSpectatorOn;
    public void SpectatorOn()
    {
        isSpectatorOn = true;
    }
    public void SpectatorOff()
    {
        isSpectatorOn = false;
        DisableRelevantParts();
    }
    #region Input

    private bool isSelected;
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
                ps.RequestMove(row, col + 1, id);
            }
            else
            {
                ps.RequestMove(row, col - 1, id);
            }
        }
        if (Input.GetButtonDown("Vertical"))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ps.RequestMove(row + 1, col, id);
            }
            else
            {
                ps.RequestMove(row - 1, col, id);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ps.RequestGridChange();
        }
    }
    #endregion
    public void ProcessGridSwap(NetGridSwapper.GridType oldValue, NetGridSwapper.GridType newValue)
    {
        switch (newValue)
        {
            case NetGridSwapper.GridType.Tri:
                switch(oldValue)
                {
                    case NetGridSwapper.GridType.Sqr:
                        //baseTile.DisableSqrPart();
                        disablePartOption = DisablePart.Sqr;
                        break;
                    case NetGridSwapper.GridType.Hex:
                        //baseTile.DisableSqrPart();
                        //baseTile.DisableHexPart();
                        disablePartOption = DisablePart.SqrHex;
                        break;
                }
                break;
            case NetGridSwapper.GridType.Sqr:
                switch (oldValue)
                {
                    case NetGridSwapper.GridType.Tri:
                        baseTile.EnableSqrPart();
                        disablePartOption = DisablePart.None;
                        break;
                    case NetGridSwapper.GridType.Hex:
                        //baseTile.DisableHexPart();
                        disablePartOption = DisablePart.Hex;
                        break;
                }
                break;
            case NetGridSwapper.GridType.Hex:
                switch (oldValue)
                {
                    case NetGridSwapper.GridType.Tri:
                        baseTile.EnableSqrPart();
                        baseTile.EnableHexPart();
                        disablePartOption = DisablePart.None;
                        break;
                    case NetGridSwapper.GridType.Sqr:
                        baseTile.EnableHexPart();
                        disablePartOption = DisablePart.None;
                        break;
                }
                break;
        }
        baseTile.UpdtateSqrHexGb();
    }
    private enum DisablePart
    { 
        None,
        Sqr,
        SqrHex,
        Hex
    }
    private DisablePart disablePartOption;
    private void DisableRelevantParts()
    {
        switch (disablePartOption)
        {
            case DisablePart.None:
                return;
            case DisablePart.Sqr:
                baseTile.DisableSqrPart();
                break;
            case DisablePart.SqrHex:
                baseTile.DisableSqrPart();
                baseTile.DisableHexPart();
                break;
            case DisablePart.Hex:
                baseTile.DisableHexPart();
                break;
        }
    }

    #region Interfaces
    public bool Equals(NetPlayer other)
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
            NetPlayer t = (NetPlayer)obj;
            return Equals(t);
        }
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public static bool operator ==(NetPlayer t1, NetPlayer t2)
    {
        return t1.Equals(t2);
    }
    public static bool operator !=(NetPlayer t1, NetPlayer t2)
    {
        return !t1.Equals(t2);
    }

    public int CompareTo(NetPlayer other)
    {
        return id.CompareTo(other.id);
    }
    #endregion
}
