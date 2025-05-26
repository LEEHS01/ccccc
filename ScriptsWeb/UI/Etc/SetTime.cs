using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

internal class SetTime : MonoBehaviour
{
    internal TextMeshProUGUI text;

    internal bool isHMSorYMD = true;
    
    internal void SetText(DateTime date)
    {
        if(isHMSorYMD)
            text.text = date.ToString("HH:mm:ss");
        else
            text.text = date.ToString("yyyy/MM/dd");

    }
}
