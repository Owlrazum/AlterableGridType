using UnityEngine;
using UnityEditor;

public class GenerateTriangle : MonoBehaviour
{
    [SerializeField]
    private float radius;
    [SerializeField]
    private float height;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = new Mesh();
        Vector3[] triangleVertices = new Vector3[6];
        Vector2[] uvs = new Vector2[6];
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = Mathf.PI / 2 - i * 2 * Mathf.PI / 3;
                Vector3 pos = new Vector3(Mathf.Cos(angle), j * height, Mathf.Sin(angle));
                triangleVertices[i + j * 3] = pos;
            }
        }
        uvs[0] = new Vector2(0.5f, 1);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 0);

        uvs[3] = new Vector2(0.5f, 1);
        uvs[4] = new Vector2(1, 0);
        uvs[5] = new Vector2(0, 0);

        mesh.vertices = triangleVertices;
        int[] triangleTriangles = { 3, 4, 5, 0, 2, 1, 0, 3, 2, 3, 5, 2, 3, 0, 1, 3, 1, 4, 2, 5, 1, 5, 4, 1};
        mesh.triangles = triangleTriangles;
        mesh.RecalculateNormals();
        mesh.uv = uvs;

        //AssetDatabase.CreateAsset(mesh, "Assets/Meshes/Triangle.asset");
       // AssetDatabase.SaveAssets();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

}
