using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillStar : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Destroy (gameObject, 0.7f);

	}

	public void OnEnable() {

		StartCoroutine (StartReturnObject ());

	}

	IEnumerator StartReturnObject(){
		yield return new WaitForSecondsRealtime (0.7f);
		ClickEffectPool.Instance.ReleaseObject (gameObject);
	}

}
