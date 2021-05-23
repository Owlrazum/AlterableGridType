using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileFinish : MonoBehaviour
{
    private Button button;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonDown);

        EventSystem.Instance.OnSpectatorOn += ProcessSpectatorOn;
        EventSystem.Instance.OnSpectatorOff += ProcessSpectatorOff;
    }
    private void ButtonDown()
    {
        Debug.Log("Button");
        PlayerInput.Instance.ChangeGrid();
    }

    private void ProcessSpectatorOn()
    {
        gameObject.SetActive(false);
    }

    private void ProcessSpectatorOff()
    {
        gameObject.SetActive(true);
    }
}
