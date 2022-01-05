using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int Mod(int a, int n)
    {
        if (a < 0)
            a += n;
        return a % n;
    }
}
