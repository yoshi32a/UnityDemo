using System.Collections.Generic;
using UnityEngine;

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