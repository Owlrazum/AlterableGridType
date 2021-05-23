using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUse : MyButton
{
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    Vector3 scale;

    Vector3[] meshPositions;

    protected override void ButtonUse(Vector3 clickPos)
    {
        meshPositions = mesh.vertices;
        List<Vector3> positions = new List<Vector3>(meshPositions);
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 newPosition = positions[i];
            newPosition.x *= scale.x;
            newPosition.y *= scale.y;
            newPosition.z *= scale.z;
            newPosition.x += clickPos.x;
            newPosition.y += clickPos.y;
            newPosition.z += clickPos.z;
            positions[i] = newPosition;
        }
        EventSystemPrev.Instance.PositionsReady(positions);
    }


}
