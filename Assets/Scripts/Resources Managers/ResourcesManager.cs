using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resources Manager", menuName = "Managers/Resources Manager")]
public class ResourcesManager : ScriptableObject
{
    public CardScriptable[] Deck;
    public ArrowScriptable[] Arrows;
    private Dictionary<int, CardScriptable> IdToCard = new Dictionary<int, CardScriptable>();
    private Dictionary<CardColor, ArrowScriptable> ColorToArrow = new Dictionary<CardColor, ArrowScriptable>();

    public void Init()
    {
        IdToCard.Clear();

        if (Deck == null)
            return;

        for (int i = 0; i < Deck.Length; i++)
        {
            IdToCard.Add(Deck[i].Id, Deck[i]);
        }

        if (Arrows == null)
            return;

        for (int i = 0; i < Arrows.Length; i++)
        {
            ColorToArrow.Add(Arrows[i].Color, Arrows[i]);
        }
    }

    public CardScriptable GetCardScriptable(int id)
    {
        IdToCard.TryGetValue(id, out CardScriptable card);
        return card;
    }

    public ArrowScriptable GetArrowScriptable(CardColor color)
    {
        ColorToArrow.TryGetValue(color, out ArrowScriptable arrow);
        return arrow;
    }

    public void UpdateCardDisplay(CardDisplay cardDisplay, int id)
    {
        CardScriptable card = GetCardScriptable(id);
        cardDisplay.LoadCard(card);
    }

    public void UpdateArrowDisplay(ArrowDisplay arrowDisplay, CardColor color)
    {
        Debug.Log("color = " + color.ToString());
        ArrowScriptable arrow = GetArrowScriptable(color);
        arrowDisplay.LoadArrow(arrow);
    }

    public GameObject GetCardPrefab(int id, GameObject cardPrefab, Transform parent, Player player)
    {
        CardScriptable card = GetCardScriptable(id);
        GameObject cardObject = Instantiate(cardPrefab, parent);
        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
        cardDisplay.LoadCard(card);
        cardObject.GetComponent<ClickCard>().Player = player;

        //Debug.Log(card + " " + cardObject.GetComponent<CardDisplay>().Card);

        return cardObject;
    }
}
