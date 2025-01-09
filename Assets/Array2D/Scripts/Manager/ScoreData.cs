public struct ScoreData
{
    public int TankBonus;      // Tank doldurma bonusu
    public int QuickBonus;     // H?zl? e?le?tirme bonusu
    public int TimeBonus;      // Süre bonusu
    public int HolderPenalty;  // Holder kullan?m cezas?
    public int Stars;          // Kazan?lan y?ld?z say?s?

    public int TotalScore => TankBonus + QuickBonus + TimeBonus + HolderPenalty;

    public override string ToString()
    {
        return $"Score: {TotalScore} (Tank: {TankBonus}, Quick: {QuickBonus}, Time: {TimeBonus}, Penalty: {HolderPenalty})";
    }
}