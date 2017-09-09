using UnityEngine;
using System.Collections.Generic;


#if UNITY_ANDROID

public class MoPubAndroidInterstitial
{
	private readonly AndroidJavaObject _interstitialPlugin;

	public MoPubAndroidInterstitial (string adUnitId)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_interstitialPlugin = new AndroidJavaObject ("com.mopub.unity.MoPubInterstitialUnityPlugin", adUnitId);
	}


	// Starts loading an interstitial ad
	public void requestInterstitialAd (string keywords = "")
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_interstitialPlugin.Call ("requestInterstitialAd", keywords);
	}


	// If an interstitial ad is loaded this will take over the screen and show the ad
	public void showInterstitialAd ()
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_interstitialPlugin.Call ("showInterstitialAd");
	}
}

#endif