using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resources Manager", menuName = "Managers/Resources Manager")]
public class ResourcesManager : ScriptableObject
{
    public CardScriptable[] Deck;
    private Dictionary<int, CardScriptable> IdToCard = new Dictionary<int, CardScriptable>();

    public void Init()
    {
        IdToCard.Clear();

        if (Deck == null)
            return;

        for (int i = 0; i < Deck.Length; i++)
        {
            IdToCard.Add(Deck[i].Id, Deck[i]);
        }
    }

    public CardScriptable GetCardScriptable(int id)
    {
        IdToCard.TryGetValue(id, out CardScriptable card);
        return card;
    }

    public void UpdateCardDisplay(CardDisplay cardDisplay, int id)
    {
        CardScriptable card = GetCardScriptable(id);
        cardDisplay.LoadCard(card);
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