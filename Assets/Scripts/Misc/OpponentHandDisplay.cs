using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHandDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text NumOfCardsText;

    public void SubscribeToUpdates(Player player)
    {
        player.NumCardsInHand.OnValueChanged += UpdateText;
    }

    private void UpdateText(int previousValue, int newValue)
    {
        NumOfCardsText.text = newValue.ToString();
    }
}
