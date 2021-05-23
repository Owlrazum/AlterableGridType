using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Array2D : MyButton
{
    [SerializeField]
    int row = 0;
    [SerializeField]
    int col = 0;
    [SerializeField]
    int interval = 0;

    private void Start()
    {
        if (row > 50 || col > 50)
        {
            row = row > 50 ? 50 : row;
            col = col > 50 ? 50 : col;
        }
    }

    protected override void ButtonUse (Vector3 clickPos)
    {
        List<Vector3> positions = new List<Vector3>();
        float startCol = -col / 2 * interval + clickPos.x;
        float startRow = -row / 2 * interval + clickPos.z;
        Vector3 pos = new Vector3(startCol, clickPos.y, startRow);

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                positions.Add(pos);
                pos.x += interval;
            }
            pos.z += interval;
            pos.x = startCol;
        }
        EventSystemPrev.Instance.PositionsReady(positions);
    }
}
