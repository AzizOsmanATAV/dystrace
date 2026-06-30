using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class OCRServerManager : MonoBehaviour
{
    public static OCRServerManager Instance { get; private set; }

    [SerializeField] private string serverUrl = "http://127.0.0.1:5000";
    [SerializeField] private float serverTimeout = 15f;

    private Process pythonProcess;
    private bool isServerReady = false;
    private bool isStartingServer = false;

    public System.Action OnServerReady;
    public System.Action OnServerFailed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializeServer());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator InitializeServer()
    {
        if (isStartingServer || isServerReady)
            yield break;

        isStartingServer = true;
        StartPythonServer();
        yield return StartCoroutine(WaitForServerReady());
        isStartingServer = false;
    }

    void StartPythonServer()
    {
        try
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                Debug.Log("OCR server zaten çalýþýyor.");
                return;
            }

            string exePath = Path.Combine(Application.streamingAssetsPath, "OCR", "ocr_server.exe");
            if (!File.Exists(exePath))
            {
                Debug.LogError($"OCR server exe bulunamadý: {exePath}");
                OnServerFailed?.Invoke();
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            pythonProcess = Process.Start(startInfo);

            if (pythonProcess != null)
            {
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
            }
            else
            {
                Debug.LogError("Python process null döndü.");
                OnServerFailed?.Invoke();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Python server baþlatýlamadý: {e}");
            OnServerFailed?.Invoke();
        }
    }

    IEnumerator WaitForServerReady()
    {
        float elapsed = 0f;

        while (!isServerReady && elapsed < serverTimeout)
        {
            UnityWebRequest www = UnityWebRequest.Get(serverUrl + "/health");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                isServerReady = true;
                Debug.Log("OCR server hazýr!");
                OnServerReady?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        if (!isServerReady)
        {
            Debug.LogError("OCR server baþlatýlamadý, zaman aþýmý.");
            OnServerFailed?.Invoke();
        }
    }

    public bool IsServerReady() => isServerReady;
    public bool IsStartingServer() => isStartingServer;
    public string GetServerUrl() => serverUrl;

    public void RestartServer()
    {
        StopServer();
        StartCoroutine(InitializeServer());
    }

    public void StopServer()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            pythonProcess.Dispose();
            pythonProcess = null;
            isServerReady = false;
            Debug.Log("OCR Server durduruldu.");
        }
    }

    void OnApplicationQuit()
    {
        StopServer();
    }
}
