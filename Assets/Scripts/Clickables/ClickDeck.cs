using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDeck : Clickable
{
    public GameManager GameManager;

    public override void OnPointerClick(PointerEventData eventData)
    {
        // TODO - get player etc.
        GameManager.MakeMove(Constants.DrawCardMove);
    }
}
