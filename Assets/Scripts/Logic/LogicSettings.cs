using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicSettings
{
    // TODO
    public int NumStartCards, NumIlegalMoveDraw, NumSameWildCards, NumSameColorCards;

    public LogicSettings()
    {
        NumStartCards = 7;
        NumIlegalMoveDraw = 2;
        NumSameWildCards = 4;
        NumSameColorCards = 2;
    }
}
