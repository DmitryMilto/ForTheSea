using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TimersAudio : MonoBehaviour
{
    [SerializeField, Required] private AudioSource source;
    [SerializeField, Min(0)] private float timeThreshold = 3;

    private Queue<Coroutine> queue = new();
    private IEnumerator _tracker;
    public void PlaySoundBeforeTimerEnds(float timerDuration)
    {
        if(_tracker != null)
        {
            StopCoroutine(_tracker);
        }
        _tracker = Tracker(timerDuration);
        queue.Enqueue(StartCoroutine(_tracker));
    }
    public void Stop()
    {
        if(source.isPlaying)
            source.Stop();
    }

    private IEnumerator Tracker(float timerDuration)
    {
        yield return new WaitForSeconds(timerDuration - timeThreshold);
        source.Play();
    }
}
