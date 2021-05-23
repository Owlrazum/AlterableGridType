using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefLine : MonoBehaviour
{
    public enum AngleOfLine
    {
        Horizontal,
        TopLeft,
        BottomLeft
    }
    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material selectedMaterial;
    [SerializeField]
    private Renderer renderer;

    private int index;
    public int Index { get; set; }
     
    private AngleOfLine angleOfLine;

    public AngleOfLine Angle
    {
        get; set;
    }

    Vector3 yDeltaForSelection = new Vector3(0, 0.01f, 0);

    private bool isDefault;
    public bool IsDefault
    {
        get
        {
            return isDefault;
        }
        set
        {
            if (value)
            {
                renderer.material = defaultMaterial;
                transform.position -= yDeltaForSelection;
            }
            isDefault = value;
        }
    }

    private bool isSelected;
    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            if (value)
            {
                renderer.material = selectedMaterial;
                transform.position += yDeltaForSelection;
            }
            isSelected = value;
        }
    }
}
