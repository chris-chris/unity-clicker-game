using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapItem : MonoBehaviour {

	public string StatType = "";

	public void OnClickButton(){

		if (StatType == "CollectGold") {
			GameController.Instance.UpgradeCollectGold ();
		} else if (StatType == "StatDamage") {
			GameController.Instance.UpgradeDamage ();
		} else if (StatType == "StatHealth") {
			GameController.Instance.UpgradeHealth ();
		} else if (StatType == "RewardedVideo10000") {
			GameController.Instance.RewardedVideo10000 ();
		}
	

	}

}
