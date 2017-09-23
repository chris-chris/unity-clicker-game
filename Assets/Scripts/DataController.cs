using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataController : MonoBehaviour {


	// Singleton class start
	static GameObject _container;
	static GameObject Container {
		get {
			return _container;
		}
	}

	static DataController _instance;
	public static DataController Instance {
		get {
			if( ! _instance ) {
				_container = new GameObject();
				_container.name = "DataController";
				_instance = _container.AddComponent( typeof(DataController) ) as DataController;
				DontDestroyOnLoad (_container);
			}

			return _instance;
		}
	}
	// Singleton class end

	public string gameDataProjectFilePath = "/game.json";

	GameData _gameData;
	public GameData gameData{
		get{
			if (_gameData == null) {
				LoadGameData ();
			}
			return _gameData;
		}
	}

	MetaData _metaData;
	public MetaData metaData{
		get{
			if (_metaData == null) {
				LoadMetaData ();
			}
			return _metaData;
		}
	}

	public void LoadMetaData(){
		TextAsset statJson = Resources.Load ("MetaData/Meta") as TextAsset;
		Debug.Log (statJson.text);
		_metaData = JsonUtility.FromJson<MetaData> (statJson.text);

		foreach (ShopItem shopItem in metaData.ShopItemList) {
			Debug.Log (shopItem.Name);
		}

	}


	public void LoadGameData()
	{
		string filePath = Application.persistentDataPath + gameDataProjectFilePath;

		if (File.Exists (filePath)) {
			Debug.Log ("loaded!");
			string dataAsJson = File.ReadAllText (filePath);
			_gameData = JsonUtility.FromJson<GameData> (dataAsJson);
		} else 
		{
			Debug.Log ("Create new");

			_gameData = new GameData ();
			_gameData.CollectGoldLevel = 1;
			_gameData.GoldPerSec = 1;
			_gameData.Gold = 0;
			_gameData.Health = 100;
			_gameData.Damage = 20;
			_gameData.Level = 1;
			_gameData.Exp = 0;

		}
	}

	public void SaveGameData()
	{

		string dataAsJson = JsonUtility.ToJson (gameData);

		string filePath = Application.persistentDataPath + gameDataProjectFilePath;
		File.WriteAllText (filePath, dataAsJson);

	}

	public void Start() {
		Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
		Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
	}

	public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
		UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
	}

	public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
		UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
	}
}
