using UnityEngine;
using System.Collections.Generic;


#if UNITY_ANDROID

public enum MoPubLocationAwareness
{
	TRUNCATED,
	DISABLED,
	NORMAL
}


public class MoPubAndroid
{
	private static readonly AndroidJavaClass _pluginClass = new AndroidJavaClass ("com.mopub.unity.MoPubUnityPlugin");


	// Add Facebook test device ID
	public static void addFacebookTestDeviceId (string hashedDeviceId)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_pluginClass.CallStatic ("addFacebookTestDeviceId", hashedDeviceId);
	}


	// Enables/disables location support for banners and interstitials
	public static void setLocationAwareness (MoPubLocationAwareness locationAwareness)
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_pluginClass.CallStatic ("setLocationAwareness", locationAwareness.ToString ());
	}


	// Reports an app download to MoPub
	public static void reportApplicationOpen ()
	{
		if (Application.platform != RuntimePlatform.Android)
			return;

		_pluginClass.CallStatic ("reportApplicationOpen");
	}
}

#endif