using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource ambienceSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip music;
    public AudioClip ambience; 

    private void Start()
    {
        musicSource.clip = music;
        ambienceSource.clip = ambience;
        musicSource.Play();
        ambienceSource.Play();
    }

    

}
