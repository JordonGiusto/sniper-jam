using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactBehavior : MonoBehaviour
{
    [SerializeField]
    AudioClip[] audioClips;

    void Start()
    {
        AudioClip audio = audioClips[(int)(0.99f * audioClips.Length * Random.value)];
        GetComponent<AudioSource>().clip = audio;
        GetComponent<AudioSource>().Play();
        Destroy(gameObject, 1.5f);
    }
}
