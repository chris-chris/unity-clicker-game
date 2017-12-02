using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldCounter : MonoBehaviour {

	public Text TextGold;

	// Use this for initialization
	void Start () {

		TextGold.text = DataController.Instance.gameData.Gold.ToString ();
		StartCoroutine (StartGoldCount ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator StartGoldCount(){
		while (true) {
			yield return new WaitForSecondsRealtime (1f);
			DataController.Instance.gameData.Gold += DataController.Instance.gameData.GoldPerSec;
			TextGold.text = DataController.Instance.gameData.Gold.ToString ();

			DataController.Instance.SaveGameData ();
		}
	}
}
