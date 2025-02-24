using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] List<SoundData> soundData;
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
    }

    public void Play(string key)
    {
        var data = soundData.Find(x => x.key == key);
        if (data == null)
        {
            Debug.LogError("Sound not found: " + key);
            return;
        }
        audioSource.PlayOneShot(data.clip);
    }


}

[Serializable]
class SoundData
{
    public string key;
    public AudioClip clip;
}
