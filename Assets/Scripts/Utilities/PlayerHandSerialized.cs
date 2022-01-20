using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public struct PlayerHandSerialized : INetworkSerializable
{
    public int[] PlayerHand;

    public PlayerHandSerialized(List<Card> playerHand)
    {
        PlayerHand = new int[playerHand.Count];
        for (int i = 0; i < playerHand.Count; i++)
            PlayerHand[i] = playerHand[i].Id;
    }

    public List<int> ArrayToList()
    {
        List<int> asList = new List<int>();
        for (int i = 0; i < PlayerHand.Length; i++)
            asList.Add(PlayerHand[i]);
        return asList;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;
        if (!serializer.IsReader)
        {
            length = PlayerHand.Length;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
            PlayerHand = new int[length];

        for (int i = 0; i < length; i++)
            serializer.SerializeValue(ref PlayerHand[i]);
    }
}
