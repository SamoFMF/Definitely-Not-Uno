using System;
using Unity.Netcode;

public struct Card : INetworkSerializable, IEquatable<Card>
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref ValueType);
    }

    public bool Equals(Card other)
    {
        return Color == other.Color && ValueType == other.ValueType;
    }
}
