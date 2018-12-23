using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(CharacterCard), true)]
public class CharacterCardHandler : Editor
{

	private CharacterCard CC;

	void Awake()
	{
		CC = (CharacterCard)target;
	}

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Apply Visuals"))
		{
			CC.UpdateCharacterCard();
		}

		DrawDefaultInspector();
	}

}
