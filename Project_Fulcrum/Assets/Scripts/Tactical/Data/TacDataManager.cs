//TacDataManager.cs
//C#
using UnityEngine;
using System;    //allows us to use IndexOutOfRangeException
using System.Collections;

public class TacDataManager : MonoBehaviour
{
	#region this stuff handles static instance management
	private static TacDataManager s_instance;
	public static TacDataManager instance { get { return s_instance; } }

	public void Awake()
	{
		if (!staticallyInstanced) { return; } 

		if (s_instance != null)
		{//only one instance can co exist
			if (s_instance != this)
			{
				GameObject.Destroy(gameObject);        //    remove duplicate instances
			}
			return; 
		}
		s_instance = this;        //at this point TacDataManager.instance refers to this script
	}
	#endregion

	public bool staticallyInstanced;    //this bool controls whether we statically instance this script

	public Character[] Character;    //this is assigned inside the inspector

	//allows you to access the Character indirectly via indexing
	public Character this[int index]
	{
		get
		{
			if (index < 0 || index >= Character.Length) { throw new IndexOutOfRangeException(); }
			return Character[index];
		}
	}
	//returns the length of the Character array
	public int Length { get { return Character.Length; } }

	//these methods can be called e.g. TacDataManager.GetCharacter(3);    //returns the 3rd inventory item defined
	public static Character[] GetCharacter()
	{//returns the Character array inside the static instance
		return s_instance.Character;
	}
	public static Character GetCharacter(int index)
	{//returns the Character in the Character array inside the static instance
		return s_instance[index];
	}
	public static int CountCharacter()
	{//returns the Character array length inside the static instance
		return s_instance.Length;
	}
}
