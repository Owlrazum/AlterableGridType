using MLAPI.Messaging;
using MLAPI.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.NetworkVariable;
using TMPro;
using UnityEngine.UI;
using MLAPI;
using System;

public class NetPlayerSelection : NetworkBehaviour
{
    NetworkVariableFloat alpha = new NetworkVariableFloat(0);
    private Image image;
    NetworkVariableString netText = new NetworkVariableString("");
    private TextMeshProUGUI textUI;

    private void Awake()
    {
        alpha.OnValueChanged += ProcessAlphaChange;
        netText.OnValueChanged += ProcessTextChange;
        image = GetComponent<Image>();
        textUI = GetComponentInChildren<TextMeshProUGUI>();
/*        if (NetworkManager.IsServer)
        { 
            //GetComponent<NetworkObject>().Spawn();        
        }*/
    }
    private void OnDestroy()
    {
        alpha.OnValueChanged -= ProcessAlphaChange;
        netText.OnValueChanged -= ProcessTextChange;
    }

    private void ProcessAlphaChange(float previousValue, float newValue)
    {
        Color newC = new Color(1, 1, 1, newValue);
        image.color = newC;
    }

    private void ProcessTextChange(string previousValue, string newValue)
    {
        textUI.text = newValue;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetAlphaServerRpc(float a)
    {
        alpha.Value = a;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetTextServerRpc(string s)
    {
        netText.Value = s;
    }

    public bool CheckAlpha()
    {
        return alpha.Value == 0;
    }
}
