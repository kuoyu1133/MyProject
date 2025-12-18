using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    /*public WorldGraph worldGraph;
    public BattleSystem(WorldGraph worldGraph)
    {
        this.worldGraph = worldGraph;
    }*/
    public class BattleResult
    {
        public CountryStateManager Winner;
        public CountryStateManager Loser;
        public int AttackerLosses;
        public int DefenderLosses;
        public bool AttackerWon;
        public int Military;
    }

    public BattleResult DoBattle(CountryStateManager attacker, CountryStateManager defender)
    {
        /*if(worldGraph == null)
        {
            Debug.LogError("WorldGraph 未設置！");
            return null;
        }*/
        if (attacker == null || defender == null)
        {
            Debug.LogError($"❌ attacker 或 defender 為 null！ attacker={attacker}, defender={defender}");
            return null;
        }
        Country attackerCountry = attacker.resource.countries.Find(c => c.CountryName == attacker.CountryName);
        Country defenderCountry = defender.resource.countries.Find(c => c.CountryName == defender.CountryName);
        /*if (!worldGraph.CheckAttack(attacker, defender))//檢查能否攻擊
        {
            Debug.Log($"{attacker.CountryName} 無法攻擊 {defender.CountryName}：沒有直接或同盟通路。");
            return null;
        }*/
        
            // 士氣換算: 基礎為 1 + 民心值/100
            float attackerMorale = 1f + attacker.morale.MoraleValue / 100f;
            float defenderMorale = 1f + defender.morale.MoraleValue / 100f;

        // 戰鬥力公式: 兵力 * 士氣
        float attackerPower = attackerCountry.MilPower * attackerMorale;
        float defenderPower = defenderCountry.MilPower * defenderMorale;

        bool attackerWon = attackerPower > defenderPower;
        CountryStateManager winner = attackerWon ? attacker : defender;
        CountryStateManager loser = attackerWon ? defender : attacker;

        // 取得勝負雙方的原始數據
        Country winCountry = attackerWon ? attackerCountry : defenderCountry;
        Country loseCountry = attackerWon ? defenderCountry : attackerCountry;
        float winMorale = attackerWon ? attackerMorale : defenderMorale;
        float loseMorale = attackerWon ? defenderMorale : attackerMorale;

        // --- 🎯 新增：動態傷亡計算邏輯 ---
        // 計算戰力差距比 (例如：2.0 代表勝方強一倍)
        float powerRatio = Mathf.Max(winCountry.MilPower, 1) / Mathf.Max(loseCountry.MilPower, 1);

        // 勝方損失係數：戰力差距越大，損失越小 (最低損失 5%，最高 12.5%)
        float winLossRate = Mathf.Clamp(0.5f / powerRatio, 0.05f, 0.125f);
        // 敗方損失係數：基礎損失較重 (最低 10%，最高 17.5%)
        float loseLossRate = Mathf.Clamp(0.5f * (1f + (1f / powerRatio)), 0.1f, 0.175f);

        int winnerLosses = Mathf.CeilToInt(winCountry.MilPower * winLossRate / winMorale);
        int loserLosses = Mathf.CeilToInt(loseCountry.MilPower * loseLossRate / loseMorale);

        // 將損失分配回攻守方
        int attackerLosses = attackerWon ? winnerLosses : loserLosses;
        int defenderLosses = attackerWon ? loserLosses : winnerLosses;

        // ✅ 加入除錯訊息 (包含剛剛要求的 MilPower 追蹤)
        Debug.Log($"<color=red>[戰鬥結算]</color> {attackerCountry.CountryName} vs {defenderCountry.CountryName}\n" +
                  $"戰力比: {powerRatio:F2} | 勝方損失率: {winLossRate * 100:F1}% | 敗方損失率: {loseLossRate * 100:F1}%\n" +
                  $"攻方軍力: {attackerCountry.MilPower} -> {Mathf.Max(0, attackerCountry.MilPower - attackerLosses)} (損失: {attackerLosses})\n" +
                  $"防方軍力: {defenderCountry.MilPower} -> {Mathf.Max(0, defenderCountry.MilPower - defenderLosses)} (損失: {defenderLosses})");

        attackerCountry.MilPower = Mathf.Max(0, attackerCountry.MilPower - attackerLosses);
        defenderCountry.MilPower = Mathf.Max(0, defenderCountry.MilPower - defenderLosses);

        return new BattleResult
            {
                Winner = winner,
                Loser = loser,
                AttackerLosses = attackerLosses,
                DefenderLosses = defenderLosses,
                AttackerWon = attackerWon
            };
        
    }
}