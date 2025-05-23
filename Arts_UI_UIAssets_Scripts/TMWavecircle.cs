﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TMWavecircle : MonoBehaviour
{
    [Range(0, 100)]
    public float no1;

    public Transform wave;
    public Transform s, e;

    //public Text theText;
    public TextMeshProUGUI theText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePercent(no1);
    }

    void UpdatePercent(float f)
    {
        wave.position = s.position + (e.position - s.position) * f / 100;

        theText.text = Mathf.RoundToInt(f) + "%";
    }
}
