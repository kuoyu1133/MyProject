using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CountryStateManager game; // 指向資源系統的主管理中心

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化信賴系統
        if (game != null && game.resource != null)
        {
            foreach (var country in game.resource.countries)
            {
                game.trust.InitializeTrusts(game.resource.countries, country.CountryName);
            }
        }
    }

    // 由 Agent 每步執行完後呼叫
    public void NextDay()
    {
        // 1. 讓 RBC 執行邏輯 (已確保 TakeTurn 內無協程)
        RuleBasedCountry[] rbcList = FindObjectsByType<RuleBasedCountry>(FindObjectsSortMode.None);
        foreach (var rbc in rbcList) rbc.TakeTurn();

        // 2. 統一更新所有國家狀態
        CountryStateManager[] allStates = FindObjectsByType<CountryStateManager>(FindObjectsSortMode.None);
        foreach (var state in allStates) state.DailyUpdate();

        // 3. ✅ 實時觀察兩國數據
        LogWorldStatus();
        Debug.Log("-------------------- 遊戲進入下一天 --------------------");
    }

    private void LogWorldStatus()
    {
        Country a = game.resource.countries.Find(c => c.CountryName == "Country A");
        Country b = game.resource.countries.Find(c => c.CountryName == "Country B");

        if (a != null && b != null)
        {
            Debug.Log($"📊 [實時統計] \n" +
                      $"【{a.CountryName}】 城市: {a.City} | 人口: {a.Population} | 民心: {a.morale.MoraleValue} |軍力: {a.MilPower}|鐵: {a.Iron} |木頭: {a.Wood} |食物: {a.Food} |" + $"【{b.CountryName}】 城市: {b.City} | 人口: {b.Population} | 民心: {b.morale.MoraleValue} | 軍力: {b.MilPower}鐵: {b.Iron} |木頭: {b.Wood} |食物: {b.Food} | ");
        }
    }
}