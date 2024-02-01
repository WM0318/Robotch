using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip bgm;
    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = bgm;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
