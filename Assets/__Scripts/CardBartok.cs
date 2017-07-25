using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CBState 
{
	drawpile,
	toHand,
	hand,
	toTarget,
	target,
	discard,
	to,
	idle
}

//CardBartok extends Card like in CardProspector
public class CardBartok : Card 
{
	//These fields will be the same for all instances of CardBartok
	static public float MOVE_DURATION = 0.5f;
	static public string MOVE_EASING = Easing.InOut;
	static public float CARD_HEIGHT = 3.5f;
	static public float CARD_WIDTH = 2f;

	public CBState state = CBState.drawpile;

	//Fields to store movement and rotation information
	public List<Vector3> bezierPts;
	public List<Quaternion> bezierRots;
	public float timeStart, timeDuration;

	public int eventualSortOrder;
	public string eventualSortLayer;

	//When the card is done moving, it calls SendMessage
	public GameObject reportFinishTo = null;

	//Tells target card to interpolate to a new position and rotation
	public void MoveTo(Vector3 ePos, Quaternion eRot)
	{
		//An interpolation list specifically for the card
		bezierPts = new List<Vector3> ();
		bezierPts.Add (transform.localPosition); //Current position
		bezierPts.Add(ePos); 					//New position
		bezierRots = new List<Quaternion>();
		bezierRots.Add (transform.rotation); 
		bezierRots.Add (eRot);

		//If timeStart is 0, it starts immediately
		if (timeStart == 0) 
		{
			timeStart = Time.time;
		}
		timeDuration = MOVE_DURATION;

		state = CBState.to;
	}

	//Overload of MoveTo that doesn't need a rotation argument
	public void MoveTo(Vector3 ePos) 
	{
		MoveTo (ePos, Quaternion.identity);
	}

	void Update()
	{
		switch (state) {
		//A to__ state is when a card is interpolating
		case CBState.toHand:
		case CBState.toTarget:
		case CBState.to:
			//Get u (0 <= u <= 1) from current time and duration
			float u = (Time.time - timeStart) / timeDuration;

			//Use Utils class to curve the u 
			float uC = Easing.Ease (u, MOVE_EASING);

			if (u < 0) { //We shouldn't move yet
				transform.localPosition = bezierPts [0];
				transform.rotation = bezierRots [0];
				return;
			} else if (u >= 1) { //We're finished moving
				if (state == CBState.toHand)
					state = CBState.hand;
				if (state == CBState.toTarget)
					state = CBState.toTarget;
				if (state == CBState.to)
					state = CBState.idle;
				//Move to final position
				transform.localPosition = bezierPts [bezierPts.Count - 1];
				transform.rotation = bezierRots [bezierPts.Count - 1];
				//Reset timeStart to 0
				timeStart = 0;

				if (reportFinishTo != null) { //If there's a callback GameObject
					// use SendMessage to call the CBCallback method with this as parameter
					reportFinishTo.SendMessage ("CBCallback", this);
					//reportFinishTo must be set to null so that 
					//the card doesn't continue to report to the 
					//same GO whenever it moves
					reportFinishTo = null;
				} else {
					//Nothing
				}
			} else { //0 <= u <= 1, so this is interpolating now
				Vector3 pos = Utils.Bezier (uC, bezierPts);
				transform.localPosition = pos;
				Quaternion rotQ = Utils.Bezier (uC, bezierRots);
				transform.rotation = rotQ;

				//Jump to the proper sort order
				if (u > 0.5f && spriteRenderers [0].sortingOrder != eventualSortOrder) {
					SetSortOrder (eventualSortOrder);
				}
				if (u > 0.75f && spriteRenderers [0].sortingLayerName != eventualSortLayer) {
					SetSortingLayerName (eventualSortLayer);
				}
			}
			break;
		}
	}
}
