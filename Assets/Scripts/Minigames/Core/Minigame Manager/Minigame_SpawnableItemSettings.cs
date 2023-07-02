using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawnable Minigame item", menuName = "ScriptableObjects/Minigames/00. Generic Spawnable Minigame item", order = 1)]
public class Minigame_SpawnableItemSettings : ScriptableObject
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int value;
    [SerializeField] [Range(0, 1)] private float spawnChance;
    [SerializeField] [Range(0, 1)] private float visibilityDurationMultiplier = 1;
    [SerializeField] private bool decreaseLivesOnMiss = true;

    public GameObject Prefab => prefab;
    public int Value => value;
    public float SpawnChance => spawnChance;
    public float VisibilityDurationMultiplier => visibilityDurationMultiplier;
    public bool DecreaseLivesOnMiss => decreaseLivesOnMiss;

    #region Randow Spawn
    [SerializeField, ReadOnly] public int minValue;
    [SerializeField, ReadOnly] public int maxValue;
    [SerializeField] public int changeRandom = 25;
    #endregion
}
