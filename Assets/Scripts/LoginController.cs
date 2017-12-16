using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.Unity;

public class LoginController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClickLogin(){


		if (FB.IsInitialized) {
			FacebookLogin ();
		} else {
			FB.Init (delegate() {
				FacebookLogin();
			});
		}

	}

	public void FacebookLogin(){
		FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, 
			delegate(ILoginResult result) {
				Debug.Log(result.RawResult);
				FacebookLoadMe();
			});
	}

	public void FacebookLoadMe(){
		FB.API ("/me", HttpMethod.GET, delegate(IGraphResult result) {
			Debug.Log(result.RawResult);
			FacebookUser fbResult = JsonUtility.FromJson<FacebookUser>(result.RawResult);

			DataController.Instance.gameData.FacebookID = fbResult.id;
			DataController.Instance.gameData.FacebookName = fbResult.name;
			DataController.Instance.gameData.FacebookPhotoURL =
				"http://graph.facebook.com/" + fbResult.id + "/picture?type=square";

			FacebookLoadFriends();
		});
	}
	/*
{
	"data":
	[
		{"name":"Chris Song","id":"137418009931831"}
	],
	"paging":{"next":"https:\/\/graph.facebook.com\/v2.2\/10204997009661738\/friends?access_token=CAAUImeIGMdEBAHmbhkz25DFS8dsJCwlCVpzDbHEjmhcGIKe3S8xzkUGUDp7ebNusQLAWOF5vG6LBsiKytu27RR1v1TOkooQXlSzDvQShZBZCICIn2ySQdn7VbgurfBsw98gZAWMUmDvhwJZAdMMgmOamesWwudy7UTWqpjbBnmRTPxTjEIGiJpABWNtgLAldx71FIO8xGTbakudCfZCxR&limit=25&offset=25&__after_id=enc_AdC1zqmTQJITqEeR4rWCWOJTZArTf1aCACsU4ywiR5TJD6oLORQ64DdkN3sIEJTME0gKG3kYDnlZBiIfk3ZAbv8ibKr"},
	"summary":{"total_count":1108}
}
	 * */
	public void FacebookLoadFriends(){

		FB.API ("/me/friends", HttpMethod.GET, delegate(IGraphResult result) {
			Debug.Log(result.RawResult);
			FacebookFriendResult fbResult = JsonUtility.FromJson<FacebookFriendResult>(result.RawResult);
			DataController.Instance.gameData.FacebookFriends = fbResult.data;
			DataController.Instance.SaveGameData();

			GameServerLogin();

		});
	}

	public void GameServerLogin()
	{
		string url = "http://unity.chris-chris.ai/Login/Facebook";
		string body = JsonUtility.ToJson (DataController.Instance.gameData);
		Debug.Log (body);
		HTTPClient.Instance.POST (url, body, delegate(WWW www) {
			ServerLoginResult serverResult = JsonUtility.FromJson<ServerLoginResult>(www.text);
			DataController.Instance.gameData.UserID = serverResult.Data.UserID;
			DataController.Instance.gameData.AccessToken = serverResult.Data.AccessToken;
			DataController.Instance.gameData.Gold = serverResult.Data.Point;
			SceneManager.LoadScene ("Game");
		});
	}

	public void MoveGame(){
		SceneManager.LoadScene ("Game");
	}
}
