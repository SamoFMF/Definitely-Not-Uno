using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public abstract void OnPointerEnter(PointerEventData eventData);

    public abstract void OnPointerExit(PointerEventData eventData);
}
