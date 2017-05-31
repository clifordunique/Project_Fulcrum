﻿using UnityEngine;
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
		if (dialogue.exit||dialogue.actor == -1)
		{
			actorIndex[currentActor].ExitStage();
			return;
		}

		//print("Loading actor #"+dialogue.actor);
		if (!loadedActors[dialogue.actor])
		{ 
			if(actorPrefabs[dialogue.actor]!=null)
			{
				GameObject newActor = (GameObject)Instantiate(actorPrefabs[dialogue.actor]);
				actorIndex[dialogue.actor] = newActor.GetComponent<Actor>();
				newActor.transform.SetParent(this.transform, false);
				loadedActors[dialogue.actor] = true;
			}
			else
			{
				GameObject newActor = (GameObject)Instantiate(actorPrefabs[0]);
				actorIndex[dialogue.actor] = newActor.GetComponent<Actor>();
				newActor.transform.SetParent(this.transform, false);
				loadedActors[dialogue.actor] = true;
			}
		}
		else
		{
			//print("Actor #"+dialogue.actor+" already loaded!");
		}
			
		actorIndex[dialogue.actor].PlayDialogueAnim(dialogue.leftSide, dialogue.emote);

		if ((currentActor != dialogue.actor)&&(loadedActors[currentActor])) 
		{
			//play closing anim on current actor, set currentactor to new
			actorIndex[currentActor].ExitStage();
			currentActor = dialogue.actor;
		}
	}
}