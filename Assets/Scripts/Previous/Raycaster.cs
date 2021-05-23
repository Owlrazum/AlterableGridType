using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
    [SerializeField]
    private Camera camera; 
    public static Raycaster Instance;

    private int layerMask;
    private void Awake()
    {
        Instance = this;
        layerMask = 1 << 5; 
        layerMask = ~layerMask; // All except UI
    }

    public Vector3 ObtainClickPosition(Vector3 posScr) 
    {
        Vector3 clickPosition; 
        RaycastHit rayHit;
        Ray ray = camera.ScreenPointToRay(posScr);
        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMask))
        {
            clickPosition = rayHit.point;
            return clickPosition;
        }
        else
        {
            return new Vector3(Mathf.Infinity, 0, 0);
        }

    }

}
