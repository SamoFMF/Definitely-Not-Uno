using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    //public enum CardColor { Red, Green, Blue, Yellow, Wild };
    //public enum CardValue { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Skip, Reverse, TakeTwo, ChangeColor, TakeFour };

    public CardColor Color;
    public CardValue ValueType;
    public int Value { get { return (int)ValueType; } }
    public int Id { get { return (int)Color * 100 + Value; } }

    public Card(CardColor color, CardValue value)
    {
        Color = color;
        ValueType = value;
    }

    public override string ToString()
    {
        return '(' + Color.ToString() + ", " + Value + ')';
    }
}
