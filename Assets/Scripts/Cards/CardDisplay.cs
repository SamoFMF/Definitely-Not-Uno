using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardScriptable Card;
    public Image Img;

    // Start is called before the first frame update
    void Start()
    {
        LoadCard(Card);
    }

    public void LoadCard(CardScriptable card)
    {
        if (card == null)
            return;

        Card = card;
        Img.sprite = card.Art;
    }
}
