using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    [Header("時間設定")]
    public float timeScale = 1f; // ゲーム内時間のスケール
    public float dayDuration = 120f; // 1日の長さ（秒）
    
    [Header("現在時刻")]
    public int currentDay = 1;
    public int currentHour = 6; // 6時スタート
    public int currentMinute = 0;
    
    [Header("季節システム")]
    public Season currentSeason = Season.Spring;
    public int daysPerSeason = 30;
    
    float gameTimeElapsed;
    float lastMinuteUpdate;
    
    public event Action<int> OnHourChanged;
    public event Action<int> OnDayChanged; 
    public event Action<Season> OnSeasonChanged;
    
    public enum Season
    {
        Spring, // 春
        Summer, // 夏  
        Autumn, // 秋
        Winter  // 冬
    }
    
    public static TimeSystem Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        UpdateTime();
    }
    
    void UpdateTime()
    {
        gameTimeElapsed += Time.deltaTime * timeScale;
        
        // 分が経過した場合
        float minutesElapsed = gameTimeElapsed / (dayDuration / (24 * 60));
        if (minutesElapsed - lastMinuteUpdate >= 1f)
        {
            AdvanceMinute();
            lastMinuteUpdate = Mathf.Floor(minutesElapsed);
        }
    }
    
    void AdvanceMinute()
    {
        currentMinute++;
        
        if (currentMinute >= 60)
        {
            currentMinute = 0;
            AdvanceHour();
        }
    }
    
    void AdvanceHour()
    {
        int prevHour = currentHour;
        currentHour++;
        
        if (currentHour >= 24)
        {
            currentHour = 0;
            AdvanceDay();
        }
        
        OnHourChanged?.Invoke(currentHour);
    }
    
    void AdvanceDay()
    {
        int prevDay = currentDay;
        currentDay++;
        
        // 季節変更チェック
        if ((currentDay - 1) % daysPerSeason == 0 && currentDay > 1)
        {
            AdvanceSeason();
        }
        
        OnDayChanged?.Invoke(currentDay);
    }
    
    void AdvanceSeason()
    {
        Season prevSeason = currentSeason;
        
        currentSeason = currentSeason switch
        {
            Season.Spring => Season.Summer,
            Season.Summer => Season.Autumn,
            Season.Autumn => Season.Winter,
            Season.Winter => Season.Spring,
            _ => Season.Spring
        };
        
        OnSeasonChanged?.Invoke(currentSeason);
    }
    
    public string GetTimeString()
    {
        return $"Day {currentDay}, {currentHour:00}:{currentMinute:00}";
    }
    
    public string GetSeasonString()
    {
        return currentSeason switch
        {
            Season.Spring => "春",
            Season.Summer => "夏",
            Season.Autumn => "秋", 
            Season.Winter => "冬",
            _ => "不明"
        };
    }
    
    public bool IsNight()
    {
        return currentHour >= 20 || currentHour < 6;
    }
    
    public bool IsMorning()
    {
        return currentHour >= 6 && currentHour < 12;
    }
    
    public bool IsAfternoon()
    {
        return currentHour >= 12 && currentHour < 18;
    }
    
    public bool IsEvening()
    {
        return currentHour >= 18 && currentHour < 20;
    }
}

// デイリータスクシステム
[System.Serializable]
public class DailyTask
{
    public string name;
    public string description;
    public bool isCompleted;
    public TaskType type;
    public int targetAmount;
    public int currentAmount;
    public ResourceType targetResource;
    
    [Header("報酬")]
    public List<ResourceStack> rewards = new List<ResourceStack>();
    public int experienceReward = 50;
    
    public enum TaskType
    {
        CollectResource,  // 資源収集
        BuildStructure,   // 建設
        TalkToNPC,       // NPCとの会話
        CompleteProject   // プロジェクト完成
    }
    
    public bool IsComplete => currentAmount >= targetAmount;
    public float Progress => (float)currentAmount / targetAmount;
}
