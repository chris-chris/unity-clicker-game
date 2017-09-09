using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


#if UNITY_IPHONE || UNITY_ANDROID
public class MoPubManager : MonoBehaviour
{
	// Fired when an ad loads in the banner. Includes the ad height.
	public static event Action<float> onAdLoadedEvent;

	// Fired when an ad fails to load for the banner
	public static event Action<string> onAdFailedEvent;

	// Android only. Fired when a banner ad is clicked
	public static event Action<string> onAdClickedEvent;

	// Android only. Fired when a banner ad expands to encompass a greater portion of the screen
	public static event Action<string> onAdExpandedEvent;

	// Android only. Fired when a banner ad collapses back to its initial size
	public static event Action<string> onAdCollapsedEvent;

	// Fired when an interstitial ad is loaded and ready to be shown
	public static event Action<string> onInterstitialLoadedEvent;

	// Fired when an interstitial ad fails to load
	public static event Action<string> onInterstitialFailedEvent;

	// Fired when an interstitial ad is dismissed
	public static event Action<string> onInterstitialDismissedEvent;

	// iOS only. Fired when an interstitial ad expires
	public static event Action<string> onInterstitialExpiredEvent;

	// Android only. Fired when an interstitial ad is displayed
	public static event Action<string> onInterstitialShownEvent;

	// Android only. Fired when an interstitial ad is clicked
	public static event Action<string> onInterstitialClickedEvent;

	// Fired when a rewarded video finishes loading and is ready to be displayed
	public static event Action<string> onRewardedVideoLoadedEvent;

	// Fired when a rewarded video fails to load. Includes the error message.
	public static event Action<string> onRewardedVideoFailedEvent;

	// iOS only. Fired when a rewarded video expires
	public static event Action<string> onRewardedVideoExpiredEvent;

	// Fired when an rewarded video is displayed
	public static event Action<string> onRewardedVideoShownEvent;

	// Fired when an rewarded video is clicked
	public static event Action<string> onRewardedVideoClickedEvent;

	// Fired when a rewarded video fails to play. Includes the error message.
	public static event Action<string> onRewardedVideoFailedToPlayEvent;

	// Fired when a rewarded video completes. Includes all the data available about the reward.
	public static event Action<RewardedVideoData> onRewardedVideoReceivedRewardEvent;

	// Fired when a rewarded video closes
	public static event Action<string> onRewardedVideoClosedEvent;

	// iOS only. Fired when a rewarded video event causes another application to open
	public static event Action<string> onRewardedVideoLeavingApplicationEvent;



	public class RewardedVideoData
	{
		public string adUnitId;
		public string currencyType;
		public float amount;


		public RewardedVideoData (string json)
		{
			var obj = MoPubInternal.ThirdParty.MiniJSON.Json.Deserialize (json) as Dictionary<string,object>;
			if (obj == null)
				return;

			if (obj.ContainsKey ("adUnitId"))
				adUnitId = obj ["adUnitId"].ToString ();

			if (obj.ContainsKey ("currencyType"))
				currencyType = obj ["currencyType"].ToString ();

			if (obj.ContainsKey ("amount"))
				amount = float.Parse (obj ["amount"].ToString ());
		}


		public override string ToString ()
		{
			return string.Format ("adUnitId: {0}, currencyType: {1}, amount: {2}", adUnitId, currencyType, amount);
		}
	}



	public class MoPubReward
	{
		private readonly string _label;
		private readonly int _amount;

		public MoPubReward (string label, int amount)
		{
			this._label = label;
			this._amount = amount;
		}

		public string Label
		{
			get { return _label; }
		}

		public int Amount
		{
			get { return _amount; }
		}

		public override string ToString ()
		{
			return string.Format ("\"{0} {1}\"", Amount, Label);
		}
	}



	static MoPubManager ()
	{
		var type = typeof(MoPubManager);
		try {
// first we see if we already exist in the scene
			var obj = FindObjectOfType (type) as MonoBehaviour;
			if (obj != null)
				return;

// create a new GO for our manager
			var managerGO = new GameObject (type.ToString ());
			managerGO.AddComponent (type);
			DontDestroyOnLoad (managerGO);
		} catch (UnityException) {
			Debug.LogWarning ("It looks like you have the " + type +
				" on a GameObject in your scene. Please remove the script from your scene.");
		}
	}


	// Banner Listeners

	void onAdLoaded (string height)
	{
		if (onAdLoadedEvent != null)
			onAdLoadedEvent (float.Parse (height));
	}


	void onAdFailed (string errorMsg)
	{
		if (onAdFailedEvent != null)
			onAdFailedEvent (errorMsg);
	}


	void onAdClicked (string adUnitId)
	{
		if (onAdClickedEvent != null)
			onAdClickedEvent (adUnitId);
	}


	void onAdExpanded (string adUnitId)
	{
		if (onAdExpandedEvent != null)
			onAdExpandedEvent (adUnitId);
	}


	void onAdCollapsed (string adUnitId)
	{
		if (onAdCollapsedEvent != null)
			onAdCollapsedEvent (adUnitId);
	}


	// Interstitial Listeners

	void onInterstitialLoaded (string adUnitId)
	{
		if (onInterstitialLoadedEvent != null)
			onInterstitialLoadedEvent (adUnitId);
	}


	void onInterstitialFailed (string errorMsg)
	{
		if (onInterstitialFailedEvent != null)
			onInterstitialFailedEvent (errorMsg);
	}


	void onInterstitialDismissed (string adUnitId)
	{
		if (onInterstitialDismissedEvent != null)
			onInterstitialDismissedEvent (adUnitId);
	}


	void interstitialDidExpire (string adUnitId)
	{
		if (onInterstitialExpiredEvent != null)
			onInterstitialExpiredEvent (adUnitId);
	}


	void onInterstitialShown (string adUnitId)
	{
		if (onInterstitialShownEvent != null)
			onInterstitialShownEvent (adUnitId);
	}


	void onInterstitialClicked (string adUnitId)
	{
		if (onInterstitialClickedEvent != null)
			onInterstitialClickedEvent (adUnitId);
	}


	// Rewarded Video Listeners

	void onRewardedVideoLoaded (string adUnitId)
	{
		if (onRewardedVideoLoadedEvent != null)
			onRewardedVideoLoadedEvent (adUnitId);
	}


	void onRewardedVideoFailed (string errorMsg)
	{
		if (onRewardedVideoFailedEvent != null)
			onRewardedVideoFailedEvent (errorMsg);
	}


	void onRewardedVideoExpired (string adUnitId)
	{
		if (onRewardedVideoExpiredEvent != null)
			onRewardedVideoExpiredEvent (adUnitId);
	}


	void onRewardedVideoShown (string adUnitId)
	{
		if (onRewardedVideoShownEvent != null)
			onRewardedVideoShownEvent (adUnitId);
	}


	void onRewardedVideoClicked (string adUnitId)
	{
		if (onRewardedVideoClickedEvent != null)
			onRewardedVideoClickedEvent (adUnitId);
	}


	void onRewardedVideoFailedToPlay (string errorMsg)
	{
		if (onRewardedVideoFailedToPlayEvent != null)
			onRewardedVideoFailedToPlayEvent (errorMsg);
	}


	void onRewardedVideoReceivedReward (string json)
	{
		if (onRewardedVideoReceivedRewardEvent != null)
			onRewardedVideoReceivedRewardEvent (new RewardedVideoData (json));
	}


	void onRewardedVideoClosed (string adUnitId)
	{
		if (onRewardedVideoClosedEvent != null)
			onRewardedVideoClosedEvent (adUnitId);
	}


	void onRewardedVideoLeavingApplication (string adUnitId)
	{
		if (onRewardedVideoLeavingApplicationEvent != null)
			onRewardedVideoLeavingApplicationEvent (adUnitId);
	}
}
#endif
