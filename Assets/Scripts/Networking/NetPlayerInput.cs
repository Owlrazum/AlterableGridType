using System;
using UnityEngine;

public class NetPlayerInput : MonoBehaviour
{
    private void Start()
    {
        NetEventSystem.Instance.OnSpectatorOn += ProcessSpectatorOn;
        NetEventSystem.Instance.OnSpectatorOff += ProcessSpectatorOff;
    }

    #region new
    #endregion

    private const int layerMaskTiles = 1 << 8;
    private Player selectedPlayer;

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

    private void Update()
    {
        return;
        if (isSpectatorOn)
        {
            return;
        }
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit rayHit, Mathf.Infinity, layerMaskTiles))
            {
                Debug.Log("HittedTile");
                Player player = rayHit.collider.GetComponentInParent<Player>();
                if (player == null)
                {
                    return;
                }
                Debug.Log("FoundPlayer");
                if (selectedPlayer != null)
                { 
                    selectedPlayer.IsSelected = false;
                }
                player.IsSelected = true;
                selectedPlayer = player;
            }
        }
    }
}
