using Lean.Localization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <typeparam name="T">https://stackoverflow.com/a/60022011</typeparam>
public abstract class MinigameManagerAddon_ScoreBase<T> : MinigameManagerAddon_Core
{
    [SerializeField, ReadOnly, FoldoutGroup("Score")] protected T score;
    [SerializeField] protected LeanToken scoreToken;
    [SerializeField] protected LeanToken bestScoreToken;
    [FoldoutGroup("Score")] public UnityEvent<string> OnScoreChanged = new();
    public void Awake()
    {
        OnScoreChanged.AddListener(_ => LeanLocalization.UpdateTranslations());
    }
    public virtual T Score
    {
        get => score;
        set
        {
            var previousScore = score;
            score = value;

            if (IsNumber(value))
            {
                var parsedScore = float.Parse(score.ToString());
                var previousParsedScore = float.Parse(previousScore.ToString());
                OnScoreChanged.Invoke(Mathf.Abs(parsedScore - previousParsedScore).ToString());
            }
            else
            {
                OnScoreChanged.Invoke(score.ToString());
            }

            scoreToken.Value = score.ToString();

            bool IsNumber(T value) => value is double or float or int or decimal;
        }
    }

    public void LoadBestScore(string key)
    {
        if (ES3.KeyExists(key))
        {
            T candidate = ES3.Load<T>(key);
            bestScoreToken.Value = candidate is null ? default(T)?.ToString() : candidate.ToString();
        }
        else
        {
            bestScoreToken.Value = default(T)?.ToString();
        }
    }
}