using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VoskResultText : MonoBehaviour 
{
    public VoskSpeechToText VoskSpeechToText;
    public Text ResultText;
    public Button gitButton;
    private const string expected = "babam ekmek aldý";
    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
    }
    private void Start()
    {
        gitButton.onClick.AddListener(ChangeScene);
    }
    private void OnTranscriptionResult(string obj)
    {
        Debug.Log(obj);
        var result = new RecognitionResult(obj);

        if (result.Phrases.Length == 0) return;

        for (int i = 0; i < result.Phrases.Length; i++)
        {
            if (i > 0)
            {
                ResultText.text += ", ";
            }

            ResultText.text += result.Phrases[i].Text;
        }

        ResultText.text += "\n";

        int distance = LevenshteinDistance(expected, ResultText.text);
        if(distance <= 5)
        {
            if (CSVDataCollector.csvInstance != null)
                CSVDataCollector.csvInstance.ReceiveVoskResult(true);
        }
        gitButton.gameObject.SetActive(true);

        // Sadece son 1000 karakteri göster (overflow engeli)
        int maxLength = 1000;
        if (ResultText.text.Length > maxLength) {
            ResultText.text = ResultText.text.Substring(ResultText.text.Length - maxLength);
        }
    }
    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; i++)
            d[i, 0] = i;
        for (int j = 0; j <= m; j++)
            d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1,      // Silme
                              d[i, j - 1] + 1),     // Ekleme
                    d[i - 1, j - 1] + cost);       // Deðiþtirme
            }
        }

        return d[n, m];
    }
    void ChangeScene()
    {
        SceneManager.LoadScene(0);
    }
}
