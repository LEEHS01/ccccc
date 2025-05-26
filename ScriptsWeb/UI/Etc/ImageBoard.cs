using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class ImageBoard : MonoBehaviour
{
    // Public variable to set the alphaHitTestMinimumThreshold in the Unity Editor
    internal float alphaThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Image item in GetComponentsInChildren<Image>())
        {
            item.alphaHitTestMinimumThreshold = alphaThreshold;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
