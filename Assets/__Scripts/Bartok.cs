using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bartok : MonoBehaviour 
{

	static public Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	public float handFanDegrees = 10f;
	public int numStartingCards = 7;
	public float drawTimeStagger = 0.1f;

	public bool ____________________________;

	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;

	public List<Player> players;
	public CardBartok targetCard;

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
		LayoutGame ();
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

	public void ArrangeDrawPile() 
	{
		CardBartok tCB;

		for (int i = 0; i < drawPile.Count; i++)
		{
			tCB = drawPile [i];
			tCB.transform.parent = layoutAnchor;
			tCB.transform.localPosition = layout.drawPile.pos;
			tCB.faceUp = false;
			tCB.SetSortingLayerName (layout.drawPile.layerName);
			tCB.SetSortOrder (-i * 4);
			tCB.state = CBState.drawpile;
		}
	}

	//The initial layout
	void LayoutGame() 
	{
		//Creating an empty GO in order to be a tableau anchor
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}

		//Positions the draw pile
		ArrangeDrawPile ();

		//Sets up the players
		Player p1;
		players = new List<Player> ();

		foreach (SlotDef tSD in layout.slotDefs) 
		{
			p1 = new Player ();
			p1.handSlotDef = tSD;
			players.Add (p1);
			p1.playerNum = players.Count;
		}

		//The 0th player is you, dude
		players [0].type = PlayerType.human;

		CardBartok tCB;
		//Deal 7 cards to each player
		for (int i = 0; i < numStartingCards; i++) {
			for (int j = 0; j < 4; j++) {
				tCB = Draw ();
				//Stagger the draw time 
				tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
				//Add the card to the player's hand
				players[(j + 1) % 4].AddCard(tCB);
			}

			//Calls DrawFirstTarget() once the hands have been drawn
			Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
		}
	}

	public void DrawFirstTarget()
	{
		//Flip up the first target card of the drawPile
		CardBartok tCB = MoveToTarget (Draw ());
	}

	//Makes a new card the target
	public CardBartok MoveToTarget(CardBartok tCB){
		tCB.timeStart = 0;
		tCB.MoveTo (layout.discardPile.pos + Vector3.back);
		tCB.state = CBState.toTarget;
		tCB.faceUp = true;
		tCB.SetSortingLayerName ("10");
		tCB.eventualSortLayer = layout.target.layerName;
		if (targetCard != null) {
			MoveToDiscard (targetCard);
		}

		targetCard = tCB;

		return (tCB);
	}

	public CardBartok MoveToDiscard(CardBartok tCB) {
		tCB.state = CBState.discard;
		discardPile.Add (tCB);
		tCB.SetSortingLayerName (layout.discardPile.layerName);
		tCB.SetSortOrder (discardPile.Count * 4);
		tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

		return (tCB);
	}

	//This pulls a single card from the draw pile and returns it
	public CardBartok Draw() {
		CardBartok cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	//This will test adding cards to players' hands
	void Update() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			players [0].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			players [1].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			players [2].AddCard (Draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			players [3].AddCard (Draw ());
		}
	}
}
