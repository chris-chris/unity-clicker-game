using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {


	// Singleton class start
	static GameObject _container;
	static GameObject Container {
		get {
			return _container;
		}
	}

	static AudioManager _instance;
	public static AudioManager Instance {
		get {
			if( ! _instance ) {
				_container = new GameObject();
				_container.name = "AudioManager";
				_instance = _container.AddComponent( typeof(AudioManager) ) as AudioManager;
				_container.AddComponent<AudioSource> ();
				DontDestroyOnLoad (_container);
			}

			return _instance;
		}
	}
	// Singleton class end

	public AudioSource audioSource;

	public string CurrentBGM = "01";

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource> ();
		PlayBGM ();
	}

	public void Init() {
		
	}

	public void PlayBGM(string BGM = "01"){
		CurrentBGM = BGM;
		AudioClip bgm = Resources.Load<AudioClip> ("BGM/" + CurrentBGM);
		audioSource.clip = bgm;
		audioSource.loop = true;
		audioSource.Play ();
	}

	public void PlaySFX(string SFX){
		AudioClip sfx = Resources.Load<AudioClip> ("SFX/" + SFX);
		audioSource.PlayOneShot (sfx);
	}

}
