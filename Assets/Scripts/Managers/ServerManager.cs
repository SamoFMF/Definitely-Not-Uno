using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class ServerManager : NetworkBehaviour
{
    public Dictionary<ulong, int> PlayerPositions;
    public Player LocalPlayer;
    public GameManager GameManager;

    private Logic GameLogic;
    private HashSet<ulong> JoinedPlayers;
    private bool GameStarted = false;
    private bool NotAcceptingMoves = true;
    private ClientRpcParams SendToJoinedPlayersClientParams;
    private Player[] ConnectedPlayers;
    private ulong[] ConnectedPlayerIds;

    public NetworkVariable<Card> LastPlayedCard = new NetworkVariable<Card>();
    public NetworkVariable<int> OnTurn = new NetworkVariable<int>();
    // public NetworkVariable<bool> ChooseColor = new NetworkVariable<bool>(false);
    public NetworkVariable<CardColor> CurrentColor = new NetworkVariable<CardColor>();
    // public NetworkList<ulong> ConnectedPlayerIds;
    public NetworkList<NetworkObjectReference> ConnectedPlayerObjects;

    public NetworkObject CurPlayer;

    private void Awake()
    {
        // ConnectedPlayerIds = new NetworkList<ulong>();
        ConnectedPlayerObjects = new NetworkList<NetworkObjectReference>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("SERVER");
        }
        else
        {
            Debug.Log("CLIENT");
            GameObject.Find("GameManager").GetComponent<GameManager>().SetupServerManager(this);
        }

        JoinedPlayers = new HashSet<ulong>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        FillPlayerPositions();

        // Create game logic
        GameLogic = new Logic(ConnectedPlayerObjects.Count, 0, new System.Random(), new LogicSettings());
        GameLogic.NewGame();

        Debug.Log("Starting game with ConnectedPlayerIds:");
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
            Debug.Log("\t" + ((NetworkObject)ConnectedPlayerObjects[i]).OwnerClientId + ": " + i);

        LastPlayedCard.Value = GameLogic.LastPlayedCard;
        OnTurn.Value = GameLogic.OnTurn;

        GameStarted = true;
        NotAcceptingMoves = false;

        // Create array of ConnectedPlayerIds and declare ClientRpcParams
        ConnectedPlayerIds = new ulong[ConnectedPlayerObjects.Count];
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
            ConnectedPlayerIds[i] = ((NetworkObject)ConnectedPlayerObjects[i]).OwnerClientId;

        SendToJoinedPlayersClientParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = ConnectedPlayerIds
            }
        };

        // Fill out ConnectedPlayers
        ConnectedPlayers = new Player[ConnectedPlayerObjects.Count];
        NetworkObject networkObject;
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
        {
            networkObject = ConnectedPlayerObjects[i];
            ConnectedPlayers[i] = networkObject.GetComponent<Player>();

            //AddPlayerInLobbyClientRpc(networkObject, SendToJoinedPlayersClientParams);
        }

        ulong clientId;
        Player player;
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
        {
            networkObject = ConnectedPlayerObjects[i];
            clientId = networkObject.OwnerClientId;
            player = networkObject.GetComponent<Player>();

            player.UpdateChooseColor(GameLogic.ChooseColor && GameLogic.OnTurn == PlayerPositions[clientId]);
            player.UpdateNumCardsInHand(GameLogic.PlayerHands[i].Count);

            //AddPlayerInLobbyClientRpc(networkObject, SendToJoinedPlayersClientParams);
        }

        StartGameClientRpc(SendToJoinedPlayersClientParams);

        NewRound();
    }

    private void NewRound()
    {
        GameLogic.NewGame();

        LastPlayedCard.Value = GameLogic.LastPlayedCard;
        OnTurn.Value = GameLogic.OnTurn;
        CurrentColor.Value = GameLogic.CurColor;

        NotAcceptingMoves = false;

        ulong playerId;
        Player player;
        for (int i = 0; i < ConnectedPlayers.Length; i++)
        {
            playerId = ConnectedPlayerIds[i];
            player = ConnectedPlayers[i];

            player.UpdateChooseColor(GameLogic.ChooseColor && GameLogic.OnTurn == PlayerPositions[playerId]);
            player.UpdateNumCardsInHand(GameLogic.PlayerHands[i].Count);
        }

        StartRoundClientRpc(SendToJoinedPlayersClientParams);
    }

    private void EndRound(MakeMoveResult moveResult)
    {
        NotAcceptingMoves = true;

        StartCoroutine(WaitAtEndRound(moveResult, 3));
    }

    private IEnumerator WaitAtEndRound(MakeMoveResult moveResult, float t)
    {
        // Before the wait timer
        EndRoundClientRpc(SendToJoinedPlayersClientParams);

        // Update scores
        NetworkObject networkObject;
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
        {
            networkObject = ConnectedPlayerObjects[i];
            networkObject.GetComponent<Player>().UpdateScore(moveResult.Scores[PlayerPositions[ConnectedPlayerIds[i]]]);
        }

        // Wait for t seconds
        yield return new WaitForSeconds(t);

        // After the wait timer
        NewRound();
    }

    [ClientRpc]
    private void EndRoundClientRpc(ClientRpcParams clientRpcParams = default)
    {

    }

    [ClientRpc]
    private void StartGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        FillPlayerPositions();

        GameStarted = true;

        LocalPlayer.StartNewGame();
    }

    [ClientRpc]
    private void StartRoundClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LocalPlayer.StartNewRound();
    }

    //[ClientRpc]
    //public void AddPlayerInLobbyClientRpc(NetworkObjectReference playerObjectRef, ClientRpcParams clientRpcParams = default)
    //{
    //    NetworkObject playerObject = playerObjectRef;
    //    ulong ownerId = playerObject.OwnerClientId;

    //    GameManager.AddOpponent(playerObject.GetComponent<Player>());
    //}

    private void FillPlayerPositions()
    {
        PlayerPositions = new Dictionary<ulong, int>();
        for (int i = 0; i < ConnectedPlayerObjects.Count; i++)
            PlayerPositions.Add(((NetworkObject)ConnectedPlayerObjects[i]).OwnerClientId, i);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(NetworkObjectReference playerObjectRef, ServerRpcParams serverRpcParams = default)
    {
        if (GameStarted)
            return;

        ulong playerId = serverRpcParams.Receive.SenderClientId;

        if (JoinedPlayers.Contains(playerId))
            return;

        JoinedPlayers.Add(playerId);
        // ConnectedPlayerIds.Add(playerId);
        ConnectedPlayerObjects.Add(playerObjectRef);
        Debug.Log("Added player: " + playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeMoveServerRpc(int move, ServerRpcParams serverRpcParams = default)
    {
        if (NotAcceptingMoves)
            return;

        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!PlayerPositions.TryGetValue(clientId, out int playerPos))
            return;

        MakeMoveResult moveResult = GameLogic.MakeMove(playerPos, move);

        Debug.Log("PlayerId = " + clientId + ", position = " + playerPos + ", move = " + move);

        // Return move response
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient))
            return;

        Player player = networkClient.PlayerObject.GetComponent<Player>();
        player.MakeMoveResponseClientRpc(moveResult);

        // Update NetworkVariables if needed
        if (moveResult.IsValid)
        {
            if (moveResult.MoveType == MoveType.PlayedCard || moveResult.MoveType == MoveType.DrewCard || moveResult.RefreshHand)
            {
                player.NumCardsInHand.Value = GameLogic.PlayerHands[playerPos].Count;
            }

            LastPlayedCard.Value = GameLogic.LastPlayedCard;
            OnTurn.Value = GameLogic.OnTurn;
            CurrentColor.Value = GameLogic.CurColor;

            player.UpdateChooseColor(GameLogic.ChooseColor);

            if (moveResult.IsOver)
                EndRound(moveResult);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;
        if (!PlayerPositions.TryGetValue(senderId, out int position))
            return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(senderId, out NetworkClient networkClient))
            return;

        Player player = networkClient.PlayerObject.GetComponent<Player>();
        player.ReceivePlayerHandClientRpc(new PlayerHandSerialized(GameLogic.PlayerHands[position]));
    }
}
