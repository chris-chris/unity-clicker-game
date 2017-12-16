using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class HTTPClient : MonoBehaviour {
	
	static GameObject _container;
	static GameObject Container {
		get {
			return _container;
		}
	}
	
	static HTTPClient _instance;
	public static HTTPClient Instance {
		get {
			if( ! _instance ) {
				_container = new GameObject();
				_container.name = "HTTPClient";
				_instance = _container.AddComponent( typeof(HTTPClient) ) as HTTPClient;
			}
			
			return _instance;
		}
	}

	public void GET(string url, Action<WWW> callback) {

		WWW www = new WWW(url);
		StartCoroutine(WaitWWW(www, callback));

	}
	
	public void POST(string url, string input, Action<WWW> callback) {

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json");
		byte[] body = Encoding.UTF8.GetBytes(input);

		WWW www = new WWW(url, body, headers);

		StartCoroutine(WaitWWW(www, callback));

	}

	public IEnumerator WaitWWW(WWW www, Action<WWW> callback)
	{
		yield return www;
		callback(www);
	}

}
