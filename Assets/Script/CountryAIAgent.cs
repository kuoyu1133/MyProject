using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;

public class CountryAIAgent : Agent
{
    public CountryStateManager country;
    public CountryStateManager target;

    public TradeSystem tradeSystem;
    public AnnouncementSystem announcement;
    public BattleSystem battleSystem;
    public PolicySystem policySystem;
    public OccupationSystem occupationSystem;
    public override void OnEpisodeBegin()
    {
        if (country != null && country.resource != null)
        {
            country.resource.ResetAllCountries();
        }

        RuleBasedCountry rbc = Object.FindFirstObjectByType<RuleBasedCountry>();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Country c = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        Country t = target.resource.countries.Find(x => x.CountryName == target.CountryName);

        // ✅ 解決 Fewer observations 警告：若數據尚未準備好，填充 14 個 0
        if (c == null || t == null)
        {
            for (int i = 0; i < 14; i++) sensor.AddObservation(0f);
            return;
        }

        // 觀察值 (共 14 個)
        sensor.AddObservation((float)c.Food/10000f);
        sensor.AddObservation((float)c.Iron/10000f);
        sensor.AddObservation((float)c.Wood/10000f);
        sensor.AddObservation(c.MilPower/100f);
        sensor.AddObservation((float)c.morale.MoraleValue/100f);
        sensor.AddObservation((float)c.Population/50000f);
        sensor.AddObservation((float)c.AP/50f);
        sensor.AddObservation((float)country.trust.GetTrust(target.CountryName));

        sensor.AddObservation((float)t.Food/10000f);
        sensor.AddObservation((float)t.Iron/ 10000f);
        sensor.AddObservation((float)t.Wood / 10000f);
        sensor.AddObservation(t.MilPower/100f);
        sensor.AddObservation((float)t.morale.MoraleValue/100f);
        sensor.AddObservation((float)t.AP / 50f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int act = actions.DiscreteActions[0];
        Country agentData = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        Country targetData = target.resource.countries.Find(x => x.CountryName == target.CountryName);
        if (agentData == null) return;

        // 1. 執行 AI 動作
        ExecuteAction(act, agentData,targetData);

        // 2. 驅動遊戲進入下一天 (同步 RBC 與 資源扣除)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextDay();
        }
        AddReward(0.1f);
        CheckEpisodeEnd(agentData);
    }

    private void ExecuteAction(int act, Country agentData, Country targetData)
    {
        switch (act)
        {
            case 0: // 不行動 (休息)
                    // 如果 AP 已經很多卻還不動，扣一點小分；AP 低時休息是好事
                AddReward(agentData.AP > 20 ? -0.01f : 0.005f);
                break;

            case 1: // 交易 (消耗 1 AP)
                if (agentData.AP >= 1)
                {
                    // 只有在真的缺糧、缺鐵或缺木時，交易才給大分
                    bool isNeedy = tradeSystem.GetResourceNeed(country, "Food") > 0 ||
                                   tradeSystem.GetResourceNeed(country, "Iron") > 0 ||
                                   tradeSystem.GetResourceNeed(country, "Wood") > 0;

                    AddReward(isNeedy ? 0.08f : -0.05f); // 🎯 降低盲目交易的獎勵
                    agentData.AP -= 1;
                }
                else AddReward(-0.01f);
                break;

            case 3: // 戰鬥 (3 AP)
                if (agentData.AP >= 3)
                {
                    var result = battleSystem.DoBattle(country, target);
                    agentData.AP -= 3;
                    if (result != null && result.AttackerWon) AddReward(0.3f);
                    else AddReward(-0.15f);
                }
                else AddReward(-0.01f);
                break;

            case 4: // 生育政策
                if (agentData.AP >= 1)
                {
                    int increasedPop = policySystem.ApplyPopulationPolicy(agentData);
                    Debug.Log($"📈 [政策日誌] {country.CountryName} 執行生育政策，人口變動: +{increasedPop}");
                    if (agentData.Population < 5000) AddReward(0.5f);
                    else AddReward(-0.5f);
                    agentData.AP--;
                }
                else AddReward(-0.01f);
                break;

            case 5: // 軍事政策
                if (agentData.AP >= 1)
                {
                    int popChange = policySystem.ApplyMilitaryPolicy(agentData);
                    Debug.Log($"⚔️ [政策日誌] {country.CountryName} 執行軍事政策，人口變動: {popChange}");
                    if (agentData.MilPower < targetData.MilPower) AddReward(0.7f);
                    else AddReward(0.1f);
                    agentData.AP--;
                }
                else AddReward(-0.01f);
                break;
            case 6:
                if (agentData.AP >= 2) {
                    if (targetData.morale.MoraleValue <= 30 || targetData.Population <= 3000){
                        occupationSystem.Occupy(country, target, true);
                        agentData.AP -= 2;
                        AddReward(10.0f);
                        Debug.Log($"{agentData.CountryName} 佔領了 {targetData.CountryName} 的領土");
                    }
                    else AddReward(-0.1f);
                }
                else AddReward(-0.01f);
                break;

        }
    }
    private void CheckEpisodeEnd(Country agentData)
    {
        Country targetData = target.resource.countries.Find(x => x.CountryName == target.CountryName);

        // 判定 A: 自己滅亡 (民心 <= 0 或 城市 <= 0)
        if (agentData.morale.MoraleValue <= 0 || agentData.City <= 0)
        {
            Debug.Log($"{country.CountryName} 滅亡，重置訓練。");
            SetReward(-100f);
            EndEpisode();
            return;
        }

        // 判定 B: 對手 (RBC) 滅亡 -> Agent 勝利
        if (targetData != null && (targetData.morale.MoraleValue <= 0 || targetData.City <= 0))
        {
            Debug.Log($"{target.CountryName} 滅亡，Agent 勝利！");
            SetReward(100f);
            EndEpisode();
            return;
        }
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask maskCollector)
    {
        Country agentData = country.resource.countries.Find(x => x.CountryName == country.CountryName);
        if (agentData == null) return;

        // 基礎索引說明：0:休息, 1:交易, 3:戰鬥, 4:生育, 5:軍事, 6:佔領 (根據你的 ExecuteAction)

        // 1. AP 不足的遮罩
        maskCollector.SetActionEnabled(0, 1, agentData.AP >= 1); // 交易需 1 AP
        maskCollector.SetActionEnabled(0, 3, agentData.AP >= 3); // 戰鬥需 3 AP
        maskCollector.SetActionEnabled(0, 4, agentData.AP >= 1); // 生育需 1 AP
        maskCollector.SetActionEnabled(0, 5, agentData.AP >= 1); // 軍事需 1 AP
        maskCollector.SetActionEnabled(0, 6, agentData.AP >= 2); // 佔領需 2 AP

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // 預設不行動
    }
}