using System.IO;
using UnityEngine;
using TMPro;
public class CSVWriter : MonoBehaviour
{
    string filename = "";
    public TMP_Text sonucText;
    private void Start()
    {
        filename = Application.persistentDataPath + "/timeRecorder.csv";
        sonucText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WriteCSV();
            Debug.Log("CSV kaydedildi: " + filename);
            sonucText.SetText($"Sonuçlarýnýz þuraya kaydedildi: {filename}");
            sonucText.gameObject.SetActive(true);
        }
    }

    public void WriteCSV()
    {
        var tt = TimeTracker.Instance;
        var csv = CSVDataCollector.csvInstance;
        if (tt == null)
        {
            Debug.LogWarning("TimeTracker instance bulunamadi!");
            return;
        }

        bool dosyaVar = File.Exists(filename);

        int counter;
        if (!dosyaVar)
        {
            counter = 1;
        }
        else
        {
            counter = PlayerPrefs.GetInt("CSVCounter", 0) + 1;
        }

        using (StreamWriter sw = new StreamWriter(filename, true)) // append mode
        {
            if (!dosyaVar)
                sw.WriteLine("sira;colorsSure;daysSure;ocrSure;voskSure;toplamSure;daysCorrect;colorsFalseCount;ocrCorrect;voskCorrect;eyeTrackingBigWarnings;gradeOfStudent");

            sw.WriteLine($"{counter};{tt.Sahne1Suresi:F2};{tt.Sahne2Suresi:F2};{tt.Sahne3Suresi:F2};{tt.Sahne4Suresi:F2};{tt.ElapsedTime};{csv.isDaysCorrect};{csv.colorsFalseCount};{csv.isOcrCorrect};{csv.isVoskCorrect};{csv.bigWarningCount};{csv.classOfStudent}");
        }

        PlayerPrefs.SetInt("CSVCounter", counter);
        PlayerPrefs.Save();
    }
}
