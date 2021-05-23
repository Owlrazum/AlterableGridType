using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;
    private void Awake()
    {
        Instance = this;
        contTimer = deltaTimeCont;
    }
    private void Start()
    {
        EventSystem.Instance.OnSpectatorOn += ProcessSpectatorOn;
        EventSystem.Instance.OnSpectatorOff += ProcessSpectatorOff;
    }

    public event Action<Vector3> OnClick;
    public void Click(Vector3 pos)
    {
        OnClick?.Invoke(pos);
    }
    #region new
    private string Config { get; set; }
    #endregion

    public event Action OnSelectGroupButtonDown;
    public void SelectAllTilesButtonDown() 
    {
        OnSelectGroupButtonDown?.Invoke();
    }

    public event Action OnHighlightModeChange;
    public void HighlightModeChanged()
    {
        OnHighlightModeChange?.Invoke();
    }

    public event Action OnDeleteButtonDown;
    public void DeleteButtonDown()
    {
        OnDeleteButtonDown?.Invoke();
    }

    public event Action OnChangeGrid;
    public void ChangeGrid()
    {
        EventSystem.Instance.SpectatorOn();
        OnChangeGrid?.Invoke();
    }

    public event Action OnTileModeStarted;
    public void TileModeStarted()
    {
        OnTileModeStarted?.Invoke();
    }


    public bool IsShiftButtonPressed { get; private set; }
    private const int layerMaskGraph = 1 << 10;
    private const int layerMaskPlane = 1 << 11;
    private int delay = 1;

    #region Spectator
    private bool isSpectatorOn = false;
    private void ProcessSpectatorOn()
    {
        isSpectatorOn = true;
    }

    private void ProcessSpectatorOff()
    {
        isSpectatorOn = false;
    }
    #endregion

    private bool wasKeyD = false;
    private bool wasKeyA = false;
    private bool wasKeyE = false;
    private bool wasKeyQ = false;

    private enum KeyCont
    { 
        D,
        A, 
        E, 
        Q
    }

    private float deltaTimeCont = 0.3f;
    private float contTimer;

    private void Update()
    {
        if (isSpectatorOn)
        {
            return;
        }
        if (Input.GetButtonDown("HorizontalLower"))
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                wasKeyD = true;
                //ps.RecordMoving(Player.Direction.RightDown);/
            }
            else 
            {
                wasKeyA = true;
               // ps.RecordMoving(Player.Direction.LeftDown);//
            }
        }
        else if (Input.GetButtonDown("HorizontalUpper"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
               // ps.RecordMoving(Player.Direction.RightUp);//
                wasKeyE = true;
            }
            else
            {
                //ps.RecordMoving(Player.Direction.LeftUp);//
                wasKeyQ = true;
            }
        }
        if (Input.GetButtonDown("Vertical"))
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                //ps.RecordMoving(Player.Direction.Up);//
            }
            else
            {
               // ps.RecordMoving(Player.Direction.Down);//
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            Click(Input.mousePosition);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            SelectAllTilesButtonDown();
        }
        else if (Input.GetButtonDown("CustomLetterM"))
        {
            HighlightModeChanged();
        }
        //else if (Input.GetButtonDown("Delete"))
        //{
        //    DeleteButtonDown();
        //}
        else if (Input.GetButton("Fire3")) // left shift
        {
            if (!IsShiftButtonPressed)
            {
                IsShiftButtonPressed = true;
            }
        }
        else if (!Input.GetButton("Fire3"))
        {
            if (IsShiftButtonPressed)
            {
                IsShiftButtonPressed = false;
            }
        }
        if (wasKeyA || wasKeyD || wasKeyE || wasKeyQ)
        {
            ProcessContiniousPress(KeyCont.A, wasKeyA);
            ProcessContiniousPress(KeyCont.D, wasKeyD);
            ProcessContiniousPress(KeyCont.Q, wasKeyQ);
            ProcessContiniousPress(KeyCont.E, wasKeyE);
            if (Input.GetKeyUp(KeyCode.A))
                wasKeyA = false;
            if (Input.GetKeyUp(KeyCode.D))
                wasKeyD = false;
            if (Input.GetKeyUp(KeyCode.Q))
                wasKeyQ = false;
            if (Input.GetKeyUp(KeyCode.E))
                wasKeyE = false;
        }
        else 
        {
            contTimer = deltaTimeCont;
        }
    }

    private void ProcessContiniousPress(KeyCont keyPr, bool wasKey)
    {
        KeyCode code = KeyCode.None;
        Player.Direction dir = Player.Direction.Zero;
        switch (keyPr)
        {
            case KeyCont.A:
                if (!wasKey)
                {
                    return;
                }
                code = KeyCode.A;
                dir = Player.Direction.LeftDown;
                break;
            case KeyCont.D:
                if (!wasKey)
                {
                    return;
                }
                code = KeyCode.D;
                dir = Player.Direction.RightDown;
                break;
            case KeyCont.Q:
                if (!wasKey)
                {
                    return;
                }
                code = KeyCode.Q;
                dir = Player.Direction.LeftUp;
                break;
            case KeyCont.E:
                if (!wasKey)
                {
                    return;
                }
                code = KeyCode.E;
                dir = Player.Direction.RightUp;
                break;
        }
        if (wasKey && Input.GetKey(code))
        {
            if (contTimer <= 0)
            {
               // ps.RecordMoving(dir);//
                contTimer = deltaTimeCont;
            }
            else
            {
                contTimer -= Time.deltaTime;
            }
        }
    }
}
