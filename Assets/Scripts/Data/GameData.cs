using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class GameData {

	public long UserID;
	public string AccessToken;

	public string FacebookID;
	public string FacebookName;
	public string FacebookPhotoURL;

	public List<FacebookUser> FacebookFriends;

	public long Point;
	public long Gold;

	public int GoldPerSec;

	public int CollectGoldLevel;

	public int Damage;

	public int Defense;

	public int Health;

	public int DamageLevel;

	public int DefenseLevel;

	public int HealthLevel;

	public int Level;

	public int Exp;

	public int OrcKillCount;

	public List<QuestInstance> QuestList;

}
