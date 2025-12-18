using UnityEngine;
//國家狀態管理
//!!!!!!!!havent done yet!!!!!!!!!!
public class CountryStateManager : MonoBehaviour
{
    public string CountryName;


    public AnnouncementSystem announcement;
    public TrustSystem trust;
    public MoraleSystem morale;
    public PolicySystem policy;
    public ResourceSystem resource;
    public OccupationSystem occupation;
    public MoraleSystem moraleSystem;

    public int Population;
    public float MilPower;
    public int Defense;


    public int Iron;
    public int Food;
    public int Wood;
    public float PopulationGrowthRate;//人口成長率

    public void DailyUpdate()
    {

        Country selfData = resource.countries.Find(c => c.CountryName == CountryName);

        if (selfData == null) return;

        resource.UpdateDay(selfData);

        if (selfData.Iron < selfData.Population * 0.02f)
            selfData.morale.ModifyMorale(-1);
        else
            selfData.morale.ModifyMorale(2);

        if (selfData.Wood < selfData.Population * 0.03f)
            selfData.morale.ModifyMorale(-2);
        else
            selfData.morale.ModifyMorale(4);

        if (selfData.Food < selfData.Population * 0.05f)
            selfData.morale.ModifyMorale(-3);
        else
            selfData.morale.ModifyMorale(5);

        if (selfData.morale.CheckDefeated(selfData))
        {
            Debug.Log($"{CountryName} 已滅亡（因民心歸零）！");
        }
        if (selfData.City == 0)
        {
            Debug.Log($"{CountryName} 已滅亡（因城市均被佔領）！");
        }
    }
    public CountryStateManager GetCountryByName(string name)
    {
        CountryStateManager[] allManagers = FindObjectsByType<CountryStateManager>(FindObjectsSortMode.None);
        foreach (var manager in allManagers)
        {
            if (manager.CountryName == name)
            {
                return manager;
            }
        }
        return null;
    }

}