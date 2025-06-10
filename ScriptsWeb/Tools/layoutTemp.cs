using UnityEngine;
using UnityEngine.UI;

public class layoutTemp : MonoBehaviour
{

    public Vector2Int horizontalLayoutSize, verticalLayoutSize;
    public float threshold = 1200f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    //private void OnValidate() => MakeLayout();


    void Update() => MakeLayout();
    void OnRectTransformDimensionsChange() => MakeLayout();
    void MakeLayout() 
    {
        try
        {

            Rect canvasRect = GetComponentInParent<RectTransform>().rect;
            Vector2 canvasSize = new(canvasRect.width, canvasRect.height);

            GetComponent<GridLayoutGroup>().cellSize = canvasSize.x > threshold ? 
                new(canvasSize.x / horizontalLayoutSize.x, canvasSize.y / horizontalLayoutSize.y) : 
                new(canvasSize.x / verticalLayoutSize.x, canvasSize.y / verticalLayoutSize.y);
        }
        catch { }
    }
}
