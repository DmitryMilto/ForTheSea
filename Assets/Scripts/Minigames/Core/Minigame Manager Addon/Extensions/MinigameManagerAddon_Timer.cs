using System;

public class MinigameManagerAddon_Timer : MinigameManagerAddon_ScoreBase<TimeSpan>
{
    public override TimeSpan Score
    {
        get => score;
        set
        {
            var previousScore = score;
            score = value;
            var formatString = @"m\:ss";
            OnScoreChanged.Invoke(score.ToString(formatString));
            scoreToken.Value = score.ToString(formatString);
        }
    }
}