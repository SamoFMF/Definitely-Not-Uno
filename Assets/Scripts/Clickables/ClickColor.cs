using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickColor : Clickable
{
    public GameManager GameManager;
    public CardColor CardColor;

    public override void OnPointerClick(PointerEventData eventData)
    {
        GameManager.MakeMove(Constants.ChooseColorMove + (int)CardColor);
    }
}
