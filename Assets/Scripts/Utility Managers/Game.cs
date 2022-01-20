using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public List<Player> Players;
    public Logic GameLogic;

    public Game(List<Player> players, System.Random rng, LogicSettings logicSettings)
    {
        Players = players;
        GameLogic = new Logic(players.Count, 0, rng, logicSettings);
    }
}
