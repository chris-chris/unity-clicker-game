using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

public class GameController : MonoBehaviour {

	public Text TextGold;
	public Camera MainCamera;
	public GameObject EffectSpark;
	public AudioClip SFXClick;
	public Transform Tap1Content;
	public Transform Tap4Content;

	public static GameController Instance;

	#if UNITY_IOS
	private string gameId = "1537701";
	#elif UNITY_ANDROID
	private string gameId = "1537702";
	#endif

	// Use this for initialization
	void Start () {
		Instance = this;
		TextGold.text = DataController.Instance.gameData.Gold.ToString();
		StartCoroutine (StartCollectGold ());
		InitTap ();

		if (Advertisement.isSupported) {
			Advertisement.Initialize (gameId);
		}

		Firebase.Analytics.FirebaseAnalytics.LogEvent(
			Firebase.Analytics.FirebaseAnalytics.EventSelectContent,
			new Firebase.Analytics.Parameter[] {
				new Firebase.Analytics.Parameter(
					Firebase.Analytics.FirebaseAnalytics.ParameterItemName, "name"),
				new Firebase.Analytics.Parameter(
					Firebase.Analytics.FirebaseAnalytics.UserPropertySignUpMethod, "Google"),
			}
		);
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

		List<Stat> statList = DataController.Instance.metaData.StatList;

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

		List<Stat> statList = DataController.Instance.metaData.StatList;

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

		List<Stat> statList = DataController.Instance.metaData.StatList;

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

	public void RewardedVideo10000(){

		ShowRewardedVideo ();

	}

	public void InitTap(){

		MetaData metaData = DataController.Instance.metaData;

		int i = 0;

		foreach (Stat stat in metaData.StatList) {

			GameObject item = Resources.Load ("Prefabs/Item") as GameObject;

			GameObject obj = Instantiate (item, Tap1Content);

			RectTransform rt = obj.GetComponent<RectTransform> ();
			rt.anchoredPosition = new Vector2 (0f, -80f + ( i * -160f));

			//obj.GetComponent<RectTransform> ().rect = rect;

			obj.name = stat.Type;

			obj.GetComponent<TapItem> ().StatType = stat.Type;

			i++;


		}

		i = 0;
		foreach (ShopItem shopItem in metaData.ShopItemList) {

			GameObject item = Resources.Load ("Prefabs/Item") as GameObject;

			GameObject obj = Instantiate (item, Tap4Content);

			RectTransform rt = obj.GetComponent<RectTransform> ();
			rt.anchoredPosition = new Vector2 (0f, -80f + ( i * -160f));

			//obj.GetComponent<RectTransform> ().rect = rect;

			obj.name = shopItem.Type;

			obj.GetComponent<TapItem> ().StatType = shopItem.Type;

			Debug.Log (shopItem.Type);

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

			List<Stat> statList = DataController.Instance.metaData.StatList;

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

		TapItem[] items4 = Tap4Content.GetComponentsInChildren<TapItem> ();

		foreach (TapItem item in items4) {

			GameObject obj = item.gameObject;

			ShopItem shopItem = null;

			List<ShopItem> shopItemList = DataController.Instance.metaData.ShopItemList;

			foreach (ShopItem s in shopItemList) {
				if (s.Type == obj.name) {
					shopItem = s;
				}
			}

			if (shopItem == null) {
				continue;
			}

			Text[] texts = obj.GetComponentsInChildren<Text> ();

			foreach (Text text in texts) {
				if (text.tag == "Description") {

					String upgradeText = String.Format("{0}", 
						shopItem.Name
					);
					text.text = upgradeText;
				}

			}
		}


	}

	void ShowRewardedVideo ()
	{
		var options = new ShowOptions();
		options.resultCallback = HandleShowResult;

		Advertisement.Show("rewardedVideo", options);
	}

	void HandleShowResult (ShowResult result)
	{
		if(result == ShowResult.Finished) {
			Debug.Log("Video completed - Offer a reward to the player");
			DataController.Instance.gameData.Gold += 10000;
			DataController.Instance.SaveGameData ();

		}else if(result == ShowResult.Skipped) {
			Debug.LogWarning("Video was skipped - Do NOT reward the player");

		}else if(result == ShowResult.Failed) {
			Debug.LogError("Video failed to show");
		}
	}

	public void PurchaseComplete(Product p)
	{
		Debug.Log (p.metadata.localizedTitle + " purchase success!");
		if (p.definition.id == "gold100000") {
			DataController.Instance.gameData.Gold += 100000;
		}
	}

}
