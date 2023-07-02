using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Octopus_TrashCan_ShuffleController : SerializedMonoBehaviour
{
    [SerializeField, FoldoutGroup("Settings")] private bool shuffleCanStaySame = true;
    [OdinSerialize] private List<IOctopusShuffable> shuffableItems;

    private Coroutine shuffle;

    [Button, EnableIf("@UnityEngine.Application.isPlaying")] public void ShuffleAll() => shuffle = StartCoroutine(ShuffleRoutine());

    private IEnumerator ShuffleRoutine()
    {
        HideAll();
        // Wait all to hide
        yield return new WaitForSeconds(shuffableItems.Max(item => item.PreShuffleDelay));

        ShuffleImmediately();

        ShowAll();
        // Wait all to show
        yield return new WaitForSeconds(shuffableItems.Max(item => item.PostShuffleDelay));
    }

    public void HideAll()
    {
        // Hide all
        shuffableItems.ForEach(item => item.OnStartShuffle());
    }

    public void ShowAll()
    {
        // Show all
        shuffableItems.ForEach(item => item.OnEndShuffle());
    }

    public void ShuffleImmediately()
    {
        List<TransitionData> transitions = new();

        foreach (var item in shuffableItems)
        {
            var allAnchors = shuffableItems.ConvertAll(input => input.Target.parent);
            var allFreeAnchors = allAnchors.Except(transitions.ConvertAll(input => input.Destination)).ToList();
            transitions.Add(new TransitionData(item.Target, GetRandomDestination(item.Target, allFreeAnchors)));
        }

        //Shuffle
        transitions.ForEach(data => data.Execute());

        Transform GetRandomDestination(Transform itemPosition, List<Transform> availableTransforms)
        {
            Transform candidatePosition;
            do candidatePosition = availableTransforms[UnityEngine.Random.Range(0, availableTransforms.Count)];
            while (shuffleCanStaySame is false && candidatePosition == itemPosition && availableTransforms.Count > 1);
            return candidatePosition;
        }
    }

    [Serializable] private class TransitionData
    {
        /// <summary>
        /// GameObject we want to move
        /// </summary>
        [SerializeField] private Transform target;
        /// <summary>
        /// Place, where we want to Place Object
        /// </summary>
        [SerializeField] public Transform Destination { get; private set; }

        public TransitionData(Transform target, Transform destination)
        {
            this.target = target;
            Destination = destination;
        }

        public void Execute() => target.SetParent(Destination);
    }
}
