using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowDisplay : MonoBehaviour
{
    public ArrowScriptable Arrow;
    public Image Img;

    // Start is called before the first frame update
    void Start()
    {
        LoadArrow(Arrow);
    }

    public void LoadArrow(ArrowScriptable arrow)
    {
        if (arrow == null)
            return;

        Arrow = arrow;
        Img.sprite = arrow.Art;
    }
}
