using UnityEngine;
using System.Collections;
using System;

public class ActorAnimator : MonoBehaviour {

	public GameObject[] actorPrefabs;
	public Actor[] actorIndex;
	public int currentActor;
	public bool[] loadedActors;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void ClearAnimation()
	{
		
	}

	public void AnimateDialogue(Dialogue dialogue)
	{
		if (!loadedActors[dialogue.actor])
		{ 
			print("Actor not loaded!!");
			GameObject newActor = (GameObject)Instantiate(actorPrefabs[dialogue.actor]);
			actorIndex[dialogue.actor] = newActor.GetComponent<Actor>();
			newActor.transform.SetParent(this.transform, false);
			loadedActors[dialogue.actor] = true;
		}
			
		actorIndex[dialogue.actor].PlayDialogueAnim(dialogue.leftSide, dialogue.emote);

		if (currentActor != dialogue.actor) 
		{
			//play closing anim on current actor, set currentactor to new
			actorIndex[currentActor].ExitStage();
			currentActor = dialogue.actor;
		}

		if (dialogue.exit)
		{
			actorIndex[currentActor].ExitStage();
		}
	}
}
