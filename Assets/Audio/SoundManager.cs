using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource m_audioSource;
    //public AudioClip m_audioClip;
    public float volumeLerpTime;
    private bool startAudio;
    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        startAudio = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(startAudio == true)
        {
            float toVol = 1;
            float curVol = Mathf.Lerp(m_audioSource.volume, toVol, 2*volumeLerpTime);
            m_audioSource.volume = curVol;
        }
        if(startAudio == false)
        {
            float toVol = 0;
            float curVol = Mathf.Lerp(m_audioSource.volume, toVol, volumeLerpTime);
            m_audioSource.volume = curVol;
        }
    }

    public void StartAudio( )
    {
        startAudio = true;
    }
    
    public void stopAudio()
    {
        startAudio = false;
    }
}
