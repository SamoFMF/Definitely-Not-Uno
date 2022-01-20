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
    private bool IsRunning;

    public NetworkVariable<Card> LastPlayedCard = new NetworkVariable<Card>();
    public NetworkVariable<int> OnTurn = new NetworkVariable<int>();
    // public NetworkVariable<bool> ChooseColor = new NetworkVariable<bool>(false);
    public NetworkVariable<CardColor> CurrentColor = new NetworkVariable<CardColor>();
    public NetworkList<ulong> ConnectedPlayerIds;

    public NetworkObject CurPlayer;

    private void Awake()
    {
        ConnectedPlayerIds = new NetworkList<ulong>();
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

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        FillPlayerPositions();

        // Create game logic
        GameLogic = new Logic(ConnectedPlayerIds.Count, 0, new System.Random(), new LogicSettings());
        GameLogic.NewGame();

        Debug.Log("Starting game with ConnectedPlayerIds:");
        for (int i = 0; i < ConnectedPlayerIds.Count; i++)
            Debug.Log("\t" + ConnectedPlayerIds[i] + ": " + i);

        LastPlayedCard.Value = GameLogic.LastPlayedCard;
        OnTurn.Value = GameLogic.OnTurn;
        // ChooseColor.Value = GameLogic.ChooseColor;

        // TODO: Enable ChooseColor if needed

        // Create array of ConnectedPlayerIds
        ulong[] connectedPlayerIds = new ulong[ConnectedPlayerIds.Count];
        for (int i = 0; i < ConnectedPlayerIds.Count; i++)
            connectedPlayerIds[i] = ConnectedPlayerIds[i];

        ulong clientId;
        Player player;
        NetworkObject networkObject;
        List<Player> players = new List<Player>();
        for (int i = 0; i < ConnectedPlayerIds.Count; i++)
        {
            clientId = ConnectedPlayerIds[i];
            networkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            player = networkObject.GetComponent<Player>();

            player.UpdateChooseColor(GameLogic.ChooseColor && GameLogic.OnTurn == PlayerPositions[clientId]);
            player.UpdateNumCardsInHand(GameLogic.PlayerHands[i].Count);

            AddPlayerInLobbyClientRpc(networkObject, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = connectedPlayerIds
                }
            });
        }

        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        FillPlayerPositions();

        LocalPlayer.StartNewGame();
    }

    [ClientRpc]
    public void AddPlayerInLobbyClientRpc(NetworkObjectReference playerObjectRef, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject playerObject = playerObjectRef;
        ulong ownerId = playerObject.OwnerClientId;

        GameManager.AddOpponent(playerObject.GetComponent<Player>());
    }

    private void FillPlayerPositions()
    {
        PlayerPositions = new Dictionary<ulong, int>();
        for (int i = 0; i < ConnectedPlayerIds.Count; i++)
            PlayerPositions.Add(ConnectedPlayerIds[i], i);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeMoveServerRpc(int move, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        PlayerPositions.TryGetValue(clientId, out int playerPos);
        if (playerPos == null)
            return;

        MakeMoveResult moveResult = GameLogic.MakeMove(playerPos, move);

        Debug.Log("PlayerId = " + clientId + ", position = " + playerPos + ", move = " + move);

        // Return move response
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient);
        if (networkClient == null)
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
            // ChooseColor.Value = GameLogic.ChooseColor;
            CurrentColor.Value = GameLogic.CurColor;

            player.UpdateChooseColor(GameLogic.ChooseColor);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        if (JoinedPlayers.Contains(playerId))
            return;

        JoinedPlayers.Add(playerId);
        ConnectedPlayerIds.Add(playerId);
        Debug.Log("Added player: " + playerId);
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
