
// File:			GAFClipsCreator.cs
// Version:			5.2
// Last changed:	2017/3/30 18:33
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEngine;

using System.Collections;

using GAF.Assets;
using GAF.Core;

namespace GAF.Demo
{
	[AddComponentMenu("GAF/Demo/GAFClipsCreator")]
	public class GAFClipsCreator : MonoBehaviour
	{
		#region Members

		[SerializeField]
		private string BundleUrl = string.Empty;
		[SerializeField]
		private string AssetName = string.Empty;
		[Range(0, 5000)]
		[SerializeField]
		private int TimelineID = 0;
		[SerializeField]
		private bool BakeObjects = false;
		[SerializeField]
		private Vector3 Position = Vector3.zero;

		private AssetBundle m_Bundle = null;

		#endregion // Members

		#region Interface

		public GAFTexturesResource getResource(string _ResourceName)
		{
			GAFTexturesResource resource = null;

			if (m_Bundle != null)
			{
				resource = m_Bundle.LoadAsset(_ResourceName, typeof(GAFTexturesResource)) as GAFTexturesResource;
			}

			return resource;
		}

		#endregion // Interface

		#region Implementation

		private void Awake()
		{
			switch (Application.platform)
			{
#if UNITY_5_0
                case RuntimePlatform.Android:
                    BundleUrl += "_android.unity3d";
                    break;

                case RuntimePlatform.IPhonePlayer:
                    BundleUrl += "_iphone.unity3d";
                    break;

                default:
                    BundleUrl += ".unity3d";
                    break;
#else
                case RuntimePlatform.Android:
                    BundleUrl += "_android";
                    break;

                case RuntimePlatform.IPhonePlayer:
                    BundleUrl += "_ios";
                    break;

#endif // UNITY_5_0
            }
        }

		private IEnumerator Start()
		{
			Caching.CleanCache();
		    string bundleUrl = BundleUrl;

#if !UNITY_5_0
             bundleUrl = BundleUrl.ToLower();
#endif
            var www = WWW.LoadFromCacheOrDownload(bundleUrl, 0);

			yield return www;

			m_Bundle = www.assetBundle;

			createClip();

			m_Bundle.Unload(false);
		}

		private void createClip()
		{
			if (m_Bundle != null)
			{
				GAFAnimationAsset asset = null;
				if (string.IsNullOrEmpty(AssetName))
					asset = m_Bundle.mainAsset as GAFAnimationAsset;
				else
					asset = m_Bundle.LoadAsset(AssetName, typeof(GAFAnimationAsset)) as GAFAnimationAsset;

				var clipObj = new GameObject(asset.name);
				clipObj.transform.position = Position;

				GAFInternal.Core.GAFBaseClip clip = null;
				if (BakeObjects)
					clip = clipObj.AddComponent<GAFBakedMovieClip>();
				else
					clip = clipObj.AddComponent<GAFMovieClip>();

				clip.initialize(asset, TimelineID);

				clip.useCustomDelegate = true;
				clip.customGetResourceDelegate = getResource;
				clip.reload();
			}
		}

#endregion // Implementation
	}
}
