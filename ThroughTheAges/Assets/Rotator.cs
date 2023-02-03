using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private void Update()
    {
        var mousePos = Input.mousePosition;
        // normalize itz
        var screenPos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
        // sub 1 
        screenPos = screenPos - Vector3.one * 0.5f;

        transform.eulerAngles = new Vector3(screenPos.x * 10, screenPos.y * 10, 0);
        
    }
}
