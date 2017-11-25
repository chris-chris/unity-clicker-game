using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldCounter : MonoBehaviour {

	public int Gold = 0;
	public Text TextGold;

	// Use this for initialization
	void Start () {
		StartCoroutine (StartGoldCount ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator StartGoldCount(){
		while (true) {
			yield return new WaitForSecondsRealtime (1f);
			Gold++;
			TextGold.text = Gold.ToString ();
		}
	}
}
