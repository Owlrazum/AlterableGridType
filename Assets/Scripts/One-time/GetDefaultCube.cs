/*using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GetDefaultCube : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Mesh my = new Mesh();
        my.vertices = mesh.vertices;
        my.uv = mesh.uv;
        my.triangles = mesh.triangles;
        AssetDatabase.CreateAsset(my, "Assets/DefaultCube.fbx");
        AssetDatabase.SaveAssets();
    }

}
*/