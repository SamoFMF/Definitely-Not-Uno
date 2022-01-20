using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerOld : MonoBehaviour
{
    public GameObject[] PlayerHands;
    public GameObject CardPrefab;
    public PlayerScriptable[] Players;
    public GameObject LastPlayedCard;
    public GameObject Deck;
    public GameObject ChooseColor;
    public GameObject OnTurnArrow;
    public GameObject[] DeclareLastCards;


    private ResourcesManager rm;
    private Logic GameLogic;
    private CardDisplay LastPlayedCardDisplay;
    private int[] Scores;

    // Start is called before the first frame update
    void Start()
    {
        rm = Resources.Load<ResourcesManager>("Resources Managers/ResourcesManager");
        rm.Init();
        //rm.GetCardPrefab(7, CardPrefab, Player1Hand.transform);
        //rm.GetCardPrefab(110, CardPrefab, Player1Hand.transform);
        //rm.GetCardPrefab(307, CardPrefab, Player1Hand.transform);
        //GameObject card = rm.GetCardPrefab(413, CardPrefab, Player1Hand.transform);
        //card.transform.SetSiblingIndex(1);

        LastPlayedCardDisplay = LastPlayedCard.GetComponent<CardDisplay>();
        ChooseColor.SetActive(false);

        // Create game
        GameLogic = new Logic(4, 0, new System.Random(), new LogicSettings());

        // TODO - temporary
        //Deck.GetComponent<ClickDeck>().Players = Players;
        //ClickColor clickColor = ChooseColor.GetComponent<ClickColor>();
        //for (int i = 0; i < 4; i++)
        //    ChooseColor.transform.GetChild(i).GetComponent<ClickColor>().Players = Players;
        //ChooseColor.GetComponent<ClickColor>().Players = Players;

        // Display cards
        for (int player = 0; player < 4; player++)
        {
            Players[player].GameManager = this;

            // DisplayPlayerCards(player);
        }

        NewGame();

        // Scoreboard
        Scores = new int[4];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame()
    {
        GameLogic.NewGame();

        for (int player = 0; player < GameLogic.NumPlayers; player++)
            UpdatePlayerHand(player);

        // Display last played
        UpdateLastPlayedCard();


        // Update arrow
        UpdateArrow(GameLogic.OnTurn, GameLogic.LastPlayedCard.Color);

        // Enable ChooseCard if needed
        if (GameLogic.LastPlayedCard.Color == CardColor.Wild)
            ChooseColor.SetActive(true);
        else
            ChooseColor.SetActive(false);
    }

    private void SetPlayerHandSpacing(int player)
    {
        if (Utils.Mod(player, 2) == 1)
            return; // TODO - temporary fix

        int numCards = Mathf.Min(GameLogic.PlayerHands[player].Count, 12);

        GridLayoutGroup grid = PlayerHands[player].GetComponent<GridLayoutGroup>();

        grid.spacing = new Vector2()
        {
            x = Mathf.Min(10f, (760f - 100 * numCards) / (numCards - 1)),
            y = grid.spacing.y
        };
    }

    private void DisplayPlayerCards(int player)
    {
        foreach (Card card in GameLogic.PlayerHands[player])
        {
            //_ = rm.GetCardPrefab(card.Id, CardPrefab, PlayerHands[player].transform, Players[player]);
        }

        SetPlayerHandSpacing(player);
    }

    private void DestroyPlayerCard(int player, int cardIdx)
    {
        Destroy(PlayerHands[player].transform.GetChild(cardIdx).gameObject);

        SetPlayerHandSpacing(player);
    }

    private void DestroyPlayerCards(int player)
    {
        for (int i = 0; i < PlayerHands[player].transform.childCount; i++)
            DestroyPlayerCard(player, i);
    }

    private void UpdatePlayerHand(int player)
    {
        DestroyPlayerCards(player);
        DisplayPlayerCards(player);
    }

    private void UpdateLastPlayedCard()
    {
        int cardId = GameLogic.LastPlayedCard.Id;
        rm.UpdateCardDisplay(LastPlayedCardDisplay, cardId);
    }
    
    private void SetArrowDisplay(CardColor cardColor)
    {
        rm.UpdateArrowDisplay(OnTurnArrow.GetComponent<ArrowDisplay>(), cardColor); 
    }

    private void SetArrowDirection(int player)
    {
        OnTurnArrow.transform.eulerAngles = new Vector3(
            OnTurnArrow.transform.eulerAngles.x,
            OnTurnArrow.transform.eulerAngles.y,
            -(player + 1) * 90
        );
    }

    private void UpdateArrow(int player, CardColor cardColor)
    {
        SetArrowDisplay(cardColor);
        SetArrowDirection(player);
    }

    private void UpdateScoreboard(int[] scores)
    {
        for (int player = 0; player < GameLogic.NumPlayers; player++)
        {
            Scores[player] += scores[player];
            Debug.Log("Player " + player + ": " + scores[player] + " -> " + Scores[player]);
        }
    }

    public void MakeMove(int player, int move)
    {
        Card card;
        if (move < 200)
            card = GameLogic.PlayerHands[player][move];
        else
            card = new Card(CardColor.Blue, CardValue.Zero);

        int moveModifiers = DeclareLastCards[player].GetComponent<Toggle>().isOn ? Constants.DeclareLastCardMove : 0;
        Debug.Log("Player move: " + (move + moveModifiers));
        MakeMoveResult moveResult = GameLogic.MakeMove(player, move + moveModifiers);
        if (moveResult.IsValid)
        {
            if (moveModifiers > 0)
                DeclareLastCards[player].GetComponent<Toggle>().isOn = false;

            Debug.Log("Move (" + player + "," + move + "," + card + ") is VALID");
            if (moveResult.MoveType == MoveType.PlayedCard)
            {
                //Destroy(PlayerHands[player].transform.GetChild(move).gameObject);
                DestroyPlayerCard(player, move);
                if (moveResult.EnableChooseColor)
                    ChooseColor.SetActive(true);
            }
            else if (moveResult.MoveType == MoveType.DrewCard) // Now handled in moveResult.RefreshHand
            {
                //DestroyPlayerCards(player);
                //DisplayPlayerCards(player);
            }
            else if (moveResult.MoveType == MoveType.ChoseColor)
            {
                ChooseColor.SetActive(false);
                foreach (HoverColor hoverColor in ChooseColor.GetComponentsInChildren<HoverColor>())
                    hoverColor.SetLocalScale();
                // UpdateArrow(GameLogic.OnTurn, GameLogic.CurColor);
            }

            if (moveResult.RefreshHand)
                UpdatePlayerHand(player);

            UpdateLastPlayedCard();

            if (moveResult.IsOver)
            {
                Debug.Log("Winner = " + moveResult.Winner);
                UpdateScoreboard(moveResult.Scores);

                NewGame();
            }
            else
            {
                //if (moveResult.MoveType == MoveType.ChoseColor)
                //    UpdateArrow(GameLogic.OnTurn, GameLogic.CurColor);
                //else
                //    UpdateArrow(GameLogic.OnTurn, GameLogic.LastPlayedCard.Color);
                UpdateArrow(GameLogic.OnTurn, GameLogic.CurColor);
            }
        }
        else
        {
            Debug.Log("Move (" + player + "," + move + "," + card + ") is NOT VALID");
        }
    }
}
