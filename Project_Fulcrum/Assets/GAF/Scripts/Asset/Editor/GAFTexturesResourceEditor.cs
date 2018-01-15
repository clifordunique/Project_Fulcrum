
// File:			GAFTexturesResourceEditor.cs
// Version:			5.2
// Last changed:	2017/3/28 12:42
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEngine;
using UnityEditor;

using GAF.Assets;

using GAFEditorInternal.Assets;

namespace GAFEditor.Assets
{
	[CustomEditor(typeof(GAFTexturesResource))]
	public class GAFTexturesResourceEditor : GAFTexturesResourceInternalEditor
	{
	}
}