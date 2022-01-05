using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickColor : Clickable
{
    public Player[] Players;
    public CardColor CardColor;
    private int curPlayer = 0;

    public override void OnPointerClick(PointerEventData eventData)
    {
        // TODO - get player etc.
        Players[curPlayer].MakeMove(Constants.ChooseColorMove + (int)CardColor);
        curPlayer++;
        curPlayer %= Players.Length;
    }
}
