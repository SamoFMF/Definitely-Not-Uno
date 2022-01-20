using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum MoveType { PlayedCard, ChoseColor, DrewCard, EndedTurn, InvalidType };

public struct MakeMoveResult : INetworkSerializable
{
    public bool IsValid, IsOver, EnableChooseColor, RefreshHand;
    public int NextPlayer, Winner;
    public int[] Scores;
    public MoveType MoveType;

    public MakeMoveResult(bool isValid = false, MoveType moveType = MoveType.InvalidType, bool isOver = false, bool enableChooseColor = false, bool refreshHand = false, int nextPlayer = -1, int winner = -1, int[] scores = null)
    {
        IsValid = isValid;
        MoveType = moveType;
        IsOver = isOver;
        EnableChooseColor = enableChooseColor;
        NextPlayer = nextPlayer;
        Winner = winner;
        RefreshHand = refreshHand || (moveType == MoveType.DrewCard);
        if (scores == null)
            Scores = new int[0];
        else
            Scores = scores;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref IsValid);
        serializer.SerializeValue(ref MoveType);
        serializer.SerializeValue(ref IsOver);
        serializer.SerializeValue(ref EnableChooseColor);
        serializer.SerializeValue(ref RefreshHand);
        serializer.SerializeValue(ref NextPlayer);
        serializer.SerializeValue(ref Winner);


        // Scores:
        // Length
        int length = 0;
        if (!serializer.IsReader)
        {
            length = Scores.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            Scores = new int[length];
        }

        for (int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref Scores[i]);
        }
    }
}
