using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class LoginController : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoginFacebook(){
		
//		FB.Init ( delegate() {
//			
//			FB.LogInWithReadPermissions(
//
//				new List<string>(){"public_profile", "email", "user_friends"},
//
//				delegate(ILoginResult result) {
//				
//					if(result.Error == null){
//
//						LoadFacebookMe();
//
//						//Application.LoadLevel("Game");
//					}
//
//					Debug.Log(result.RawResult);
//
//				});
//			
//		});
	}

	public void LoadFacebookMe(){
//		FB.API ("/me", HttpMethod.GET, delegate(IGraphResult result) {
//			Debug.Log(result.RawResult);
//			Debug.Log(result.ResultDictionary["name"]);
//			Debug.Log(result.ResultDictionary["id"]);
//			Debug.Log("https://graph.facebook.com/" + result.ResultDictionary["id"] + "/picture");
//			LoadFacebookFriends();
//		});
	}

	public void LoadFacebookFriends(){
	
//		FB.API ("/me/friends", HttpMethod.GET, delegate(IGraphResult result) {
//			Debug.Log(result.RawResult);
//		});

	}

}
