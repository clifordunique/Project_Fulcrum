using UnityEngine;
using System.Collections;
using System;

public class ActorAnimator : MonoBehaviour {

	public GameObject[] actorPrefab;
	//[ReadOnlyAttribute][SerializeField] private List<Actor> actorInstance;
	[ReadOnlyAttribute][SerializeField] private Actor[] actorInstance;
	[ReadOnlyAttribute][SerializeField] private int currentActor;
	[ReadOnlyAttribute][SerializeField] private bool[] actorIsLoaded;

	void Start () 
	{
		actorInstance = new Actor[actorPrefab.Length];
		actorIsLoaded = new bool[actorPrefab.Length];
	}
	
	void Update () 
	{
	}

	private void LoadActor(int id)
	{
		if(actorPrefab[id]!=null)
		{
			print("Loading actor #"+id);
			GameObject newActor = (GameObject)Instantiate(actorPrefab[id]);
			actorInstance[id] = newActor.GetComponent<Actor>();
			newActor.transform.SetParent(this.transform, false);
			actorIsLoaded[id] = true;
		}
		else
		{
			print("Actor #"+id+" not found, loading default actor.");
			GameObject newActor = (GameObject)Instantiate(actorPrefab[0]);
			actorInstance[id] = newActor.GetComponent<Actor>();
			newActor.transform.SetParent(this.transform, false);
			actorIsLoaded[id] = true;
		}
	}

	public void AnimateDialogue(Dialogue dialogue)
	{
		foreach(ActorAction aa in dialogue.actorAction)
		{
			int id = aa.actorID;
			if(id==-1) // Action applies to the scene itself
			{
				if(aa.actionID==-1) // Exit scene
				{
					foreach(Actor a in actorInstance)
					{
						if(a!=null)
						{
							a.SetPositionAnim(0);
						}
					}
				}
			}
			else if(id >= actorPrefab.Length)
			{
				print("ActorID "+id+" outside of valid range.");
			}
			else
			{
				if(actorInstance[id] == null)
				{
					LoadActor(id);
				}
				if(actorInstance[id]!=null)
				{
					actorInstance[id].SetPositionAnim(aa.positionID);
					actorInstance[id].SetEmoteAnim(aa.actionID);
				}
			}
		}

//		if (dialogue.exit||dialogue.actor == -1)
//		{
//			print("currentActor:"+currentActor);
//			if(actorInstance[currentActor]!=null)
//			{
//				actorInstance[currentActor].ExitStage();
//			}
//			return;
//		}
//
//		print("Loading actor #"+dialogue.actor);
//		if (!actorIsLoaded[dialogue.actor])
//		{ 
//			if(actorPrefab[dialogue.actor]!=null)
//			{
//				print("Loaded actor #"+dialogue.actor);
//				GameObject newActor = (GameObject)Instantiate(actorPrefab[dialogue.actor]);
//				actorInstance[dialogue.actor] = newActor.GetComponent<Actor>();
//				newActor.transform.SetParent(this.transform, false);
//				actorIsLoaded[dialogue.actor] = true;
//			}
//			else
//			{
//				print("Actor #"+dialogue.actor+" not found, loading default actor.");
//				GameObject newActor = (GameObject)Instantiate(actorPrefab[0]);
//				actorInstance[dialogue.actor] = newActor.GetComponent<Actor>();
//				newActor.transform.SetParent(this.transform, false);
//				actorIsLoaded[dialogue.actor] = true;
//			}
//		}
//		else
//		{
//			print("Actor #"+dialogue.actor+" already loaded!");
//		}
//			
//		actorInstance[dialogue.actor].PlayDialogueAnim(dialogue.leftSide, dialogue.emote);
//
//		if ((currentActor != dialogue.actor)&&(actorIsLoaded[currentActor])) 
//		{
//			//play closing anim on current actor, set currentactor to new
//			actorInstance[currentActor].ExitStage();
//			currentActor = dialogue.actor;
//		}
	}
}
