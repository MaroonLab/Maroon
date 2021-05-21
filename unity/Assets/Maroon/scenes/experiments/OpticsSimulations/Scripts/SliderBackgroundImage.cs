﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maroon.Physics;
using Maroon.UI;
//
// Author: Michael Holly 
//
// Modifications: Tobias Stöckl
//

[RequireComponent(typeof(QuantityPropertyView))]
public class SliderBackgroundImage : MonoBehaviour
{
    [SerializeField] private Sprite imageSource;

    //TODO this is not beautiful/efficient
    void Update()
    {
        UpdateSliderBackgroundImage(); 
    }

    public void UpdateSliderBackgroundImage()
    {
        var quantityPropertyView = GetComponent<QuantityPropertyView>();
        var quantity = quantityPropertyView.quantity.Value;

        if (!(quantity is QuantityInt) && !(quantity is QuantityFloat))
            return;

        var slider = gameObject.GetComponentInChildren<Slider>();
        var backgroundImage = slider.transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
        var fillImage = slider.transform.Find("Fill Area").transform.Find("Fill").GetComponent<UnityEngine.UI.Image>();
        //var handleImage = slider.transform.Find("Handle Slide Area").transform.Find("Handle").GetComponent<UnityEngine.UI.Image>();
        // -> set color of handle accordingly? maybe.
        fillImage.color = new Color(1, 1, 1, 0); //white, but transparent
        backgroundImage.sprite = imageSource;
    }
}