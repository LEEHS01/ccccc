using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class TMGetTime : MonoBehaviour
{
    public TextMeshProUGUI text;

    public bool isHMSorYMD = true;
    
    void Update()
    {
        if(isHMSorYMD)
        text.text = System.DateTime.UtcNow.AddHours(9).ToString("yyyy/MM/dd HH:mm:ss");

    }
}
