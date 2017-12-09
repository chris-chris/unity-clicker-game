using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopItemController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnPurchaseComplete(Product product){
		if ("gold10000".Equals (product.definition.id)) {
			DataController.Instance.gameData.Gold += 10000;
			DataController.Instance.SaveGameData ();
			NotificationCenter.Instance.Notify ("GoldUpdate");
		}
	}
}
