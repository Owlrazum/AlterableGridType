using UnityEngine;
using UnityEngine.UI;

public class MyButton : MonoBehaviour
{
    public enum Type
    {
        Array2D,
        MeshUse
    }

    [SerializeField]
    private Sprite defaultImg;
    [SerializeField]
    private Sprite selectedImg;

    private Image buttonImage;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        buttonImage = button.GetComponent<Image>();
        button.onClick.AddListener(ButtonDown);
    }
    protected virtual void ButtonUse(Vector3 clickPos)
    {
        Debug.Log("Button Use");
    }

    private void ButtonDown() 
    {
        buttonImage.sprite = selectedImg;
        PlayerInputPrev.Instance.OnClickAfterButton += ProcessAfterClick;
        PlayerInputPrev.Instance.OnClickAfterButton += ButtonUse;
        PlayerInputPrev.Instance.PlacementRejected();
        PlayerInputPrev.Instance.ButtonDown();
    }
    private void ProcessAfterClick(Vector3 clickPos)
    {
        buttonImage.sprite = defaultImg;
        PlayerInputPrev.Instance.OnClickAfterButton -= ProcessAfterClick;
        PlayerInputPrev.Instance.OnClickAfterButton -= ButtonUse;
    }
}
