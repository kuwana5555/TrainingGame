using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager1 : MonoBehaviour
{
    [Header("Change BGM")]
    [SerializeField] AudioClip changeBGM;
    [Header("ChangeTiming BGM Volume")]
    [SerializeField] public float changeBGMVolume = 0.5f;
    [Header("Fade Time")]
    [SerializeField] float fadeTime = 3f;

    AudioSource audioSource;
    public float bgmVolume = 0;
    float fadeDeltaTime = 0, fadeDeltaTimeReset = 0;
    float volumeMin = 0;
    bool isFadeIn = false, isFadeOut = false;
    bool changeBGMStart = false;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        bgmVolume = audioSource.volume;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!changeBGMStart)
        {
            if (audioSource == null || changeBGM == null)
            {
                return;
            }
        }

        if (isFadeOut)
        {
            fadeDeltaTime += Time.deltaTime;
            if (fadeDeltaTime >= fadeTime)
            {
                fadeDeltaTime = fadeTime;
                isFadeOut = false;
            }

            audioSource.volume = bgmVolume - (fadeDeltaTime / fadeTime);
            if (audioSource.volume <= volumeMin)
            {
                audioSource.volume = volumeMin;
                isFadeOut = false;
                isFadeIn = true;
                fadeDeltaTime = fadeDeltaTimeReset;
            }
        }

        if (isFadeIn)
        {
            fadeDeltaTime += Time.deltaTime;
            audioSource.clip = changeBGM;
            audioSource.Play();
            if (fadeDeltaTime >= fadeTime)
            {
                fadeDeltaTime = fadeTime;
                isFadeIn = false;
            }

            audioSource.volume = fadeDeltaTime / fadeTime;
            if (audioSource.volume >= changeBGMVolume)
            {
                audioSource.volume = changeBGMVolume;
                isFadeIn = false;
                fadeDeltaTime = fadeDeltaTimeReset;
                changeBGMStart = false;
            }
        }



    }

    public void ChangeBGMStart()
    {
        changeBGMStart = true;
        isFadeOut = true;
    }
}
