using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeTracker : MonoBehaviour
{
    public static TimeTracker Instance { get; private set; }

    public float ElapsedTime; // Toplam süre

    private Dictionary<string, float> sceneTimes = new();

    private string currentScene;
    private bool isRunning;
    private GameManager gameManager;

    [Header("Sahne Süreleri")]
    [SerializeField] private float sahne1Suresi;
    [SerializeField] private float sahne2Suresi;
    [SerializeField] private float sahne3Suresi;
    [SerializeField] private float sahne4Suresi;

    public float Sahne1Suresi => sahne1Suresi;
    public float Sahne2Suresi => sahne2Suresi;
    public float Sahne3Suresi => sahne3Suresi;
    public float Sahne4Suresi => sahne4Suresi;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        StartTimer();
        SceneManager.sceneLoaded += OnSceneLoaded;

        currentScene = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        if (!isRunning) return;

        float delta = Time.deltaTime;
        ElapsedTime += delta;

        if (!string.IsNullOrEmpty(currentScene))
        {
            if (sceneTimes.ContainsKey(currentScene))
                sceneTimes[currentScene] += delta;
            else
                sceneTimes[currentScene] = delta;

            UpdateSceneTimerVariable(currentScene, sceneTimes[currentScene]);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PrintTimes();

        currentScene = scene.name;

        Debug.Log($"Yeni sahne yüklendi: {currentScene}");
        Debug.Log($"Skor: {gameManager?.score ?? -1}");
    }

    public void StartTimer() => isRunning = true;
    public void StopTimer() => isRunning = false;

    public void ResetTimer()
    {
        ElapsedTime = 0f;
        sceneTimes.Clear();

        sahne1Suresi = 0f;
        sahne2Suresi = 0f;
        sahne3Suresi = 0f;
        sahne4Suresi = 0f;
    }

    public void PrintTimes()
    {
        Debug.Log("---- Sahnelerde Geçirilen Süreler ----");
        foreach (var kvp in sceneTimes)
            Debug.Log($"Sahne: {kvp.Key}, Süre: {kvp.Value:F2} saniye");

        if (gameManager != null && gameManager.completedEventIDs.Count == 4)
            Debug.Log($"Toplam Süre: {ElapsedTime:F2} saniye");
    }

    private void UpdateSceneTimerVariable(string sceneName, float value)
    {
        switch (sceneName)
        {
            case "ColorsShapes":
                sahne1Suresi = value;
                break;
            case "Days":
                sahne2Suresi = value;
                break;
            case "OCR":
                sahne3Suresi = value;
                break;
            case "Vosk":
                sahne4Suresi = value;
                break;
        }
    }

    // İşte istediğin metod, sahne sürelerini dışarıya veriyor
    public Dictionary<string, float> GetSceneTimes()
    {
        return new Dictionary<string, float>(sceneTimes);
    }
}
