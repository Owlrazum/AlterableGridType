using Cinemachine;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using System;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
    [SerializeField]
    private ConnectionSystem cs;
    [SerializeField]
    private NetPlayerCreator creator;
    [SerializeField]
    private NetColorSelect colorSelect;
    [SerializeField]
    private GameObject localPlayerPrefab;
    [SerializeField]
    private PlayersSystem psRef;

    private bool isColorPicked = false;
    public void SelectColor(int colorIndex)
    {
        if (!cs.IsNetworking || isColorPicked)
        {
            return;
        }
        //creator.NewPlayer((byte)colorIndex);
        if (!colorSelect.TrySelectColor((byte)(colorIndex - 1)))
        {
            Debug.Log("NetColor false");
            return;
        }
        //Debug.Log("NetColor true");
        isColorPicked = true;
    }
    public void SelectColorLocal(int colorIndex)
    {
        if (cs.IsNetworking)
        {
            return;
        }
        Debug.Log("Local");
        Player player = Instantiate(localPlayerPrefab).GetComponent<Player>();
        player.Init(psRef, (byte)colorIndex);
        cs.ColorPicked();
    }

    private void Update()
    {
        if (cs.IsNetworking)
        {
            return;
        }
    }
}
