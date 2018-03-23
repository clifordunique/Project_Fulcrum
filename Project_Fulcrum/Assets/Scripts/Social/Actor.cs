using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour {

	public Animator emoteAnimator;
	public Animator positionAnimator;
	public int actorID;

	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetInactive() //Used when closing animation finishes to save resources.
	{
		
	}

	public void ExitStage()
	{
		positionAnimator.SetInteger("state", 0);
	}

	public void TestAnim(int value)
	{
		positionAnimator.SetInteger("state", value);
	}

	public void TestEmote(int value)
	{
		emoteAnimator.SetInteger("EMOTE", value);
	}

	public void SetPositionAnim(int id)
	{
		positionAnimator.SetInteger("state", id);
	}

	public void SetEmoteAnim(int id)
	{
		emoteAnimator.SetInteger("EMOTE", id);
	}

	public void PlayDialogueAnim(bool leftSide, int emote)
	{
		if (leftSide)
		{
			positionAnimator.SetInteger("state", 1);
		}
		else
		{
			positionAnimator.SetInteger("state", 2);
		}

		emoteAnimator.SetInteger("EMOTE", emote);



		//actors[Dialogue.actor].PlayDialogueAnim(side, emote)
	}
}
