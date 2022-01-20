using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Player : NetworkBehaviour
{
    // public NetworkVariable<Card> Hand = new NetworkVariable<Card>(NetworkVariableReadPermission.OwnerOnly);
    // public NetworkListCustom<int> Hand;
    public NetworkVariable<int> NumCardsInHand = new NetworkVariable<int>(0);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<bool> ChooseColor = new NetworkVariable<bool>(false);

    public GameManager GameManager;
    public ServerManager ServerManager;

    public List<int> Hand;

    //private void Awake()
    //{
    //    Hand = new NetworkListCustom<int>(NetworkVariableReadPermission.OwnerOnly, new List<int>());
    //}

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            GameManager.OnConnectLocalPlayer(this);

            if (GameManager.ServerManager != null)
            {
                SetupServerManager(GameManager.ServerManager);
            }
        }
    }

    public void SetupServerManager(ServerManager serverManager)
    {
        ServerManager = serverManager;
        ServerManager.LocalPlayer = this;
        ServerManager.AddPlayerServerRpc();
    }

    public void StartNewGame()
    {
        GameManager.NewGame(NetworkManager.Singleton.LocalClientId);

        // ServerManager.ChooseColor.OnValueChanged += UpdateChooseColor;

       //  UpdateChooseColor(false, ServerManager.ChooseColor.Value);

        RequestPlayerHand();
    }

    public void RequestPlayerHand()
    {
        ServerManager.RequestPlayerHandServerRpc();
    }

    public void EndGame()
    {
        // ServerManager.ChooseColor.OnValueChanged -= UpdateChooseColor;
    }

    public void MakeMove(int move)
    {
        Debug.Log("PLAYING MOVE " + move);
        ServerManager.MakeMoveServerRpc(move);
    }

    [ClientRpc]
    public void MakeMoveResponseClientRpc(MakeMoveResult moveResult)
    {
        if (!IsOwner)
            return;

        GameManager.MakeMoveResponse(moveResult);
    }

    [ClientRpc]
    public void ReceivePlayerHandClientRpc(PlayerHandSerialized playerHand)
    {
        if (!IsOwner)
            return;

        Debug.Log("WE ARE HERE!");
        Hand = playerHand.ArrayToList();

        GameManager.UpdateHand();
    }

    public void UpdateNumCardsInHand(int n)
    {
        NumCardsInHand.Value = n;
    }

    public void UpdateChooseColor(bool isOn)
    {
        ChooseColor.Value = isOn;
    }
}
