using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Arrow", menuName = "Arrow")]
public class ArrowScriptable : ScriptableObject
{
    public CardColor Color;
    public Sprite Art;

    public ArrowScriptable(CardColor color)
    {
        Color = color;
    }

    public override string ToString()
    {
        return '>' + Color.ToString() + '>';
    }
} 
