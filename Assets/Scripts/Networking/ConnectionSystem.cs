using UnityEngine;
using MLAPI;
using TMPro;
using System.Text;
using MLAPI.Spawning;
using MLAPI.Connection;
using MLAPI.Logging;
using MLAPI.Messaging;

public class ConnectionSystem : NetworkBehaviour
{
    [SerializeField] 
    private TMP_InputField passwordInputField;
    [SerializeField] 
    private GameObject passwordEntryUI;
    [SerializeField] 
    private GameObject leaveButton;
    [SerializeField]
    private GameObject startButton;
    [SerializeField]
    private GameObject noHostRunningMessage;
    [SerializeField]
    private GameObject colorPickCanvas;
    [SerializeField]
    private GameObject netSelectionCanvas;

    [SerializeField]
    private string useless;

    [SerializeField]
    private NetColorSelect colorSelect;
    [SerializeField]
    private NetPlayerCreator creator;

    public bool IsNetworking;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        NetEventSystem.Instance.OnMakeNewPlayer += TurnOnColorPick;
    }

    private void OnDestroy()
    {
        // Prevent error in the editor
        if (NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void Host()
    {
        // Hook up password approval check
        noHostRunningMessage.SetActive(false);
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        IsNetworking = true;
    }

    public void Client()
    {
        // Set password ready to send to the server to validate
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInputField.text);
        NetworkManager.Singleton.StartClient();
        noHostRunningMessage.SetActive(true);
        IsNetworking = true;
    }
    public void Single()
    {
        noHostRunningMessage.SetActive(false);
        passwordEntryUI.SetActive(false);
        leaveButton.SetActive(true);
        colorPickCanvas.SetActive(true);
        IsNetworking = false;
    }
    public void StartGame()
    {
        startButton.SetActive(false);
        while (colorSelect.orderOfColors.Count > 0)
        {
            byte colorIndex = colorSelect.orderOfColors.Dequeue();
            ulong clientId = colorSelect.orderOfPlayers.Dequeue();
            creator.NewPlayer(colorIndex, clientId);
        }
        StartGameServerRpc();
    }
    [ServerRpc]
    private void StartGameServerRpc()
    {
        StartGameClientRpc();
    }
    [ClientRpc]
    private void StartGameClientRpc()
    {
        colorPickCanvas.SetActive(false);
        netSelectionCanvas.SetActive(false);
        NetEventSystem.Instance.GameStarted();
    }
    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }

        passwordEntryUI.SetActive(true);
        leaveButton.SetActive(false);
        colorPickCanvas.SetActive(false);
        netSelectionCanvas.SetActive(false);
        if (!IsNetworking)
        { 
            NetEventSystem.Instance.ResetSingle();
        }
    }

    private void HandleServerStarted()
    {
        // Temporary workaround to treat host as client
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.ServerClientId);
            startButton.SetActive(true);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Are we the client that is connecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(false);
            leaveButton.SetActive(true);
            colorPickCanvas.SetActive(true);
            netSelectionCanvas.SetActive(true);
            noHostRunningMessage.SetActive(false);
        }

    }
    private void HandleClientDisconnect(ulong clientId)
    {
        // Are we the client that is disconnecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            colorPickCanvas.SetActive(false);
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        string password = Encoding.ASCII.GetString(connectionData);
        bool approveConnection = password == passwordInputField.text;
        callback(true, null, approveConnection, Vector3.zero, Quaternion.identity);
    }
    public void ColorPicked()
    {
        colorPickCanvas.SetActive(false);
    }

    private void TurnOnColorPick()
    {
        colorPickCanvas.SetActive(true);
    }
}
