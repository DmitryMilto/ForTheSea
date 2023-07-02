using Lean.Pool;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameManager_Crab : MinigameManager_Core
{
    enum SpawnSides { Left, Right, Any }

    [SerializeField, FoldoutGroup("Settings")] private SpawnSides spawnSide = SpawnSides.Right;
    [SerializeField, FoldoutGroup("Settings")] private float maxSpawnCount = 3;
    [SerializeField, FoldoutGroup("Settings")] private float maxSpawnDelay = 2;
    [SerializeField, FoldoutGroup("Settings")] private Vector2 speedMinMax;
    [SerializeField, FoldoutGroup("Settings")] private List<Minigame_SpawnableItemSettings> ItemVariations = new();

    [SerializeField, FoldoutGroup("Components")] private Controller_SwipeCutter cutController;
    [SerializeField, FoldoutGroup("Components")] private RectTransform spawnArea;
    [SerializeField, FoldoutGroup("Components")] internal CanvasScaler Canvas;

    public Dictionary<CrabMinigame_ItemLinker, GameObject> instantiatedItems = new();

    public override void Awake()
    {
        base.Awake();
        AddonScore.LoadBestScore("Minigame/Crab/Best score");
    }

    protected override IEnumerator GameRoutine()
    {
        CheckDecreaseHeartsOnMiss();
        if (isAllowedToRun) yield return new WaitForSeconds(startDelay);
        yield return new WaitUntil(() => isAllowedToRun);

        while (enabled)
        {
            yield return new WaitWhile(() => IsPaused || !isAllowedToRun);

            CheckDecreaseHeartsOnMiss();

            var count = Mathf.RoundToInt(Mathf.Clamp(Random.value * maxSpawnCount, 1, maxSpawnCount));

            SpawnItems(count);

            currentDifficulty += difficultyIncreaseOverTime;
            yield return new WaitForSeconds(maxSpawnDelay);
        }

        void SpawnItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Randomize position and speed for spawn iteration
                var randomHeight = spawnArea.rect.height * Random.value;
                var randomSide = spawnSide switch
                {
                    SpawnSides.Left => Vector2.left,
                    SpawnSides.Right => Vector2.right,
                    SpawnSides.Any => Random.value >= 0.5f ? Vector2.right : Vector2.left,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var speed = Random.Range(speedMinMax.x * currentDifficulty, speedMinMax.y * currentDifficulty);

                // Raw (doesn't involve prefab size as offset) position on the edge of spawn area rectangle
                Vector2 spawnPosition = Vector2.Scale(new Vector2(randomSide == Vector2.left ? spawnArea.rect.xMin : spawnArea.rect.xMax, spawnArea.rect.yMin + randomHeight), Canvas.transform.localScale);
                Vector2 oppositeSpawnPosition = Vector2.Scale(new Vector2(randomSide == Vector2.left ? spawnArea.rect.xMax : spawnArea.rect.xMin, spawnArea.rect.yMin + randomHeight), Canvas.transform.localScale);

                // Pick Item to spawn
                var itemToSpawn = GetRandomItem(ItemVariations);
                // Spawn Item
                var itemInstance = LeanPool.Spawn(itemToSpawn.Prefab, spawnArea);
                if (!itemInstance.TryGetComponent<CrabMinigame_ItemLinker>(out var itemInstanceLinker)) throw new MissingComponentException();

                var itemWidth = Vector2.Scale(new Vector2(((RectTransform)itemToSpawn.Prefab.transform).rect.width, 0), Canvas.transform.localScale);

                // Move Item and adjust rotation
                itemInstance.transform.position = randomSide == Vector2.left ? spawnPosition : spawnPosition + itemWidth;
                itemInstanceLinker.RotatableVisual.localScale = randomSide == Vector2.left ? Vector3.one : new Vector3(-1, 1, 1);

                // Calculate and adjust end position to involve prefab size as offset
                var destination = randomSide == Vector2.left ? oppositeSpawnPosition + itemWidth : oppositeSpawnPosition;
                var distance = Vector2.Distance(spawnPosition, destination);
                var duration = distance / speed;

                // Set despawn timer, when destination should be reached, based on computed duration
                itemInstanceLinker.OnDespawn.AddListener(() => LeanPool.Despawn(itemInstance, 0.01f));

                itemInstanceLinker.Init(duration, destination, itemToSpawn);
            }
        }
    }

    protected internal void IncreaseScore(int value) => AddonScore.Score += value;

    public override void Save()
    {
        var bestScoreKey = "Minigame/Crab/Best score";
        var lastBestScore = ES3.Load<int>(bestScoreKey, 0);
        if (AddonScore.Score > lastBestScore) ES3.Save(bestScoreKey, AddonScore.Score);
    }

    internal void DamageHP()
    {
        if (!decreaseHeartsOnMiss) return;
        if (base.AddonHealth is MinigameManagerAddon_Health AddonHealth)
            AddonHealth.Damage(1);
    }

    internal void AddHPProgress()
    {
        if (base.AddonHealth is MinigameManagerAddon_Health_Restorable AddonHealth)
            AddonHealth.TryAdd(1);
    }
}