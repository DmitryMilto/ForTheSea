using UnityEngine;

public interface IOctopusShuffable
{
    public RectTransform Target { get; }
    public float PreShuffleDelay { get; }
    public float PostShuffleDelay { get; }

    public void OnStartShuffle();
    public void OnEndShuffle();
}
