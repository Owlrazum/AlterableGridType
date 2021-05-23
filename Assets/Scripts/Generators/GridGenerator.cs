using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance;
    private void Awake()
    {
        Instance = this;
        generatedMaterialsIntensity = new Dictionary<(byte, byte), Material[]>();
    }
    public Transform parentForTiles;
    [SerializeField]
    private int numberOfRows; 
    [SerializeField]
    private int numberOfColumns; // assuming it is odd for RefLines
    [SerializeField]
    private float widthBetween; // assuming it is constant for RefLines
    [SerializeField]
    private float heightBetwRows; // too
    [SerializeField]
    private float heightBetwAdjacent; // too
    [SerializeField]
    private GameObject tile;

    [SerializeField]
    private Material mainTri;
    [SerializeField]
    private Material mainSqr;
    [SerializeField]
    private Material mainHex;
    [SerializeField]
    private Color color1;
    [SerializeField]
    private Color color2;
    [SerializeField]
    private Color color3;
    [SerializeField]
    private Color color4;

    //private List<List<GameObject>> tiles;

    public List<List<NetTile>> GenerateTiles()
    {
        List<List<NetTile>> tiles = new List<List<NetTile>>();
        Vector3 pos = new Vector3(-widthBetween * (numberOfColumns - 1) / 2.0f, 0, -heightBetwRows * (numberOfRows - 1) / 2.0f);
        float startPosWidth = pos.x;
        Quaternion rotationUp = Quaternion.Euler(new Vector3(0, 90, 0));
        Quaternion rotationDown = Quaternion.Euler(new Vector3(0, 270, 0));
        Quaternion rot;
        NetTile.OrientType orient = NetTile.OrientType.Up;
        for (int row = 0; row < numberOfRows; row++)
        {
            tiles.Add(new List<NetTile>());
            for (int col = 0; col < numberOfColumns; col++)
            {
                rot = orient == NetTile.OrientType.Up ? rotationUp : rotationDown;
                GameObject tileGenerated = Instantiate(tile, pos, rot, parentForTiles);
                NetTile tileClassObject = tileGenerated.GetComponent<NetTile>();
                tileGenerated.GetComponent<NetTile>().Listen();
                NetEventSystem.Instance.TileCreated(row, col, orient);
                tiles[row].Add(tileClassObject);
                //tileGenerated.layer = 8;
                pos.x += widthBetween;
                if (orient == NetTile.OrientType.Up)
                {
                    pos.z += heightBetwAdjacent;
                    orient = NetTile.OrientType.Down;
                }
                else
                {
                    pos.z -= heightBetwAdjacent;
                    orient = NetTile.OrientType.Up;
                }
            }
            pos.x = startPosWidth;
            if (numberOfColumns % 2 == 1)
            {
                float dist = orient == NetTile.OrientType.Up ? -heightBetwAdjacent : +heightBetwAdjacent;
                pos.z += heightBetwRows;
                //orient = orient == NetTile.OrientType.Up ? NetTile.OrientType.Down : NetTile.OrientType.Up;
            }
            else
            {
                float dist = orient == NetTile.OrientType.Up ? heightBetwAdjacent : -heightBetwAdjacent;
                pos.z += heightBetwRows + dist;
                orient = orient == NetTile.OrientType.Up ? NetTile.OrientType.Down : NetTile.OrientType .Up;
            }
        }
        return tiles;
    }

    public Material[] GetMaterials(byte colorIndex)
    {
        Material[] mats = new Material[3];
        mats[0] = new Material(mainTri);
        mats[1] = new Material(mainSqr);
        mats[2] = new Material(mainHex);
        switch (colorIndex)
        {
            case 0:
                foreach (Material mat in mats)
                {
                    mat.color = color1;
                }
                break;
            case 1:
                foreach (Material mat in mats)
                {
                    mat.color = color2;
                }
                break;
            case 2:
                foreach (Material mat in mats)
                {
                    mat.color = color3;
                }
                break;
            case 3:
                foreach (Material mat in mats)
                {
                    mat.color = color4;
                }
                break;
        }
        return mats;
    }

    private Dictionary<(byte, byte), Material[]> generatedMaterialsIntensity;

    public Material[] GetMaterials(byte colorIndex, byte intensity)
    {
        if (generatedMaterialsIntensity.ContainsKey((colorIndex, intensity)))
        {
            return generatedMaterialsIntensity[(colorIndex, intensity)];
        }
        Material[] mats = new Material[3];
        mats[0] = new Material(mainTri);
        mats[1] = new Material(mainSqr);
        mats[2] = new Material(mainHex);
        float intensityFactor;
        switch (intensity)
        {
            case 5:
                intensityFactor = 0.9f;
                break;
            case 4:
                intensityFactor = 0.6f;
                break;
            case 3:
                intensityFactor = 0.5f;
                break;
            case 2:
                intensityFactor = 0.4f;
                break;
            case 1:
                intensityFactor = 0.3f;
                break;
            default:
                intensityFactor = 0;
                break;
        }
        switch (colorIndex)
        {
            case 0:
                foreach (Material mat in mats)
                {
                    Color newColor = color1;
                    newColor.r *= intensityFactor;
                    newColor.g *= intensityFactor;
                    newColor.b *= intensityFactor;
                    mat.color = newColor;
                }
                break;
            case 1:
                foreach (Material mat in mats)
                {
                    Color newColor = color2;
                    newColor.r *= intensityFactor;
                    newColor.g *= intensityFactor;
                    newColor.b *= intensityFactor;
                    mat.color = newColor;
                }
                break;
            case 2:
                foreach (Material mat in mats)
                {
                    Color newColor = color3;
                    newColor.r *= intensityFactor;
                    newColor.g *= intensityFactor;
                    newColor.b *= intensityFactor;
                    mat.color = newColor;
                }
                break;
            case 3:
                foreach (Material mat in mats)
                {
                    Color newColor = color4;
                    newColor.r *= intensityFactor;
                    newColor.g *= intensityFactor;
                    newColor.b *= intensityFactor;
                    mat.color = newColor;
                }
                break;
        }
        TileMaterial.Instance.AddMaterials(mats);
        generatedMaterialsIntensity.Add((colorIndex, intensity), mats);
        return mats;
    }
}


// scale 15 1 0.1

// horiz:
// row : 1.825
// angled:
// angle: 57 ++ -- 
// col : 2.4

// reflines "\" are the first
// 5 7 7

