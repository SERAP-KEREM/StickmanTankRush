using SerapKeremGameTools._Game._Singleton;
using UnityEngine;

public class ScoreManager : MonoSingleton<ScoreManager>
{
    [Header("Score Settings")]
    private const int TANK_COMPLETE_SCORE = 100;
    private const int QUICK_MATCH_BONUS = 50;
    private const int HOLDER_PENALTY = -30;
    private const int TIME_BONUS_MAX = 300;

    [Header("Star Thresholds")]
    private const int THREE_STAR_SCORE = 800;
    private const int TWO_STAR_SCORE = 500;
    private const int ONE_STAR_SCORE = 200;

    private float _levelStartTime;
    private int _tankBonus;
    private int _quickBonus;
    private int _timeBonus;
    private int _holderPenalty;

    private void OnEnable()
    {
        // Event'lere subscribe ol
        LevelManager.OnLevelStarted += OnLevelStarted;
    }

    private void OnDisable()
    {
        // Event'lerden unsubscribe ol
        LevelManager.OnLevelStarted -= OnLevelStarted;
    }

    private void OnLevelStarted()
    {
        ResetScores();
    }

    public void ResetScores()
    {
        Debug.Log("[ScoreManager] Resetting scores");
        _levelStartTime = Time.time;
        _tankBonus = 0;
        _quickBonus = 0;
        _timeBonus = 0;
        _holderPenalty = 0;
    }

    public void OnTankCompleted()
    {
        _tankBonus += TANK_COMPLETE_SCORE;
        Debug.Log($"[ScoreManager] Tank completed! Tank Bonus: {_tankBonus}");
    }

    public void OnQuickMatch()
    {
        _quickBonus += QUICK_MATCH_BONUS;
        Debug.Log($"[ScoreManager] Quick match! Quick Bonus: {_quickBonus}");
    }

    public void OnHolderUsed()
    {
        _holderPenalty += HOLDER_PENALTY;
        Debug.Log($"[ScoreManager] Holder used! Total Penalty: {_holderPenalty}");
    }

    private int CalculateTimeBonus()
    {
        float levelTime = Time.time - _levelStartTime;
        float timeBonus = Mathf.Max(0, TIME_BONUS_MAX - levelTime) * 2;
        Debug.Log($"[ScoreManager] Time Bonus calculated: {timeBonus}");
        return (int)timeBonus;
    }

    private int CalculateStars(int totalScore)
    {
        if (totalScore >= THREE_STAR_SCORE) return 3;
        if (totalScore >= TWO_STAR_SCORE) return 2;
        if (totalScore >= ONE_STAR_SCORE) return 1;
        return 0;
    }

    public ScoreData GetScoreData()
    {
        _timeBonus = CalculateTimeBonus();
        int totalScore = _tankBonus + _quickBonus + _timeBonus + _holderPenalty;
        int stars = CalculateStars(totalScore);

        Debug.Log($"[ScoreManager] Final Score Breakdown:");
        Debug.Log($"Tank Bonus: {_tankBonus}");
        Debug.Log($"Quick Bonus: {_quickBonus}");
        Debug.Log($"Time Bonus: {_timeBonus}");
        Debug.Log($"Holder Penalty: {_holderPenalty}");
        Debug.Log($"Total Score: {totalScore}");
        Debug.Log($"Stars Earned: {stars}");

        return new ScoreData
        {
            TankBonus = _tankBonus,
            QuickBonus = _quickBonus,
            TimeBonus = _timeBonus,
            HolderPenalty = _holderPenalty,
            Stars = stars
        };
    }
}