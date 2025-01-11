using UnityEngine;

public struct ScoreData
{
    [Header("Bonuses")]
    [Tooltip("Bonus for filling the tank")]
    public int TankFillBonus;  // Tank fill bonus

    [Tooltip("Bonus for quick matching")]
    public int QuickMatchBonus; // Quick match bonus

    [Tooltip("Bonus based on time")]
    public int TimeBonus;      // Time bonus

    [Header("Penalties")]
    [Tooltip("Penalty for using the holder")]
    public int HolderPenalty;  // Holder usage penalty

    [Header("Stars")]
    [Tooltip("Number of stars earned")]
    public int Stars;          // Number of stars earned

    /// <summary>
    /// Calculates the total score, taking penalties as negative values.
    /// </summary>
    public int TotalScore => TankFillBonus + QuickMatchBonus + TimeBonus - HolderPenalty;
}
