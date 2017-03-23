using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudio : MonoBehaviour {

	private AudioSource charAudioSource;
	[SerializeField]private AudioClip[] footStepSounds;
	[SerializeField]private PlatformerCharacter2D theCharacter;
	[SerializeField][Range(0,1f)]private float volumeM;


	// Use this for initialization
	void Start () 
	{
		charAudioSource = this.gameObject.GetComponent<AudioSource>();
	}

	public void StepSound()
	{
		float speed = theCharacter.pVel.magnitude;
		print(speed);
		float volume = 0.5f;
		if(speed <= 30)
		{
			volume = 0.5f*(speed/30);
		}
		volume *= volumeM;
		charAudioSource.PlayOneShot(footStepSounds[1], volume);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
