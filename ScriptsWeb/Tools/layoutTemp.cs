using UnityEngine;
using UnityEngine.UI;

public class layoutTemp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    void Update() => MakeLayout();
    //private void OnValidate() => MakeLayout();


    void OnRectTransformDimensionsChange() => MakeLayout();
    void MakeLayout() 
    {
        try
        {

            Rect canvasRect = GetComponentInParent<RectTransform>().rect;
            Vector2 canvasSize = new(canvasRect.width, canvasRect.height);

            GetComponent<GridLayoutGroup>().cellSize = canvasSize.x > 1200 ? new(canvasSize.x / 2, canvasSize.y / 3) : new(canvasSize.x / 1, canvasSize.y / 6);
        }
        catch { }
    }
}
