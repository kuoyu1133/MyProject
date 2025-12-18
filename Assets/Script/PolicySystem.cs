using UnityEngine;

public class PolicySystem : MonoBehaviour
{
    private const int MaxPopulation = 100000000;

    public int ApplyPopulationPolicy(Country country)
    {
        int oldPop = country.Population;
        // 改為基於城市數量的固定增長，避免複利爆炸
        int popIncrease = country.City * 200;

        long nextPop = (long)country.Population + popIncrease;
        country.Population = (nextPop > MaxPopulation || nextPop < 0) ? MaxPopulation : (int)nextPop;

        return country.Population - oldPop;
    }

    public int ApplyMilitaryPolicy(Country country)
    {
        int oldPop = country.Population;
        float oldMil = country.MilPower; // 新增：記錄舊軍力

        // 軍事政策邏輯
        float gains = country.Population * 0.001f; // 計算增量
        country.MilPower += gains;

        // ✅ 新增除錯訊息
        Debug.Log($"<color=cyan>[軍事政策]</color> {country.CountryName}: 軍力 {oldMil:F1} -> {country.MilPower:F1} (增量: +{gains:F1}, 消耗人口: {Mathf.CeilToInt(country.Population * 0.01f)})");

        country.Population = Mathf.CeilToInt(country.Population * 0.99f); // 徵兵消耗人口
        country.morale.ModifyMorale(-5);

        return country.Population - oldPop;
    }
}