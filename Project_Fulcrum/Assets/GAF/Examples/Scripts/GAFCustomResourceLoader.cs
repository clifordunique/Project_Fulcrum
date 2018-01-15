
// File:			GAFCustomResourceLoader.cs
// Version:			5.2
// Last changed:	2017/3/28 12:42
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEngine;

using System.Collections.Generic;

using GAFInternal.Assets;

namespace GAF.Demo
{
	[AddComponentMenu("GAF/Demo/GAFCustomResourceLoader")]
	public class GAFCustomResourceLoader : MonoBehaviour 
	{
		#region Members

		[SerializeField] private List<GAFTexturesResourceInternal> PreloadedResources = new List<GAFTexturesResourceInternal>();

		#endregion // Members

		#region Interface

		public GAFTexturesResourceInternal getResource(string _ResourceName)
		{
			return PreloadedResources.Find (resource => resource.name == _ResourceName);
		}

		#endregion // Interface
	}
}
