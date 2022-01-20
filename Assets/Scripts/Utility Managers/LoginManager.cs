using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LoginManager : MonoBehaviour
{
    public GameObject GameManager;
    public GameObject ServerManagerPrefab;

    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        } else
        {
            StatusLabels();
        }
    }

    private void StartButtons()
    {
        if (GUILayout.Button("Server")) StartAs("Server");
        if (GUILayout.Button("Host")) StartAs("Host");
        if (GUILayout.Button("Client")) StartAs("Client");
    }

    private void StartAs(string mode)
    {
        if (mode == "Server")
        {
            NetworkManager.Singleton.StartServer();
            SpawnServer();
        }
        else if (mode == "Host")
        {
            // TODO: remove?
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        Debug.Log("CHECKPOINT 1");
        GameManager.GetComponent<GameManager>().Init(mode);
        Debug.Log("CHECKPOINT 2");
    }

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
        GUILayout.Label("Connected as: " + mode);
    }

    private void SpawnServer()
    {
        GameObject go = Instantiate(ServerManagerPrefab);
        go.GetComponent<NetworkObject>().Spawn();
    }
}
