using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Players/New Player")]
public class PlayerScriptable : ScriptableObject
{
    public GameManagerOld GameManager;
    public int Id;

    public void MakeMove(int move)
    {
        GameManager.MakeMove(Id, move);
    }
}
