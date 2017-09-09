using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ShopItem {

	public string Type = "";

	public string Name = "";

	public string PurchaseType = ""; // rewardedVideo, cash, gold

	public decimal Cost = 1;

	public string RewardType = ""; // gold, hint, item, boost, exp

	public int RewardAmount = 1;

}
