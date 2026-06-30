using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class VoskDialogText : MonoBehaviour
{
    public VoskSpeechToText VoskSpeechToText;

    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
    }

    private void OnTranscriptionResult(string obj)
    {
        Debug.Log(obj);  // Vosk'tan gelen yazılı metni göster
    }
}
