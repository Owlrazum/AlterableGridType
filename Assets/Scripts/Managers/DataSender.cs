using System;
using System.Collections.Generic;
using UnityEngine;

public class DataSender : MonoBehaviour
{
    public static DataSender Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField]
    private Vector2 firstVerticePosUp;
    [SerializeField]
    private Vector2 secondVerticePosUp;
    [SerializeField]
    private Vector2 thirdVerticePosUp;

    [SerializeField]
    private Vector2 firstVerticePosDown;
    [SerializeField]
    private Vector2 secondVerticePosDown;
    [SerializeField]
    private Vector2 thirdVerticePosDown;

    public (Vector2, Vector2, Vector2) GetLocalVerticesUp()
    {
        return (firstVerticePosUp, secondVerticePosUp, thirdVerticePosUp);
    }

    public (Vector2, Vector2, Vector2) GetLocalVerticesDown()
    {
        return (firstVerticePosDown, secondVerticePosDown, thirdVerticePosDown);
    }

    public event Action<List<List<Tile>>, List<List<RefLine>>, (int, int)> OnSendedContainers;
    public void SendContainers(List<List<Tile>> tiles, List<List<RefLine>> reflines, (int, int) dimensions)
    {
        OnSendedContainers?.Invoke(tiles, reflines, dimensions);
    }
}
