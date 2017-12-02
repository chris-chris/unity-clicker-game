using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour {

	public int Health = 100;

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
		NotificationCenter.Instance.Delete ("PlayerAttack", this.OnDamage);
		Destroy (gameObject);
	}
}
