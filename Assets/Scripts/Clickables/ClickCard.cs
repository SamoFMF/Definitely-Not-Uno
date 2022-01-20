using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCard : Clickable
{
    public GameManager GameManager;

    public override void OnPointerClick(PointerEventData eventData)
    {
        int move = transform.GetSiblingIndex();

        GameManager.MakeMove(move);
    }
}
