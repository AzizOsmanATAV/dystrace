using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using SFB;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class OcrClient : MonoBehaviour
{
    public Button selectImageButton;
    public Image selectedImage;
    public TMP_Text ocrResultText;
    public Button changeScene;

    private string selectedImagePath;

    void Start()
    {
        changeScene.gameObject.SetActive(false);
        changeScene.onClick.AddListener(ChangeScene);
        selectImageButton.onClick.AddListener(OpenFilePicker);
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        selectImageButton.interactable = false;
        ocrResultText.text = "Server kontrol ediliyor...";

        float elapsed = 0f;
        float maxWait = 15f;

        while ((OCRServerManager.Instance == null || !OCRServerManager.Instance.IsServerReady()) && elapsed < maxWait)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        if (OCRServerManager.Instance == null || !OCRServerManager.Instance.IsServerReady())
        {
            ocrResultText.text = "Server hazýr deðil ama iþlem yapýlabilir.";
        }
        else
        {
            ocrResultText.text = "Server hazýr. Resim seçebilirsiniz.";
        }

        selectImageButton.interactable = true;
    }

    void OpenFilePicker()
    {
        if (OCRServerManager.Instance == null || !OCRServerManager.Instance.IsServerReady())
        {
            ocrResultText.text = "Server hazýr deðil, ama yine de deneyebiliriz.";
        }

#if UNITY_EDITOR
        selectedImagePath = EditorUtility.OpenFilePanel("Resim Seç", "", "png,jpg,jpeg");
#else
        var paths = StandaloneFileBrowser.OpenFilePanel("Resim Seç", "", new[] { new ExtensionFilter("Images", "png", "jpg", "jpeg") }, false);
        selectedImagePath = paths.Length > 0 ? paths[0] : null;
#endif

        if (!string.IsNullOrEmpty(selectedImagePath))
        {
            StartCoroutine(LoadImageAndSendOCR(selectedImagePath));
        }
    }

    IEnumerator LoadImageAndSendOCR(string path)
    {
        selectImageButton.interactable = false;
        ocrResultText.text = "OCR baþlatýlýyor...";

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        selectedImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        yield return SendImageForOCR(path);
        selectImageButton.interactable = true;
    }

    IEnumerator SendImageForOCR(string path)
    {
        if (!File.Exists(path))
        {
            ocrResultText.text = "Resim bulunamadý.";
            yield break;
        }

        var form = new WWWForm();
        form.AddBinaryData("image", File.ReadAllBytes(path), Path.GetFileName(path), "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post(OCRServerManager.Instance.GetServerUrl() + "/ocr", form))
        {
            www.timeout = 30;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<OCRResponse>(www.downloadHandler.text);
                ocrResultText.text = response != null ? string.Join("\n", response.text) : "OCR sonucu boþ!";
                if(ocrResultText.text == "DEDE")
                {
                    if (CSVDataCollector.csvInstance != null)
                        CSVDataCollector.csvInstance.ReceiveOCRResult(true);
                }
                changeScene.gameObject.SetActive(true);
            }
            else
            {
                ocrResultText.text = "OCR baþarýsýz: " + www.error;
            }
        }
    }
    void ChangeScene()
    {
        SceneManager.LoadScene(0);
    }

    [System.Serializable]
    public class OCRResponse
    {
        public string[] text;
    }
}
