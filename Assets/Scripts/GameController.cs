using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// DataController.Instance.ResetGameData ();

		AudioManager.Instance.Init();

		MonsterPool.Instance.Init ();
		ClickEffectPool.Instance.Init ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
