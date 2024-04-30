using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineTestController : MonoBehaviour
{
    public PlayableDirector playableDirector;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayTimeline();
        }
    }

    public void PlayTimeline()
    {
        if (playableDirector)
        {
            playableDirector.Play();
        }
    }

    public void PauseTimeline()
    {
        if (playableDirector)
        {
            playableDirector.Pause();
        }
    }
}
