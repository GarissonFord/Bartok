using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bartok : MonoBehaviour 
{

	static public Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	public bool ____________________________;

	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;

	void Awake() 
	{
		S = this;
	}

	void Start()
	{
		deck = GetComponent<Deck> (); //Get the deck
		deck.InitDeck(deckXML.text); //Pass DeckXML
		Deck.Shuffle(ref deck.cards); //Shuffle the deck
		//The keyword 'ref' passes a reference to deck.cards,
		//which lets deck.cards be modified by deck.Shuffle()

		layout = GetComponent<BartokLayout> ();
		layout.ReadLayout (layoutXML.text);

		drawPile = UpgradeCardsList (deck.cards);
	}

	List<CardBartok> UpgradeCardsList(List<Card> lCD)
	{
		List<CardBartok> lCB = new List<CardBartok> ();
		foreach (Card tCD in lCD) 
		{
			lCB.Add (tCD as CardBartok);
		}
		return (lCB);
	}
}
