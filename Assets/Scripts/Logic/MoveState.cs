using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType { PlayedCard, ChoseColor, DrewCard, EndedTurn, DeclaredLastCard };

public class MakeMoveResult
{
    public bool IsValid, IsOver, EnableChooseColor, RefreshHand;
    public int NextPlayer, Winner;
    public int[] Scores;
    public MoveType MoveType;

    public MakeMoveResult(bool isValid, MoveType moveType, bool isOver = false, bool enableChooseColor = false, bool refreshHand = false, int nextPlayer = -1, int winner = -1, int[] scores = null)
    {
        IsValid = isValid;
        MoveType = moveType;
        IsOver = isOver;
        EnableChooseColor = enableChooseColor;
        NextPlayer = nextPlayer;
        Winner = winner;
        RefreshHand = refreshHand || (moveType == MoveType.DrewCard);
        Scores = scores;
    }
}
