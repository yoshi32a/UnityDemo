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

public class DailyTaskManager : MonoBehaviour
{
    [Header("タスク")]
    public List<DailyTask> availableTasks = new List<DailyTask>();
    public List<DailyTask> currentTasks = new List<DailyTask>();
    public int maxDailyTasks = 3;
    
    [Header("参照")]
    public ResourceInventory resourceInventory;
    
    [Header("UI")]
    public bool showTaskUI = true;
    Vector2 taskScrollPos;
    
    void Start()
    {
        if (!resourceInventory)
            resourceInventory = FindFirstObjectByType<ResourceInventory>();
        
        // 時間システムの日付変更イベントに登録
        if (TimeSystem.Instance)
        {
            TimeSystem.Instance.OnDayChanged += OnNewDay;
        }
        
        // 初回タスク生成
        GenerateNewTasks();
    }
    
    void OnNewDay(int newDay)
    {
        GenerateNewTasks();
    }
    
    void GenerateNewTasks()
    {
        // 前日のタスクをクリア
        currentTasks.Clear();
        
        // 新しいタスクをランダムに選択
        var shuffledTasks = new List<DailyTask>(availableTasks);
        
        for (int i = 0; i < Mathf.Min(maxDailyTasks, shuffledTasks.Count); i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledTasks.Count);
            var temp = shuffledTasks[i];
            shuffledTasks[i] = shuffledTasks[randomIndex];
            shuffledTasks[randomIndex] = temp;
            
            // タスクのコピーを作成してリセット
            var taskCopy = JsonUtility.FromJson<DailyTask>(JsonUtility.ToJson(shuffledTasks[i]));
            taskCopy.currentAmount = 0;
            taskCopy.isCompleted = false;
            
            currentTasks.Add(taskCopy);
        }
        
        Debug.Log($"新しい日のタスクを生成しました（Day {TimeSystem.Instance?.currentDay}）");
    }
    
    public void UpdateTaskProgress(DailyTask.TaskType taskType, ResourceType resource = null, int amount = 1)
    {
        foreach (var task in currentTasks)
        {
            if (task.isCompleted) continue;
            if (task.type != taskType) continue;
            
            bool shouldUpdate = false;
            
            switch (taskType)
            {
                case DailyTask.TaskType.CollectResource:
                    shouldUpdate = (task.targetResource == resource);
                    break;
                default:
                    shouldUpdate = true;
                    break;
            }
            
            if (shouldUpdate)
            {
                task.currentAmount += amount;
                
                if (task.IsComplete && !task.isCompleted)
                {
                    CompleteTask(task);
                }
            }
        }
    }
    
    void CompleteTask(DailyTask task)
    {
        task.isCompleted = true;
        
        // 報酬を付与
        foreach (var reward in task.rewards)
        {
            resourceInventory?.AddResource(reward.resourceType, reward.amount);
        }
        
        Debug.Log($"タスク完了: {task.name} - 報酬を受け取りました！");
    }
    
    void OnGUI()
    {
        if (!showTaskUI || !TimeSystem.Instance) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 250, 300, 240));
        
        GUILayout.Label("=== デイリータスク ===", GUI.skin.box);
        GUILayout.Label($"{TimeSystem.Instance.GetTimeString()} ({TimeSystem.Instance.GetSeasonString()})");
        
        taskScrollPos = GUILayout.BeginScrollView(taskScrollPos, GUILayout.Height(180));
        
        foreach (var task in currentTasks)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            string statusIcon = task.isCompleted ? "✓" : "○";
            GUILayout.Label($"{statusIcon} {task.name}");
            GUILayout.Label(task.description);
            GUILayout.Label($"進捗: {task.currentAmount}/{task.targetAmount} ({task.Progress:P1})");
            
            GUILayout.EndVertical();
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
