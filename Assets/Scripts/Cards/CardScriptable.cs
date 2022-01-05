using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardScriptable : ScriptableObject
{
    //public enum CardColor { Red, Green, Blue, Yellow, Wild };
    //public enum CardValue { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Skip, Reverse, TakeTwo, ChangeColor, TakeFour };

    public CardColor Color;
    public CardValue ValueType;
    public int Id { get { return (int)Color * 100 + (int)ValueType; } }
    public Sprite Art;

    public CardScriptable(CardColor color, CardValue value)
    {
        Color = color;
        ValueType = value;
    }

    public override string ToString()
    {
        return '(' + Color.ToString() + ", " + ValueType.ToString() + ')';
    }
}
