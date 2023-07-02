using UnityEngine;

public class MinigameManagerAddon_Score : MinigameManagerAddon_ScoreBase<int>
{
    public override int Score
    {
        get => score;
        set
        {
            var previousScore = score;
            score = value;
            OnScoreChanged.Invoke(Mathf.Abs(score - previousScore).ToString());
            scoreToken.Value = score.ToString();
        }
    }
}