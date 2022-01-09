using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverColor : Hoverable
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        SetLocalScale(new Vector3()
        {
            x = 2,
            y = 2,
            z = 2
        });

        transform.SetAsLastSibling();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        SetLocalScale();
    }

    public void SetLocalScale(Vector3? vec = null)
    {
        transform.localScale = vec ?? new Vector3() { x = 1, y = 1, z = 1 };
    }
}
