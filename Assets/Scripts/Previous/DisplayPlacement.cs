using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPlacement : MonoBehaviour
{
    [SerializeField]
    private GameObject placementObject;

    private GameObject placementDisplay;

    private void Start()
    {
        placementDisplay = new GameObject();
        EventSystemPrev.Instance.OnPositionsReady += DisplayPlaces;
        PlayerInputPrev.Instance.OnPlacementConfirmed += UndoDisplay;
        PlayerInputPrev.Instance.OnPlacementRejected += UndoDisplay;
    }
    private void DisplayPlaces(List<Vector3> positions)
    {
        placementDisplay.name = "placementDisplay";
        for (int i = 0; i < positions.Count; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, 0);
            Instantiate(placementObject, positions[i], rot, placementDisplay.transform);
        }
    }
    private void UndoDisplay()
    {
        Destroy(placementDisplay);
        placementDisplay = new GameObject();
    }

}
