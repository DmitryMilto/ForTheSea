using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameManager_Octopus : MinigameManager_Core
{
    [FoldoutGroup("Minigame settings"), SerializeField, AssetList] private List<OctopusMinigame_TrashType> PossibleTrashTypes;
    [FoldoutGroup("Minigame settings"), SerializeField, SceneObjectsOnly] private List<Octopus_DragableItem> SpawnPoints;

    [FoldoutGroup("Minigame settings/Spawn/Default Spawn Delay"), SerializeField, HideLabel] 
    private SpawnData DefaultSpawnData = new(5, 4);
    [FoldoutGroup("Minigame settings/Spawn/Default Spawn Delay"), SerializeField]
    private bool AllowRandomCount = true;
    [FoldoutGroup("Minigame settings/Spawn/Default Spawn Delay"), SerializeField, ShowIf(nameof(AllowRandomCount))]
    private bool AllowZeroCount = false;
    [FoldoutGroup("Minigame settings/Spawn/Default Spawn Delay"), SerializeField]
    private float DelayBetweenIterations = 0.5f;

    [BoxGroup("Minigame settings/Spawn"), SerializeField, TableList(DrawScrollView = true, ShowIndexLabels = true)]
    private List<SpawnData> OnStartSpawnData = new()
    {
        new(3, 1),
        new(2, 2),
        new(2, 2),
        new(4, 3),
        new(5, 4)
    };

    [BoxGroup("Minigame settings/Spawn"), SerializeField] private Slider timerVisual;

    [FoldoutGroup("Minigame settings/Trashcans"), SerializeField] private Octopus_TrashCan_ShuffleController shuffleController;

    [FoldoutGroup("Addons"), SerializeField] private TimersAudio timerAudio;

    private List<Octopus_DragableItem> FreeSpawnPoints => SpawnPoints.Where(item => item.CurrentlyHeldItem is null).ToList();
    private List<Octopus_DragableItem> BusySpawnPoints => SpawnPoints.Where(item => item.CurrentlyHeldItem is not null).ToList();

    private MinigameManagerAddon_Health_Restorable Health => AddonHealth as MinigameManagerAddon_Health_Restorable;

    public override void Awake()
    {
        base.Awake();
        AddonScore.LoadBestScore("Minigame/Octopus/Best score");
    }

    protected override IEnumerator GameRoutine()
    {
        CheckDecreaseHeartsOnMiss();
        if (isAllowedToRun) yield return new WaitForSeconds(startDelay);
        else yield return new WaitUntil(() => isAllowedToRun);

        int iteration = 0;
        TweenerCore<float, float, FloatOptions> timerTween = null;

        while (enabled)
        {
            yield return new WaitWhile(() => IsPaused || !isAllowedToRun);

            CheckDecreaseHeartsOnMiss();

            CheckIf_ItemsLeftOnTimeOut();
            timerTween?.Kill(true);

            if (iteration > 0) yield return new WaitForSeconds(DelayBetweenIterations);

            SpawnItems(iteration);

            if (iteration > 0) shuffleController.ShuffleAll();

            if (iteration > OnStartSpawnData.Count) currentDifficulty += difficultyIncreaseOverTime;
            iteration++;

            float delay = (iteration < OnStartSpawnData.Count
                ? OnStartSpawnData[iteration - 1].Delay
                : DefaultSpawnData.Delay) - (iteration > OnStartSpawnData.Count ? currentDifficulty : 0);

            timerVisual.minValue = 0;
            timerVisual.maxValue = delay;
            timerVisual.value = delay;
            timerTween = DOTween.To(() => timerVisual.value, value => timerVisual.value = value, 0, delay).SetEase(Ease.Linear);
            timerAudio.PlaySoundBeforeTimerEnds(delay);

            yield return WaitUntilTimeOutOrRoundEnd(delay);
        }

        IEnumerator WaitUntilTimeOutOrRoundEnd(float roundDuration)
        {
            // Check 4 times per second
            var checkFrequency = roundDuration * 4;
            // Check every 0.25s
            var checkDelay = roundDuration / checkFrequency;
            // Check in loop during whole round
            for (int i = 0; i < checkFrequency; i++)
            {
                if (BusySpawnPoints.Any()) yield return new WaitForSeconds(checkDelay);
            }
        }

        void SpawnItems(int iteration)
        {
            timerAudio.Stop();
            int spawnCount = iteration < OnStartSpawnData.Count ? OnStartSpawnData[iteration].SpawnCount :
                AllowRandomCount ? Random.Range(
                    AllowZeroCount ? 0 : 1,
                    DefaultSpawnData.SpawnCount + 1) :
                DefaultSpawnData.SpawnCount;

            for (int i = 0; i < spawnCount; i++)
            {
                var SpawnItem = PossibleTrashTypes[Random.Range(0, PossibleTrashTypes.Count)];
                if (FreeSpawnPoints.Count > 0)
                {
                    var index = Random.Range(0, FreeSpawnPoints.Count);
                    FreeSpawnPoints[index].OnSpawn(SpawnItem);
                }
            }
        }
    }

    public override void Save()
    {
        var bestScoreKey = "Minigame/Octopus/Best score";
        var lastBestScore = ES3.Load<int>(bestScoreKey, 0);
        if (AddonScore.Score > lastBestScore) ES3.Save(bestScoreKey, AddonScore.Score);
    }

    public void OnMatch()
    {
        if (decreaseHeartsOnMiss) Health.TryAdd(1);
        AddScore(10);
    }

    public void OnMiss()
    {
        if (decreaseHeartsOnMiss) Health.Damage(1);
    }

    void CheckIf_ItemsLeftOnTimeOut()
    {
        if (BusySpawnPoints.Any())
        {
            OnMiss();
            BusySpawnPoints.ForEach(item => item.OnDespawn());
        }
    }

    [Serializable]
    private class SpawnData
    {
        public float Delay => _delay; [SerializeField, SuffixLabel("sec", true)] private float _delay = 0;
        public int SpawnCount => _spawnCount; [SerializeField, Min(0), SuffixLabel("Items", true)] private int _spawnCount = 1;

        public SpawnData(){}
        public SpawnData(float delay, int spawnCount)
        {
            if (delay < 0 || spawnCount < 1) throw new ArgumentException();
            _delay = delay;
            _spawnCount = spawnCount;
        }
    }
}
