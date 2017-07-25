using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //Enables LINQ queries

public enum PlayerType
{
	human,
	ai
}

[System.Serializable]
//Keep in mind, Player does not extend MonoBehaviour
public class Player 
{
	public PlayerType type = PlayerType.ai;
	public int playerNum;

	public List<CardBartok> hand; //Cards in the player's hand

	public SlotDef handSlotDef;

	public CardBartok AddCard(CardBartok eCB) 
	{
		if (hand == null) hand = new List<CardBartok> ();
		//Add the card to the hand
		hand.Add (eCB);

		//Sort the cards by rank using LINQ if it's a human's
		if (type == PlayerType.human) {
			CardBartok[] cards = hand.ToArray (); //Copy hand toa new array

			//This LINQ call works on the CardBartok array
			//Similar to foreach but it also sorts them by rank
			cards = cards.OrderBy (cd => cd.rank).ToArray ();
			//Convert the array back to a List<CardBartok>
			hand = new List<CardBartok> (cards);
		}

		eCB.SetSortingLayerName ("10"); //Sorts the moving card to the top
		eCB.eventualSortLayer = handSlotDef.layerName;

		FanHand ();
		return(eCB);
	}

	//Removes a card from the hand
	public CardBartok RemoveCard(CardBartok cb) {
		hand.Remove (cb);
		FanHand ();
		return(cb);
	}

	public void FanHand() {
		//The rotation about Z of the first card
		float startRot = 0;
		startRot = handSlotDef.rot;
		if (hand.Count > 1) {
			startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
		}
		//Each card is then rotated by handFanDegrees

		//Move the cards to their new position
		Vector3 pos;
		float rot;
		Quaternion rotQ;

		for (int i = 0; i < hand.Count; i++) 
		{
			rot = startRot - Bartok.S.handFanDegrees * i; //Rotate about the z axis
			//Quaternion of the same rotation as rot
			rotQ = Quaternion.Euler (0, 0, rot);

			//A vector3 slightly above [0, 0, 0]
			pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
			//Mutiplying a Quaternion and a Vector3 rotates the
			//Vector3 by the rotation of the Quaternion
			pos = rotQ * pos;

			//Add the base position of the player's hand
			pos += handSlotDef.pos;
			//Staggers the cards along Z to prevent colliders from overlapping
			pos.z = -0.5f * i;

			//Set localPosition and rotation of the ith card in the hand
			hand[i].MoveTo(pos, rotQ); //Tells CardBartok to interpolate
			hand[i].state = CBState.toHand;

			/*
			hand [i].transform.localPosition = pos;
			hand [i].transform.rotation = rotQ;
			hand [i].state = CBState.hand;
			*/

			//faceUp will be true if type is the enum PlayerType.human
			hand [i].faceUp = (type == PlayerType.human);

			//Sets a proper SortOrder for overlap
			//hand [i].SetSortOrder (i * 4);
			hand [i].eventualSortOrder = i * 4;

		}
	}
}
