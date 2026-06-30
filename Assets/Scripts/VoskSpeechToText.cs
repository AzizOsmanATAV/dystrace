using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vosk;

public class VoskSpeechToText : MonoBehaviour
{
    public string ModelPath = "vosk-model-small-tr-0.3.zip";
    public VoiceProcessor VoiceProcessor;
    public int MaxAlternatives = 1;
    public float MaxRecordLength = 5;
    public bool AutoStart = false;
    public List<string> KeyPhrases = new List<string>();
    public List<string> ExpectedPhrases = new List<string>();
    public Button startButton;

    private Model _model;
    private VoskRecognizer _recognizer;
    private bool _recognizerReady;
    private string _decompressedModelPath;
    private string _grammar = "";
    private bool _isDecompressing;
    private bool _isInitializing;
    private bool _didInit;
    private bool _running;
    private bool _isRecording = false;

    private readonly ConcurrentQueue<short[]> _threadedBufferQueue = new ConcurrentQueue<short[]>();
    private readonly ConcurrentQueue<string> _threadedResultQueue = new ConcurrentQueue<string>();
    private Coroutine _recordingTimeoutCoroutine;

    public Action<string> OnStatusUpdated;
    public Action<string> OnTranscriptionResult;
    public Action<string> OnValidRecognition;
    public Action<string> OnInvalidRecognition;

    void Start()
    {
        if (AutoStart) StartVoskStt();
        startButton?.onClick.AddListener(StartSingleRecognition);
    }

    public void StartVoskStt(List<string> keyPhrases = null, string modelPath = null, int maxAlternatives = 1)
    {
        if (_isInitializing || _didInit) return;

        if (!string.IsNullOrEmpty(modelPath)) ModelPath = modelPath;
        if (keyPhrases != null) KeyPhrases = keyPhrases;

        MaxAlternatives = maxAlternatives;
        StartCoroutine(DoStartVoskStt());
    }

    private IEnumerator DoStartVoskStt()
    {
        _isInitializing = true;

        Debug.Log($"StreamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"PersistentDataPath: {Application.persistentDataPath}");

        yield return WaitForMicrophoneInput();
        yield return Decompress();

        _model = new Model(_decompressedModelPath);

        VoiceProcessor.OnFrameCaptured += VoiceProcessorOnFrameCaptured;
        VoiceProcessor.OnRecordingStop += VoiceProcessorOnRecordingStop;

        _isInitializing = false;
        _didInit = true;
        OnStatusUpdated?.Invoke("Vosk hazýr.");
    }

    private IEnumerator Decompress()
    {
        string zipName = Path.GetFileNameWithoutExtension(ModelPath);
        _decompressedModelPath = Path.Combine(Application.persistentDataPath, zipName);

        if (Directory.Exists(_decompressedModelPath))
        {
            Debug.Log("Model zaten açýlmýþ.");
            yield break;
        }

        Debug.Log("Zip modeli açýlýyor...");

        string zipPath = Path.Combine(Application.streamingAssetsPath, ModelPath);
        Stream zipStream;

        if (zipPath.Contains("://"))
        {
            UnityWebRequest www = UnityWebRequest.Get(zipPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Model dosyasý indirilemedi: " + www.error);
                yield break;
            }

            zipStream = new MemoryStream(www.downloadHandler.data);
        }
        else
        {
            zipStream = File.OpenRead(zipPath);
        }

        var zipFile = ZipFile.Read(zipStream);
        zipFile.ExtractProgress += (s, e) =>
        {
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
                _isDecompressing = true;
        };

        zipFile.ExtractAll(Application.persistentDataPath);
        while (!_isDecompressing) yield return null;

        Debug.Log("Zip açma tamamlandý.");
    }

    private IEnumerator WaitForMicrophoneInput()
    {
        while (Microphone.devices.Length == 0) yield return null;
    }

    public void StartSingleRecognition()
    {
        if (!_didInit || _isRecording) return;

        _isRecording = true;
        _running = true;

        _threadedBufferQueue.Clear();
        _threadedResultQueue.Clear();

        VoiceProcessor.StartRecording();
        Task.Run(ThreadedWork);

        _recordingTimeoutCoroutine = StartCoroutine(RecordingTimeout());
        OnStatusUpdated?.Invoke("Dinleniyor...");
    }

    public void StopRecognition()
    {
        if (!_isRecording) return;

        _running = false;
        _isRecording = false;
        VoiceProcessor.StopRecording();

        if (_recordingTimeoutCoroutine != null)
            StopCoroutine(_recordingTimeoutCoroutine);

        OnStatusUpdated?.Invoke("Durdu");
    }

    private IEnumerator RecordingTimeout()
    {
        yield return new WaitForSeconds(MaxRecordLength);
        if (_isRecording) StopRecognition();
    }

    private void UpdateGrammar()
    {
        if (KeyPhrases.Count == 0)
        {
            _grammar = "";
            return;
        }

        JSONArray keywords = new JSONArray();
        foreach (string phrase in KeyPhrases) keywords.Add(new JSONString(phrase.ToLower()));
        keywords.Add(new JSONString("[unk]"));
        _grammar = keywords.ToString();
    }

    private async Task ThreadedWork()
    {
        if (!_recognizerReady)
        {
            UpdateGrammar();
            _recognizer = string.IsNullOrEmpty(_grammar)
                ? new VoskRecognizer(_model, 16000f)
                : new VoskRecognizer(_model, 16000f, _grammar);

            _recognizer.SetMaxAlternatives(MaxAlternatives);
            _recognizerReady = true;
        }

        while (_running)
        {
            if (_threadedBufferQueue.TryDequeue(out short[] buffer))
            {
                if (_recognizer.AcceptWaveform(buffer, buffer.Length))
                {
                    _threadedResultQueue.Enqueue(_recognizer.Result());
                    break;
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    void Update()
    {
        if (_threadedResultQueue.TryDequeue(out string result)) ProcessRecognitionResult(result);
    }

    private void ProcessRecognitionResult(string result)
    {
        OnTranscriptionResult?.Invoke(result);

        if (IsValidRecognition(result))
        {
            OnValidRecognition?.Invoke(result);
            OnStatusUpdated?.Invoke("Doðru: " + result);
        }
        else
        {
            OnInvalidRecognition?.Invoke(result);
            OnStatusUpdated?.Invoke("Hatalý: " + result);
        }

        StopRecognition();
    }

    private bool IsValidRecognition(string result)
    {
        if (ExpectedPhrases.Count == 0) return true;
        string res = result.ToLower();
        return ExpectedPhrases.Exists(p => res.Contains(p.ToLower()));
    }

    private void VoiceProcessorOnFrameCaptured(short[] samples)
    {
        if (_isRecording) _threadedBufferQueue.Enqueue(samples);
    }

    private void VoiceProcessorOnRecordingStop()
    {
        Debug.Log("VoiceProcessor durdu.");
    }

    void OnDestroy()
    {
        _running = false;
        _isRecording = false;
        if (_recordingTimeoutCoroutine != null) StopCoroutine(_recordingTimeoutCoroutine);
    }
}
