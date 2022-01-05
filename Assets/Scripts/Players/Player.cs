using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Players/New Player")]
public class Player : ScriptableObject
{
    public GameManager GameManager;
    public int Id;

    public void MakeMove(int move)
    {
        GameManager.MakeMove(Id, move);
    }
}
