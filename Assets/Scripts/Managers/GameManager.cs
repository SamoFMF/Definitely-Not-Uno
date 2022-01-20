using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject ChooseColor;
    public GameObject OnTurnArrow;
    public GameObject DeclareLastCard;
    public ServerManager ServerManager = null;
    public GameObject PlayerHand;
    public GameObject NewGameButton;
    public GameObject GameScreen;

    // Prefabs
    public GameObject CardPrefab;
    public GameObject OpponentHandPrefab;
    public GameObject PlayerHandPrefab;

    private ResourcesManager rm;
    private CardDisplay LastPlayedCardDisplay;
    private bool IsServer; // Override NetworkBehaviour.IsServer
    private Player LocalPlayer = null;

    // Game related
    private Dictionary<ulong, Player> PlayersInGame;
    private Dictionary<ulong, GameObject> OpponentHands;
    private int PlayerPos;

    private Card LastPlayedCard;

    public void Init(string mode)
    {
        IsServer = mode == "Server";

        if (IsServer)
        {
            Debug.Log("Init Game Manager: Server");
        }
        else
        {
            Debug.Log("Init Game Manager: Client");
            rm = Resources.Load<ResourcesManager>("Resources Managers/ResourcesManager");
            rm.Init();

            LastPlayedCardDisplay = GameObject.Find("LastPlayedCard").GetComponent<CardDisplay>();
            ChooseColor.SetActive(false);

            PlayersInGame = new Dictionary<ulong, Player>();
        }
    }

    public void SetupServerManager(ServerManager serverManager)
    {
        ServerManager = serverManager;
        serverManager.GameManager = this;

        LastPlayedCard = ServerManager.LastPlayedCard.Value;

        // Add server to LocalPlayer if it is already added
        if (LocalPlayer != null) LocalPlayer.ServerManager = ServerManager;
    }

    public void OnDestroy()
    {
        if (IsServer)
        {

        }
        else
        {
            // Unsubscribe from network variable changes
            //LastPlayedCard.OnValueChanged -= UpdateLastPlayedCard;
            //OnTurn.OnValueChanged -= UpdateOnTurnChanged;
        }
    }

    public void UpdateChooseColor(bool previousValue, bool newValue)
    {
        ChooseColor.SetActive(newValue);
    }

    private void UpdateOnTurnChanged(int previousValue, int newValue)
    {
        int numPlayers = ServerManager.ConnectedPlayerObjects.Count;
        int player = Utils.Mod(newValue - PlayerPos, numPlayers);

        SetArrowDirection(player, numPlayers);
    }

    private void UpdateLastPlayedCard(Card previousCard, Card newCard)
    {
        LastPlayedCard = newCard;
        rm.UpdateCardDisplay(LastPlayedCardDisplay, newCard.Id);
    }

    private void SetArrowDisplay(CardColor previousValue, CardColor newValue)
    {
        rm.UpdateArrowDisplay(OnTurnArrow.GetComponent<ArrowDisplay>(), newValue);
    }

    private void SetArrowDirection(int player, int numPlayers)
    {
        OnTurnArrow.transform.eulerAngles = new Vector3(
            OnTurnArrow.transform.eulerAngles.x,
            OnTurnArrow.transform.eulerAngles.y,
            -90 - player * (360 / numPlayers)
        );
    }

    public void StartGameClick()
    {
        NewGameButton.SetActive(false);

        ServerManager.StartGameServerRpc();
    }

    public void NewGame(ulong localId)
    {
        // Subscribe to Server variable changes
        ServerManager.LastPlayedCard.OnValueChanged += UpdateLastPlayedCard;
        ServerManager.OnTurn.OnValueChanged += UpdateOnTurnChanged;
        ServerManager.CurrentColor.OnValueChanged += SetArrowDisplay;

        // Subscribe to Local player variable changes
        LocalPlayer.NumCardsInHand.OnValueChanged += UpdateNumCardsPlayer;
        LocalPlayer.Score.OnValueChanged += UpdateScore;
        LocalPlayer.ChooseColor.OnValueChanged += UpdateChooseColor;

        // Set local player position
        PlayerPos = ServerManager.PlayerPositions[localId];

        ulong playerId;
        Player player;
        NetworkObject networkObject;
        OpponentHands = new Dictionary<ulong, GameObject>();
        for (int i = 0; i < ServerManager.ConnectedPlayerObjects.Count; i++)
        {
            networkObject = ServerManager.ConnectedPlayerObjects[i];
            playerId = networkObject.OwnerClientId;
            if (playerId == localId)
                continue;

            player = networkObject.GetComponent<Player>();

            player.Score.OnValueChanged += UpdateScore;

            AddOpponentHand(player, playerId);
        }

        //foreach (KeyValuePair<ulong, Player> entry in PlayersInGame)
        //{
        //    clientId = entry.Key;
        //    if (clientId == localId)
        //        continue;

        //    player = entry.Value;

        //    // Subscribe to opponents variable changes
        //    player.Score.OnValueChanged += UpdateScore;

        //    AddOpponentHand(player, clientId);
        //}
    }

    public void NewRound()
    {
        // Set starting values
        UpdateLastPlayedCard(ServerManager.LastPlayedCard.Value, ServerManager.LastPlayedCard.Value);
        UpdateOnTurnChanged(ServerManager.OnTurn.Value, ServerManager.OnTurn.Value);
        SetArrowDisplay(ServerManager.CurrentColor.Value, ServerManager.CurrentColor.Value);
    }

    private void AddOpponentHand(Player player, ulong playerId)
    {
        int opponentPos = ServerManager.PlayerPositions[playerId];
        int numOpponents = ServerManager.ConnectedPlayerObjects.Count - 1;
        // Vector3 pos = GetOpponentPosition(Utils.Mod(opponentPos - playerPos, numOpponents), ServerManager.ConnectedPlayerIds.Count - 1);

        GameObject opponentHand = Instantiate(OpponentHandPrefab, GameScreen.transform);
        opponentHand.transform.localPosition = GetOpponentPosition(opponentPos > PlayerPos ? opponentPos - PlayerPos - 1 : Utils.Mod(opponentPos - PlayerPos, numOpponents),
            ServerManager.ConnectedPlayerObjects.Count - 1);

        OpponentHands.Add(playerId, opponentHand);

        opponentHand.GetComponent<OpponentHandDisplay>().SubscribeToUpdates(player);
    }

    private Vector3 GetOpponentPosition(int pos, int numOpponents)
    {
        int x = 0;
        int y = 0;
        if (numOpponents == 1)
        {
            x = 0;
            y = 250;
        }
        else if (numOpponents == 2)
        {
            x = (pos * 2 - 1) * 550;
            y = 250;
        }
        else if (numOpponents == 3)
        {
            x = (pos - 1) * 550;
            y = pos == 1 ? 250 : 0;
        }
        // TODO: more opponents

        return new Vector3(x, y, 0);
    }

    //public void AddOpponent(Player opponent)
    //{
    //    ulong opponentId = opponent.OwnerClientId;

    //    PlayersInGame.Add(opponentId, opponent);
    //}

    private void UpdateScore(int previousValue, int newValue)
    {
        throw new NotImplementedException();
    }

    private void UpdateNumCardsPlayer(int previousValue, int newValue)
    {
        LocalPlayer.RequestPlayerHand();
    }

    public void OnConnectLocalPlayer(Player player)
    {
        LocalPlayer = player;
    }

    public void UpdateHand()
    {
        DestroyPlayerCards();
        DisplayPlayerCards();
    }

    private void DisplayPlayerCards()
    {
        int cardId;
        for (int i = 0; i < LocalPlayer.Hand.Count; i++)
        {
            cardId = LocalPlayer.Hand[i];
            _ = rm.GetCardPrefab(cardId, CardPrefab, PlayerHand.transform, this);
        }

        SetPlayerHandSpacing();
    }

    private void DestroyPlayerCard(int cardIdx)
    {
        Destroy(PlayerHand.transform.GetChild(cardIdx).gameObject);

        SetPlayerHandSpacing();
    }

    private void DestroyPlayerCards()
    {
        for (int i = 0; i < PlayerHand.transform.childCount; i++)
            DestroyPlayerCard(i);
    }

    private void SetPlayerHandSpacing()
    {
        int numCards = Mathf.Min(LocalPlayer.Hand.Count, 12);

        GridLayoutGroup grid = PlayerHand.GetComponent<GridLayoutGroup>();

        grid.spacing = new Vector2()
        {
            x = Mathf.Min(10f, (760f - 100 * numCards) / (numCards - 1)),
            y = grid.spacing.y
        };
    }

    public void MakeMove(int move)
    {
        int moveModifiers = DeclareLastCard.GetComponent<Toggle>().isOn ? Constants.DeclareLastCardMove : 0;

        LocalPlayer.MakeMove(move + moveModifiers);
    }

    public void MakeMoveResponse(MakeMoveResult moveResult)
    {
        if (moveResult.IsValid)
        {
            Toggle toggle = DeclareLastCard.GetComponent<Toggle>();
            if (toggle.isOn)
                toggle.isOn = false;

            if (moveResult.MoveType == MoveType.PlayedCard)
            {
                if (moveResult.EnableChooseColor)
                    ChooseColor.SetActive(true);
            }
            else if (moveResult.MoveType == MoveType.ChoseColor)
            {
                ChooseColor.SetActive(false);
                foreach (HoverColor hoverColor in ChooseColor.GetComponentsInChildren<HoverColor>())
                    hoverColor.SetLocalScale();
            }
            else if (moveResult.MoveType == MoveType.DrewCard)
            {

            }
            else if (moveResult.MoveType == MoveType.EndedTurn)
            {

            }

            if (moveResult.IsOver)
            {
                // TODO
            }
            else
            {
                // TODO (maybe update arrow?)
            }
        }
    }
}
