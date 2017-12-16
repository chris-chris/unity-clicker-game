using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour {

	public string MonsterName;
	public int Health = 100;
	public int MaxHealth = 100;

	// Use this for initialization
	void Start () {
		NotificationCenter.Instance.Add ("PlayerAttack", this.OnDamage);
	}

	public void OnDamage() {
		
		Health = Health - DataController.Instance.gameData.Damage;
		if (Health <= 0) {
			OnDie ();
		}
	}

	public void OnDie() {

		AudioManager.Instance.PlaySFX ("Coin");

		NotificationCenter.Instance.Delete ("PlayerAttack", this.OnDamage);
		NotificationCenter.Instance.Notify ("MonsterDie");

		if ("Orc".Equals (MonsterName)) {
			DataController.Instance.gameData.OrcKillCount++;
			NotificationCenter.Instance.Notify ("OrcKill");
		}

		MonsterPool.Instance.ReleaseObject (gameObject);
	}
}
