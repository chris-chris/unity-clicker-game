using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class GoogleLoginController : MonoBehaviour {

	public Text text;

	// Use this for initialization
	void Start () {
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			// enables saving game progress.
			.EnableSavedGames()
			// requests the email address of the player be available.
			// Will bring up a prompt for consent.
			.RequestEmail()
			// requests a server auth code be generated so it can be passed to an
			//  associated back end server application and exchanged for an OAuth token.
			.RequestServerAuthCode(false)
			// requests an ID token be generated.  This OAuth token can be used to
			//  identify the player to other services such as Firebase.
			.RequestIdToken()
			.Build();

		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();

		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			Debug.Log(success);
		});

		Debug.Log (Social.localUser.userName);
		text.text = string.Format ("Google Play ID: {0} Name : {1}", 
			Social.localUser.id,
			Social.localUser.userName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
