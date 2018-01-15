
// File:			GAFLoadResourceFromBundle.cs
// Version:			5.2
// Last changed:	2017/3/31 09:45
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEngine;

using System.Collections;

using GAF.Core;
using GAF.Assets;
using GAFInternal.Assets;
using GAFInternal.Core;

namespace GAF.Demo
{
	[AddComponentMenu("GAF/Demo/GAFLoadResourceFromBundle")]
	[RequireComponent(typeof(GAFMovieClip))]
	public class GAFLoadResourceFromBundle : MonoBehaviour
	{
		#region Members

		[SerializeField]
		private string BundleUrl = string.Empty;
		[SerializeField]
		private string AssetName = string.Empty;
		[Range(0, 5000)]
		[SerializeField]
		private int TimelineID = 0;

		private AssetBundle m_Bundle = null;

		#endregion // Members

		#region Interface

		public GAFTexturesResourceInternal getResource(string _ResourceName)
		{
			GAFTexturesResourceInternal resource = null;

			if (m_Bundle != null)
			{
				resource = m_Bundle.LoadAsset(_ResourceName, typeof(GAFTexturesResourceInternal)) as GAFTexturesResourceInternal;
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

			setUpClip();

			m_Bundle.Unload(false);
		}

		private void setUpClip()
		{
			if (m_Bundle != null)
			{
				GAFAnimationAsset asset = null;
				if (string.IsNullOrEmpty(AssetName))
				{
					asset = m_Bundle.mainAsset as GAFAnimationAsset;
				}
				else
				{
					asset = m_Bundle.LoadAsset(AssetName, typeof(GAFAnimationAsset)) as GAFAnimationAsset;
				}

				var clip = GetComponent<GAFMovieClip>();

				clip.initialize(asset, TimelineID);
				clip.usePlaceholder = false;

				
				clip.reload();

				clip.setDefaultSequence(true);
			}
		}

#endregion // Implementation
	}
}
