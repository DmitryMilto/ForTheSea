using Lean.Gui;
using Lean.Transition;
using Lean.Transition.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LeanToggle))]
public class MinigameManagerAddon_Health : MinigameManagerAddon_HealthBasic
{
    [ProgressBar(0, "maxHealth", Segmented = true), ShowInInspector, PropertyOrder(-2), LabelText("Health")] public int CurrentHP
    {
        get => currentHP;
        private set
        {
            var previousValue = currentHP;
            var newValue = currentHP = value;
            var difference = Mathf.Abs(previousValue - newValue);

            if (previousValue > newValue)
            {
                // Decrease
                var activeLives = lives.Where(toggle => toggle.On).ToList();
                for (int i = 0; i < difference; i++)
                {
                    if (i >= activeLives.Count) continue;
                    activeLives[i].TurnOff();
                }
            }
            else
            {
                // Increase
                var activeLives = lives.Where(toggle => !toggle.On).ToList();
                activeLives.Reverse();
                for (int i = 0; i < difference; i++)
                {
                    if (i >= activeLives.Count) continue;
                    activeLives[i].TurnOn();
                }
            }

            if (currentHP <= 0) OnDeath.Invoke();
        }
    } private int currentHP;
    public int MaxHealth => maxHealth; [FoldoutGroup("Settings"), PropertyOrder(-1), SerializeField] private int maxHealth = 3;

    [FoldoutGroup("Components/Assets"), PropertyOrder(1), SerializeField] private GameObject HeartPrefab;
    [FoldoutGroup("Components/Assets"), PropertyOrder(1), SerializeField] private GameObject PulsingLeanAnimation;
    [FoldoutGroup("Components/Assets"), PropertyOrder(1), SerializeField] private Transform LivesContainer;
    [FoldoutGroup("Components/Assets"), PropertyOrder(1), SerializeField] private Transform PulseRootTransform;

    [FoldoutGroup("Events"), PropertyOrder(1)] public UnityEvent<int> OnDamage, OnHeal;

    public LeanToggle HealthVisibility
    {
        get => healthVisibility ??= GetComponent<LeanToggle>();
        private set => healthVisibility = value ?? throw new NullReferenceException();
    }
    private LeanToggle healthVisibility;
    private List<LeanToggle> lives = new();
    private List<Transform> pulses = new ();
    
    protected virtual void Awake()
    {
        currentHP = MaxHealth;
        InstantiateHearts();

        void InstantiateHearts()
        {
            for (int i = 0; i < maxHealth; i++)
            {
                var newHP = Instantiate(HeartPrefab, LivesContainer);
                
                if (!newHP.TryGetComponent(out LeanToggle toggle)) throw new MissingComponentException();
                lives.Add(toggle);

                var newHPPulseChain = Instantiate(PulsingLeanAnimation, pulses.LastOrDefault() is null ? PulseRootTransform : pulses.Last().transform);
                foreach (var scaler in newHPPulseChain.GetComponents<LeanTransformLocalScale_xy>()) scaler.Data.Target = newHP.transform;
                pulses.Add(newHPPulseChain.transform);

                if (i != 0) continue;
                var repeater = LivesContainer.GetComponent<LeanAnimationRepeater>();
                repeater.Transitions.Entries.Add(new LeanPlayer.Entry { Root = pulses.First() });
            }
        }
    }

    public void Damage(int count)
    {
        CurrentHP -= count;
        OnDamage.Invoke(count);
    }

    public void Heal(int count)
    {
        CurrentHP += count;
        OnHeal.Invoke(count);
    }

    public override void Kill()
    {
        var wasHP = CurrentHP;
        CurrentHP = 0;
        OnDamage.Invoke(wasHP);
    }

    public void Heal()
    {
        var wasHP = CurrentHP;
        CurrentHP = MaxHealth;
        OnHeal.Invoke(MaxHealth - wasHP);
    }
}
