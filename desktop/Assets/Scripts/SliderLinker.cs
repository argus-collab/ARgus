using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLinker : MonoBehaviour
{
    public Slider input;
    public Slider output;

    
    public void UpdateValue()
    {
        output.value = input.value; 
    }
}
