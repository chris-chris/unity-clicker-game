using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapItemController : MonoBehaviour {

	public string TapItemType;

	public Text TextDesc, TextStatus, TextButtonText;

	// Use this for initialization
	void Start () {
		UpdateStatus ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnButtonClick(){
		if ("CollectGold".Equals (TapItemType)) {
			int Cost = DataController.Instance.gameData.CollectGoldLevel
			           * DataController.Instance.gameData.CollectGoldLevel;
			if (DataController.Instance.gameData.Gold >= Cost) {
				DataController.Instance.gameData.Gold -= Cost;
				DataController.Instance.gameData.CollectGoldLevel++;
				DataController.Instance.gameData.GoldPerSec =
					DataController.Instance.gameData.CollectGoldLevel;
			}
		} else if ("Damage".Equals (TapItemType)) {
			int Cost = DataController.Instance.gameData.DamageLevel * 10;
			if (DataController.Instance.gameData.Gold >= Cost) {
				DataController.Instance.gameData.Gold -= Cost;
				DataController.Instance.gameData.DamageLevel++;
				DataController.Instance.gameData.Damage += 10;
			}
		}

		UpdateStatus ();
		DataController.Instance.SaveGameData ();
	}

	public void UpdateStatus(){
		if ("CollectGold".Equals (TapItemType)) {
			TextStatus.text = string.Format ("Lv{0} : {1}G / s",
				DataController.Instance.gameData.CollectGoldLevel,
				DataController.Instance.gameData.GoldPerSec);
			int Cost = DataController.Instance.gameData.CollectGoldLevel
			           * DataController.Instance.gameData.CollectGoldLevel;
			TextButtonText.text = string.Format ("{0}G", Cost);
		} else if ("Damage".Equals (TapItemType)) {
			TextStatus.text = string.Format ("Lv{0} : {1}", 
				DataController.Instance.gameData.DamageLevel,
				DataController.Instance.gameData.Damage);
			int Cost = DataController.Instance.gameData.DamageLevel * 10;
			TextButtonText.text = string.Format ("{0}G", Cost);
			
		}
		
	}

}
