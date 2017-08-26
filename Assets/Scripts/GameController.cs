using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public Text TextGold;

	// Use this for initialization
	void Start () {
		TextGold.text = DataController.Instance.Gold.ToString();
		StartCoroutine (StartCollectGold ());
	}

	IEnumerator StartCollectGold() {

		while (true) {
		
			yield return new WaitForSecondsRealtime (1f);
			DataController.Instance.Gold += DataController.Instance.GoldPerSec;
			TextGold.text = DataController.Instance.Gold.ToString();

		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
