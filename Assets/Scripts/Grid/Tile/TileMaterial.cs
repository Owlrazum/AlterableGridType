using MLAPI.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMaterial : MonoBehaviour
{
    [SerializeField]
    private List<Material> trianglePart;
    [SerializeField]
    private List<Material> squarePart;
    [SerializeField]
    private List<Material> hexPart;

    private float sqrAlpha;
    private float hexAlpha;

    public static TileMaterial Instance;

    private void Awake()
    {
        sqrAlpha = 0;
        hexAlpha = 0;
        Instance = this;
    }
    public void IncSqrAlpha()
    {
        sqrAlpha += 0.01f ;
        foreach (Material m in squarePart)
        {
            Color prev = m.color;
            prev.a = sqrAlpha;
            m.color = prev;
        }
    }
    public void DecSqrAlpha()
    {
        sqrAlpha -= 0.01f;
        foreach (Material m in squarePart)
        {
            Color prev = m.color;
            prev.a = sqrAlpha;
            m.color = prev;
        }
    }

    public void IncHexAlpha()
    {
        hexAlpha += 0.01f;
        foreach (Material m in hexPart)
        {
            Color prev = m.color;
            prev.a = hexAlpha;
            m.color = prev;
        }
    }

    public void DecHexAlpha()
    {
        hexAlpha -= 0.01f;
        foreach (Material m in hexPart)
        {
            Color prev = m.color;
            prev.a = hexAlpha;
            m.color = prev;
        }
    }

    public void AddMaterials(Material[] mats)
    {
        if (mats.Length != 3)
        {
            NetworkLog.LogWarningServer("mats size not equal to three");
            return;
        }
        trianglePart.Add(mats[0]);
        squarePart.Add(mats[1]);
        hexPart.Add(mats[2]);
    }
}
