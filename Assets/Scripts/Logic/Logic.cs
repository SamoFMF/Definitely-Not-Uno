using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Logic
{
    private Random Rng;
    private int NumStartCards;
    private int NumIlegalMoveDraw;

    // Variables
    private List<Card> Deck;
    public List<Card>[] PlayerHands;
    public List<Card> PlayedCards;
    public Card LastPlayedCard;
    public int NumPlayers, Dealer, OnTurn, Delta, NumToDraw;
    public int[] Scores;
    public bool ChooseColor;
    public bool[] DeclaredUno;
    private int BlockedPlayer;
    private bool AlreadyDrewCard;

    public Logic(int numPlayers, int dealer, Random rng, LogicSettings logicSettings)
    {
        NumPlayers = numPlayers;
        Dealer = dealer;
        Rng = rng;
        NumStartCards = logicSettings.NumStartCards;
        NumIlegalMoveDraw = logicSettings.NumIlegalMoveDraw;

        SetupDeck();
        NewGame();
    }

    private void NewGame()
    {
        PlayerHands = new List<Card>[NumPlayers];
        for (int i = 0; i < NumPlayers; i++)
            PlayerHands[i] = new List<Card>();
        DeclaredUno = new bool[NumPlayers];

        OnTurn = Dealer;
        AlreadyDrewCard = false;
        Delta = 1;
        NumToDraw = 0;
        ChooseColor = false;
        BlockedPlayer = -1;

        PlayedCards = new List<Card>();
        ShuffleDeck();
        DealCards();

        // First (forced) move
        LastPlayedCard = null;
        Card card = Deck[Deck.Count - 1];
        DrawCards(OnTurn, 1);
        int move = PlayerHands[OnTurn].FindIndex(x => x.Color == card.Color && x.ValueType == card.ValueType);
        MakeMove(OnTurn, move);
        PlayedCards.RemoveAt(0); // Remove null from PlayedCards
    }

    private void SetupDeck()
    {
        Deck = new List<Card>();
        int repeat;
        foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
        {
            foreach (CardValue value in System.Enum.GetValues(typeof(CardValue)))
            {
                if (color == CardColor.Wild)
                {
                    if (value == CardValue.ChangeColor)
                        repeat = 8;
                    else if (value == CardValue.TakeFour)
                        repeat = 4;
                    else
                        repeat = 0;
                }
                else if (value == CardValue.Zero)
                    repeat = 1;
                else if (value != CardValue.ChangeColor && value != CardValue.TakeFour)
                    repeat = 2;
                else
                    repeat = 0;
                for (int i = 0; i < repeat; i++)
                    Deck.Add(new Card(color, value));
            }
        }
        Debug.Log("Num of cards in deck: " + Deck.Count);
    }

    /*
         * Use Fisher-Yates method to shuffle deck
         * */
    private void ShuffleDeck()
    {
        int j;
        Card temp;
        for (int i = 0; i < Deck.Count - 1; i++)
        {
            j = Rng.Next(i, Deck.Count);
            temp = Deck[j];
            Deck[j] = Deck[i];
            Deck[i] = temp;
        }
    }

    private void PlayedToDeck()
    {
        Deck = PlayedCards;
        PlayedCards = new List<Card>();
        ShuffleDeck();
    }

    private void SortHand(int player)
    {
        PlayerHands[player] = PlayerHands[player].OrderBy(x => (int)(x.Color)).ThenBy(x => x.Value).ToList();
    }

    private void DrawCards(int player, int numCards)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (Deck.Count == 0)
                PlayedToDeck();

            PlayerHands[player].Add(Deck[Deck.Count - 1]);
            Deck.RemoveAt(Deck.Count - 1);
        }
        SortHand(player);
    }

    private void DealCards()
    {
        for (int i = 0; i < NumPlayers; i++)
            DrawCards(i, NumStartCards);
    }

    private int GetPlayerScore(int player)
    {
        int score = 0;
        foreach (Card card in PlayerHands[player])
            if (card.Color == CardColor.Wild)
                score += 50;
            else if (card.Value < 10)
                score += card.Value;
            else
                score += 20;
        return score;
    }

    private MakeMoveResult EndGame(int winner)
    {
        int[] scores = new int[NumPlayers];
        for (int player = 0; player < NumPlayers; player++)
            scores[player] = GetPlayerScore(player);

        return new MakeMoveResult(true, MoveType.PlayedCard)
        {
            IsOver = true,
            Winner = winner,
            Scores = scores
        };
    }

    private void ClearMove()
    {
        BlockedPlayer = -1;
        AlreadyDrewCard = false;
    }

    private bool IsLegalMove(int player, int move)
    {
        if (LastPlayedCard == null)
            return true;
        else if (player == BlockedPlayer || (ChooseColor && player != OnTurn) || (ChooseColor && (move < Constants.ChooseColorMove || move >= Constants.ChooseColorMove + 4)))
        {
            return false;
        }
        else if (move == Constants.DrawCardMove)
        {
            // Draw card
            return player == OnTurn;
        }
        else if (move == Constants.DeclareLastCardMove)
        {
            // Declare last card
            return player == OnTurn && !DeclaredUno[player];
        }
        else if (move >= Constants.ChooseColorMove && move < Constants.ChooseColorMove+4)
        {
            // Choose color
            return ChooseColor && player == OnTurn;
        }
        else
        {
            Card card = PlayerHands[player][move];
            if (OnTurn == player)
            {
                if (NumToDraw > 0)
                    return card.Value == LastPlayedCard.Value;
                else
                    return card.Color == LastPlayedCard.Color || card.Value == LastPlayedCard.Value || card.Color == CardColor.Wild;
            }
            else
            {
                return card.Color == LastPlayedCard.Color && card.Value == LastPlayedCard.Value;
            }
        }
    }

    public MakeMoveResult MakeMove(int player, int move)
    {
        if (IsLegalMove(player, move))
        {
            MoveType moveType;
            if (move == Constants.DrawCardMove)
            {
                if (AlreadyDrewCard)
                {
                    // End turn (already drew card(s) this turn)
                    ClearMove();
                    OnTurn = Utils.Mod(player + Delta, NumPlayers);
                    moveType = MoveType.EndedTurn;
                }
                else
                {
                    // Draw card(s) from deck
                    if (NumToDraw > 0)
                    {
                        ClearMove();
                        BlockedPlayer = player;
                        DrawCards(player, NumToDraw);
                        NumToDraw = 0;
                        OnTurn = Utils.Mod(player + Delta, NumPlayers);
                    }
                    else
                    {
                        AlreadyDrewCard = true;
                        DrawCards(player, 1);
                    }
                    DeclaredUno[player] = false;
                    moveType = MoveType.DrewCard;
                }
                return new MakeMoveResult(true, moveType);
            }
            else if (move == Constants.PlayDoubleMove)
            {
                // TODO: Play 2 cards simultaneously
                return new MakeMoveResult(false, MoveType.PlayedCard);
            }
            else if (move == Constants.DeclareLastCardMove)
            {
                // Declare last card
                DeclaredUno[player] = true;
                return new MakeMoveResult(true, MoveType.DeclaredLastCard);
            }
            else if (move >= Constants.ChooseColorMove && move < Constants.ChooseColorMove + 4)
            {
                // Choose color
                ClearMove();
                LastPlayedCard = new Card((CardColor)(move - Constants.ChooseColorMove), LastPlayedCard.ValueType);
                ChooseColor = false;
                OnTurn = Utils.Mod(player + Delta, NumPlayers);
                return new MakeMoveResult(true, MoveType.ChoseColor);
            }
            else
            {
                // Play card
                ClearMove();
                int deltaMult = 1;
                Card card = PlayerHands[player][move];
                PlayerHands[player].RemoveAt(move);
                PlayedCards.Add(LastPlayedCard);
                LastPlayedCard = card;

                if (PlayerHands[player].Count == 0)
                {
                    // Game Over: Player Wins
                    return EndGame(player);
                    //return new MakeMoveResult(true, MoveType.PlayedCard)
                    //{
                    //    IsOver = true,
                    //    Winner = player
                    //};
                }
                else if (card.ValueType == CardValue.Skip)
                {
                    // Stop / Skip
                    BlockedPlayer = Utils.Mod(player + Delta, NumPlayers);
                    deltaMult = 2;
                }
                else if (card.ValueType == CardValue.Reverse)
                {
                    // Reverse / Change direction
                    Delta *= -1;
                }
                else if (card.ValueType == CardValue.TakeTwo)
                {
                    // Take two
                    NumToDraw += 2;
                }
                else if (card.ValueType == CardValue.ChangeColor)
                {
                    // Change color
                    ChooseColor = true;
                    deltaMult = 0;
                }
                else if (card.ValueType == CardValue.TakeFour)
                {
                    // Take four
                    NumToDraw += 4;
                    ChooseColor = true;
                    deltaMult = 0;
                }
                OnTurn = Utils.Mod(player + deltaMult * Delta, NumPlayers);

                bool refreshHand = false;
                if ((DeclaredUno[player] && PlayerHands[player].Count != 1) || (!DeclaredUno[player] && PlayerHands[player].Count == 1))
                {
                    DrawCards(player, NumIlegalMoveDraw);
                    refreshHand = true;
                }
                return new MakeMoveResult(true, MoveType.PlayedCard)
                {
                    EnableChooseColor = ChooseColor,
                    RefreshHand = refreshHand
                };
            }
        }
        else
        {
            return new MakeMoveResult(false, MoveType.PlayedCard);
        }
    }
}
