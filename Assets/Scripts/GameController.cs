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
	public Transform Tap1Content;

	public static GameController Instance;

	// Use this for initialization
	void Start () {
		Instance = this;
		TextGold.text = DataController.Instance.gameData.Gold.ToString();
		StartCoroutine (StartCollectGold ());
		InitTap ();
	}

	IEnumerator StartCollectGold() {

		while (true) {
		
			yield return new WaitForSecondsRealtime (1f);
			DataController.Instance.gameData.Gold += DataController.Instance.gameData.GoldPerSec;
			TextGold.text = DataController.Instance.gameData.Gold.ToString();

			DataController.Instance.SaveGameData ();

		}

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (0)) {
			
			DataController.Instance.gameData.Gold += DataController.Instance.gameData.GoldPerSec;
			TextGold.text = DataController.Instance.gameData.Gold.ToString();

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
		int cost = 0;
		int currentLevel = DataController.Instance.gameData.CollectGoldLevel;

		Stat stat = null;

		List<Stat> statList = DataController.Instance.GetStatList ().StatList;

		foreach (Stat s in statList) {
			if (s.Type == "CollectGold") {
				stat = s;
			}
		}

		if (stat.CostUpgradeAmountOp == "*") {
			cost = DataController.Instance.gameData.CollectGoldLevel * stat.CostUpgradeAmount;
		} else {
			cost = stat.CostUpgradeAmount;
		}

		//int Cost = DataController.Instance.gameData.CollectGoldLevel * DataController.Instance.gameData.CollectGoldLevel;

		if (DataController.Instance.gameData.Gold < cost) {
			
			return;

		}

		DataController.Instance.gameData.CollectGoldLevel += 1;
		DataController.Instance.gameData.GoldPerSec =
			DataController.Instance.gameData.CollectGoldLevel * stat.UpgradeAmount;
		DataController.Instance.gameData.Gold -= cost;

		RefreshTap ();

	}

	public void UpgradeDamage(){
		int cost = 0;
		int currentLevel = DataController.Instance.gameData.DamageLevel;

		Stat stat = null;

		List<Stat> statList = DataController.Instance.GetStatList ().StatList;

		foreach (Stat s in statList) {
			if (s.Type == "StatDamage") {
				stat = s;
			}
		}

		if (stat.CostUpgradeAmountOp == "*") {
			cost = DataController.Instance.gameData.DamageLevel * stat.CostUpgradeAmount;
		} else {
			cost = stat.CostUpgradeAmount;
		}

		//int Cost = DataController.Instance.gameData.CollectGoldLevel * DataController.Instance.gameData.CollectGoldLevel;

		if (DataController.Instance.gameData.Gold < cost) {

			return;

		}

		DataController.Instance.gameData.DamageLevel += 1;
		DataController.Instance.gameData.Damage =
			DataController.Instance.gameData.DamageLevel * stat.UpgradeAmount;
		DataController.Instance.gameData.Gold -= cost;

		RefreshTap ();

	}

	public void UpgradeHealth(){
		int cost = 0;
		int currentLevel = DataController.Instance.gameData.HealthLevel;

		Stat stat = null;

		List<Stat> statList = DataController.Instance.GetStatList ().StatList;

		foreach (Stat s in statList) {
			if (s.Type == "StatHealth") {
				stat = s;
			}
		}

		if (stat.CostUpgradeAmountOp == "*") {
			cost = DataController.Instance.gameData.HealthLevel * stat.CostUpgradeAmount;
		} else {
			cost = stat.CostUpgradeAmount;
		}

		//int Cost = DataController.Instance.gameData.CollectGoldLevel * DataController.Instance.gameData.CollectGoldLevel;

		if (DataController.Instance.gameData.Gold < cost) {

			return;

		}

		DataController.Instance.gameData.HealthLevel += 1;
		DataController.Instance.gameData.Health =
			DataController.Instance.gameData.HealthLevel * stat.UpgradeAmount;
		DataController.Instance.gameData.Gold -= cost;

		RefreshTap ();

	}

	public void InitTap(){

		StatData statData = DataController.Instance.GetStatList ();

		int i = 0;

		foreach (Stat stat in statData.StatList) {

			GameObject item = Resources.Load ("Prefabs/Item") as GameObject;

			GameObject obj = Instantiate (item, Tap1Content);

			RectTransform rt = obj.GetComponent<RectTransform> ();
			rt.anchoredPosition = new Vector2 (0f, -80f + ( i * -160f));

			//obj.GetComponent<RectTransform> ().rect = rect;

			obj.name = stat.Type;

			obj.GetComponent<TapItem> ().StatType = stat.Type;

			i++;


		}

		RefreshTap ();
	}

	public void RefreshTap()
	{

		TapItem[] items = Tap1Content.GetComponentsInChildren<TapItem> ();

		foreach (TapItem item in items) {
			
			GameObject obj = item.gameObject;

			Stat stat = null;

			List<Stat> statList = DataController.Instance.GetStatList ().StatList;

			foreach (Stat s in statList) {
				if (s.Type == obj.name) {
					stat = s;
				}
			}

			if (stat == null) {
				continue;
			}

			Text[] texts = obj.GetComponentsInChildren<Text> ();

			foreach (Text text in texts) {
				if (text.tag == "Description") {

					int currentLevel = 0;
					int cost = 0;

					if (stat.Type == "CollectGold") {
						currentLevel = DataController.Instance.gameData.CollectGoldLevel;
						if (stat.CostUpgradeAmountOp == "*") {
							cost = DataController.Instance.gameData.CollectGoldLevel * stat.CostUpgradeAmount;
						} else {
							cost = stat.CostUpgradeAmount;
						}
					}else if (stat.Type == "StatDamage") {
						currentLevel = DataController.Instance.gameData.DamageLevel;
						if (stat.CostUpgradeAmountOp == "*") {
							cost = DataController.Instance.gameData.DamageLevel * stat.CostUpgradeAmount;
						} else {
							cost = stat.CostUpgradeAmount;
						}
					}else if (stat.Type == "StatHealth") {
						currentLevel = DataController.Instance.gameData.HealthLevel;
						if (stat.CostUpgradeAmountOp == "*") {
							cost = DataController.Instance.gameData.HealthLevel * stat.CostUpgradeAmount;
						} else {
							cost = stat.CostUpgradeAmount;
						}
					}

					String upgradeText = String.Format("{0}\n현재 : {1} 가격 : {2} Gold", 
						stat.Name, 
						currentLevel,
						cost
					);
					text.text = upgradeText;
				}

			}
		}


	}

}
