using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public Text TextName;
	public RawImage FacebookPhoto;

	// Use this for initialization
	void Start () {
		// DataController.Instance.ResetGameData ();

		AudioManager.Instance.Init();

		MonsterPool.Instance.Init ();
		ClickEffectPool.Instance.Init ();

		TextName.text = DataController.Instance.gameData.FacebookName;
		StartCoroutine(StartLoadFacebookPhoto ());

	}

	IEnumerator StartLoadFacebookPhoto(){

		WWW www = new WWW (DataController.Instance.gameData.FacebookPhotoURL);
		yield return www;
		FacebookPhoto.texture = www.texture;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
