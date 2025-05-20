using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
internal class AlphaHitTestThresholdModulator : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    internal float alphaHitTestTreshold = 0.0f;
    
    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = alphaHitTestTreshold;
    }
}