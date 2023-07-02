using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandom : MonoBehaviour
{
    [SerializeField, ValidateInput("@options != null && options?.Count > 0 && options?[0] != null", "Collection must have at least one element")] private List<AudioClip> options = new();
    [SerializeField, Required] private AudioSource source;

    public void PlayRandom() => source.PlayOneShot(options[Random.Range(0, options.Count)]);
}
