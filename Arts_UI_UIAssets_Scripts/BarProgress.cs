 
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BarProgress : MonoBehaviour
{
    public List<Image> images;
    public TMP_Text obsName;

    private float totalCount = 8;

    public void Start()
    {
        
    }
    
    public void SetBarValue(Dictionary<int, int> values)
    {
        var cnt = 0f;
        for (int i = 0; i < images.Count; i++)
        {
            cnt = values.ContainsKey(i + 1) ? (float)values[i + 1] : 0f;
            images[i].DOFillAmount(cnt / totalCount, 1);
        }
    }

    public void SetMaxVlaue(float max)
    {
        this.totalCount = max;
    }
}
