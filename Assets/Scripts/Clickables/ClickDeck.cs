using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDeck : Clickable
{
    public Player[] Players;
    private int curPlayer = 0;

    public override void OnPointerClick(PointerEventData eventData)
    {
        // TODO - get player etc.
        Players[curPlayer].MakeMove(Constants.DrawCardMove);
        curPlayer++;
        curPlayer %= Players.Length;
    }
}
