
// File:			GAFMovieClipEditor.cs
// Version:			5.2
// Last changed:	2017/3/28 12:42
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEditor;
using UnityEngine;

using GAF.Core;
using GAF.Assets;
using GAF.Objects;

using GAFEditorInternal.Core;

namespace GAFEditor.Core
{
	[CustomEditor(typeof(GAFMovieClip))]
	[CanEditMultipleObjects]
	public class GAFMovieClipEditor : GAFMovieClipInternalEditor<GAFObjectsManager, GAFTexturesResource>
	{
	}
}