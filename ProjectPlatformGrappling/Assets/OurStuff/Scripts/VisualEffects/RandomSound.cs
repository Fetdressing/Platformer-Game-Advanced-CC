using UnityEngine;
using System.Collections;

public class RandomSound : MonoBehaviour {
    public bool playOnAwake = true;
    public AudioSource aSource;
    public AudioClip[] sounds;
    public float cameraShakeDistance = 0;
    public float cameraShakeDur = 0.8f;
    public float cameraShakeMag = 2.0f;

    Transform player;
    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (aSource == null)
        {
            aSource = GetComponent<AudioSource>();
        }
    }

    void Awake () {
        if (!playOnAwake) return;

        if (aSource == null)
        {
            aSource = GetComponent<AudioSource>();
        }

        int random = Random.Range(0, sounds.Length);

        aSource.GetComponent<AudioSource>().PlayOneShot(sounds[random]);
       
	}

    public void Play()
    {
        Camera currCamera = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().currCamera;
        if(Vector3.Distance(transform.position, currCamera.transform.position) < cameraShakeDistance)
        {
            float shakeValue = Mathf.Max( 0.3f, (1 - Vector3.Distance(transform.position, currCamera.transform.position) / cameraShakeDistance));
            currCamera.transform.GetComponent<CameraShaker>().ShakeCamera(shakeValue * cameraShakeDur, shakeValue * cameraShakeMag, true);
        }

        int random = Random.Range(0, sounds.Length);
        aSource.PlayOneShot(sounds[random]);
    }
}
