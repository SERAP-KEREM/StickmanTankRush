using SerapKeremGameTools._Game._Singleton;
using UnityEngine;

public class ScoreManager : MonoSingleton<ScoreManager>
{
    #region Constants

    [Header("Score Settings")]
    [Tooltip("Score for completing a tank.")]
    private const int TANK_COMPLETE_SCORE = 100;

    [Tooltip("Bonus for quick matches.")]
    private const int QUICK_MATCH_BONUS = 50;

    [Tooltip("Penalty for using a holder.")]
    private const int HOLDER_PENALTY = -30;

    [Tooltip("Maximum time for time-based bonus.")]
    private const int TIME_BONUS_MAX = 300;

    [Header("Star Thresholds")]
    [Tooltip("Score threshold for 3 stars.")]
    private const int THREE_STAR_SCORE = 800;

    [Tooltip("Score threshold for 2 stars.")]
    private const int TWO_STAR_SCORE = 500;

    [Tooltip("Score threshold for 1 star.")]
    private const int ONE_STAR_SCORE = 200;

    #endregion

    #region Private Fields

    private float _levelStartTime;
    private int _tankBonus;
    private int _quickBonus;
    private int _timeBonus;
    private int _holderPenalty;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        // Subscribe to level start event
        LevelManager.OnLevelStarted += OnLevelStarted;
    }

    private void OnDisable()
    {
        // Unsubscribe from level start event
        LevelManager.OnLevelStarted -= OnLevelStarted;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Resets scores when a new level is started.
    /// </summary>
    private void OnLevelStarted()
    {
        ResetScores();
    }

    #endregion

    #region Score Calculation

    /// <summary>
    /// Resets all score values to their default.
    /// </summary>
    public void ResetScores()
    {
        Debug.Log("[ScoreManager] Resetting scores");
        _levelStartTime = Time.time;
        _tankBonus = 0;
        _quickBonus = 0;
        _timeBonus = 0;
        _holderPenalty = 0;
    }

    /// <summary>
    /// Adds bonus for completing a tank.
    /// </summary>
    public void OnTankCompleted()
    {
        _tankBonus += TANK_COMPLETE_SCORE;
        Debug.Log($"[ScoreManager] Tank completed! Tank Bonus: {_tankBonus}");
    }

    /// <summary>
    /// Adds bonus for a quick match.
    /// </summary>
    public void OnQuickMatch()
    {
        _quickBonus += QUICK_MATCH_BONUS;
        Debug.Log($"[ScoreManager] Quick match! Quick Bonus: {_quickBonus}");
    }

    /// <summary>
    /// Adds penalty for using a holder.
    /// </summary>
    public void OnHolderUsed()
    {
        _holderPenalty += HOLDER_PENALTY;
        Debug.Log($"[ScoreManager] Holder used! Total Penalty: {_holderPenalty}");
    }

    /// <summary>
    /// Calculates the time-based bonus based on the level duration.
    /// </summary>
    /// <returns>The calculated time bonus.</returns>
    private int CalculateTimeBonus()
    {
        float levelTime = Time.time - _levelStartTime;
        float timeBonus = Mathf.Max(0, TIME_BONUS_MAX - levelTime) * 2;
        Debug.Log($"[ScoreManager] Time Bonus calculated: {timeBonus}");
        return (int)timeBonus;
    }

    /// <summary>
    /// Calculates the number of stars based on the total score.
    /// </summary>
    /// <param name="totalScore">The total score to evaluate.</param>
    /// <returns>The number of stars earned.</returns>
    private int CalculateStars(int totalScore)
    {
        if (totalScore >= THREE_STAR_SCORE) return 3;
        if (totalScore >= TWO_STAR_SCORE) return 2;
        if (totalScore >= ONE_STAR_SCORE) return 1;
        return 0;
    }

    #endregion

    #region Score Retrieval

    /// <summary>
    /// Returns the final score data with calculated bonuses, penalties, and stars.
    /// </summary>
    /// <returns>The score data for the level.</returns>
    public ScoreData GetScoreData()
    {
        // Calculate time-based bonus
        _timeBonus = CalculateTimeBonus();

        // Calculate total score
        int totalScore = _tankBonus + _quickBonus + _timeBonus + _holderPenalty;

        // Calculate stars earned based on total score
        int stars = CalculateStars(totalScore);

        // Log final score breakdown
        Debug.Log($"[ScoreManager] Final Score Breakdown:");
        Debug.Log($"Tank Bonus: {_tankBonus}");
        Debug.Log($"Quick Bonus: {_quickBonus}");
        Debug.Log($"Time Bonus: {_timeBonus}");
        Debug.Log($"Holder Penalty: {_holderPenalty}");
        Debug.Log($"Total Score: {totalScore}");
        Debug.Log($"Stars Earned: {stars}");

        return new ScoreData
        {
            TankFillBonus = _tankBonus,
            QuickMatchBonus = _quickBonus,
            TimeBonus = _timeBonus,
            HolderPenalty = _holderPenalty,
            Stars = stars
        };
    }

    #endregion
}
