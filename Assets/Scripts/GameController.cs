using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameController : MonoBehaviour {

	public Text TextGold;
	public Camera MainCamera;
	public GameObject EffectSpark;
	public AudioClip SFXClick;
	public Text TextUpgradeCollectGold;

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

		if (Input.GetMouseButtonDown (0)) {
			
			DataController.Instance.Gold += DataController.Instance.GoldPerSec;
			TextGold.text = DataController.Instance.Gold.ToString();

			Ray ray = MainCamera.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100f)) {
				Debug.Log (hit.point);
				Debug.DrawLine (ray.origin, hit.point, Color.red);
				Instantiate (EffectSpark, hit.point, EffectSpark.transform.rotation);
				MainCamera.gameObject.GetComponent<AudioSource> ().PlayOneShot (SFXClick);
			}
			
		}

	}

	public void UpgradeCollectGold(){

		int Cost = DataController.Instance.CollectGoldLevel * DataController.Instance.CollectGoldLevel;

		if (DataController.Instance.Gold < Cost) {
			
			return;

		}

		DataController.Instance.CollectGoldLevel += 1;
		DataController.Instance.GoldPerSec = DataController.Instance.CollectGoldLevel;
		DataController.Instance.Gold -= Cost;
		TextGold.text = DataController.Instance.Gold.ToString();

		Cost = DataController.Instance.CollectGoldLevel * DataController.Instance.CollectGoldLevel;
		String upgradeText = String.Format("골드수집 속도 향상\n현재 : {0} 가격 : {1} Gold", 
			DataController.Instance.CollectGoldLevel, Cost
			);
		TextUpgradeCollectGold.text = upgradeText;



	}

}
