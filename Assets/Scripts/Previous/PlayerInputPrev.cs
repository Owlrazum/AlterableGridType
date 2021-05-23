using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Raycaster))]
public class PlayerInputPrev : MonoBehaviour
{
    public static PlayerInputPrev Instance;
    private void Awake()
    {
        Instance = this;
    }

    bool isButtonSelected = false;

    //public event Action OnButtonDown;
    public void ButtonDown() {
        isButtonSelected = true;
        //OnButtonDown?.Invoke();
    }

    public event Action<Vector3> OnClickAfterButton;
    public void ClickAfterButton(Vector3 pos)
    {
        pos = Raycaster.Instance.ObtainClickPosition(pos);
        if (pos.x != Mathf.Infinity)
        {
            OnClickAfterButton?.Invoke(pos);
            isButtonSelected = false;
        }
    }

    public event Action OnPlacementConfirmed;
    public void PlacementConfirmed()
    {
        OnPlacementConfirmed?.Invoke();
    }

    public event Action OnPlacementRejected;
    public void PlacementRejected()
    {
        OnPlacementRejected?.Invoke();
    }

    public event Action OnSelectAllTilesButtonDown;
    public void SelectAllTilesButtonDown() {
        OnSelectAllTilesButtonDown?.Invoke();
    }

    public event Action OnCanselButtonDown;
    public void Cansel()
    {
        OnCanselButtonDown?.Invoke();
    }

    private bool isDisplayPlace = false;
    public void TheDisplayPlaceStarted()
    {
        isDisplayPlace = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isButtonSelected)
            { 
                ClickAfterButton(Input.mousePosition);
            }
        }
        if (isDisplayPlace)
        {
            if (Input.GetButtonDown("Jump"))
            {
                PlacementConfirmed();
            }
            if (Input.GetButtonDown("Cancel"))
            {
                PlacementRejected();
            }
        }
    }
}
