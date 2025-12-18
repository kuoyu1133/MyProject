using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Country
{
    public MoraleSystem morale;
    public string CountryName;
    public int AP; // 行動點
    public int Population;
    public int Iron;
    public int Food;
    public int Wood;
    public float MilPower;
    public int Defense;
    public int City;
    public float PopulationGrowthRate;

    public int DailyFoodProd;
    public int DailyIronProd;
    public int DailyWoodProd;

    public Country(string name, int ap, int population, int iron, int food, int wood, float milPower, int defense, float growthRate, int foodProd, int ironProd, int woodProd, int city)
    {
        CountryName = name;
        Population = population;
        Iron = iron;
        Food = food;
        Wood = wood;
        MilPower = milPower;
        Defense = defense;
        PopulationGrowthRate = growthRate;
        DailyFoodProd = foodProd;
        DailyIronProd = ironProd;
        DailyWoodProd = woodProd;
        City = city;
        AP = ap;
        morale = new MoraleSystem();
    }
}
public class ResourceSystem : MonoBehaviour
{
    public List<Country> countries;
    private const int MaxPopulation = 100000000;
    private List<Country> initialDataBackup = new List<Country>();
    void Awake()
    {
        countries = new List<Country>()
        {
            /*new Country("鐵之國", 5, 0, 8000, 1500, 800, 1000, 130, 100, 0.1f, 800, 450, 300, 3),
            new Country("糧之國", 5, 0, 10000, 400, 1800, 1000, 100, 100, 0.3f, 1300, 100, 200, 4),
            new Country("木材國", 5, 0, 8500, 700, 900, 1600, 100, 130, 0.15f, 900, 200, 450, 3),
            new Country("貿易國", 5, 0, 7500, 500, 600, 600, 100, 100, 0.1f, 700, 180, 200, 2),
            new Country("奢侈品國", 5, 0, 9000, 800, 1000, 900, 100, 100, 0.1f, 900, 220, 300, 3),
            new Country("科技國", 5, 0, 8500, 900, 900, 900, 110, 110, 0.1f, 950, 250, 300, 3)*/
            new Country("Country A", 5, 10000, 5000, 5000, 5000, 110, 100, 0.001f, 500, 200, 300, 3),
            new Country("Country B", 5, 10000, 5000, 5000, 5000, 110, 100, 0.001f, 500, 200, 300, 3)
        };
        foreach (var c in countries)
        {
            initialDataBackup.Add(new Country(c.CountryName, c.AP, c.Population, c.Iron, c.Food, c.Wood, c.MilPower, c.Defense, c.PopulationGrowthRate, c.DailyFoodProd, c.DailyIronProd, c.DailyWoodProd, c.City));
        }
    }
    public void ResetAllCountries()
    {
        for (int i = 0; i < countries.Count; i++)
        {
            var backup = initialDataBackup[i];
            var current = countries[i];

            current.Population = backup.Population;
            current.Iron = backup.Iron;
            current.Food = backup.Food;
            current.Wood = backup.Wood;
            current.MilPower = backup.MilPower;
            current.City = backup.City;
            current.AP = backup.AP;
            current.morale.SetMorale(50); // 將民心重置為 50
            current.AP = 5;
        }
        Debug.Log("♻️ 所有國家資料已還原至初始狀態！");
    }
    public void UpdateDay(Country country)
    {
        // 資源消耗
        country.Food -= Mathf.CeilToInt(country.Population * 0.05f);
        country.Iron -= Mathf.CeilToInt(country.Population * 0.02f);
        country.Wood -= Mathf.CeilToInt(country.Population * 0.03f);

        // 資源產量
        country.Food += country.DailyFoodProd;
        country.Iron += country.DailyIronProd;
        country.Wood += country.DailyWoodProd;

        // ✅ 人口自然增長：限制增長率並加入溢位保護
        float safeRate = Mathf.Min(country.PopulationGrowthRate / 100f, 0.01f); // 每日自然成長上限 1%
        long growth = (long)Mathf.CeilToInt(country.Population * safeRate);
        long nextPop = (long)country.Population + growth;

        country.Population = (nextPop > MaxPopulation || nextPop < 0) ? MaxPopulation : (int)nextPop;
        int dailyAPRecovery = 5;
        int maxAPLimit = 50; // 建議設定一個上限（如 50），防止長期不行動導致數值過大

        country.AP = Mathf.Min(country.AP + dailyAPRecovery, maxAPLimit);

        Debug.Log($"{country.CountryName} 每日恢復：AP +5，目前總計：{country.AP}");
    }
}