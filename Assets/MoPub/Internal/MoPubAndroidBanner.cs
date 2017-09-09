using UnityEngine;
using System.Collections.Generic;


#if UNITY_ANDROID

public enum MoPubAdPosition
{
	TopLeft,
	TopCenter,
	TopRight,
	Centered,
	BottomLeft,
	BottomCenter,
	BottomRight
}


public class MoPubAndroidBanner
{
	private readonly AndroidJavaObject _bannerPlugin;

	public MoPubAndroidBanner (string adUnitId)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_bannerPlugin = new AndroidJavaObject ("com.mopub.unity.MoPubBannerUnityPlugin", adUnitId);
	}


	// Creates a banner of the given type at the given position
	public void createBanner (MoPubAdPosition position)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_bannerPlugin.Call ("createBanner", (int)position);
	}


	// Destroys the banner and removes it from view
	public void destroyBanner ()
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_bannerPlugin.Call ("destroyBanner");
	}


	// Shows/hides the banner
	public void showBanner (bool shouldShow)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_bannerPlugin.Call ("hideBanner", !shouldShow);
	}


	// Sets the keywords for the current banner
	public void setBannerKeywords (string keywords)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_bannerPlugin.Call ("setBannerKeywords", keywords);
	}
}

#endif