using UnityEngine;
using System.Collections;

public class RandomSound : MonoBehaviour {
    public AudioClip[] sounds;
	// Use this for initialization
	void Awake () {
        int random = Random.Range(0, sounds.Length);
        GetComponent<AudioSource>().PlayOneShot(sounds[random]);
	}
}
