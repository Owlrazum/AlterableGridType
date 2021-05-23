using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceObjects : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;

    List<Vector3> positions;

    void Start()
    {
        EventSystemPrev.Instance.OnPositionsReady += ReceivePositions;
        PlayerInputPrev.Instance.OnPlacementConfirmed += InstantiateObjects;
    }

    void ReceivePositions(List<Vector3> sendedPositions)
    {
        positions = sendedPositions;
    }

    void InstantiateObjects()
    {
        GameObject grid = new GameObject();
        grid.name = "Grid";
        Quaternion rot = Quaternion.Euler(0, 0, 0);
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject obj = Instantiate(prefab, positions[i], rot, grid.transform);
            DontDestroyOnLoad(obj);
        }
    }
}
