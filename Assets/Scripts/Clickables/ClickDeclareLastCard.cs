using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDeclareLastCard : Clickable
{
    public Player Player;

    public override void OnPointerClick(PointerEventData eventData)
    {
        // TODO - get player etc.
        Player.MakeMove(Constants.DeclareLastCardMove);
    }
}
