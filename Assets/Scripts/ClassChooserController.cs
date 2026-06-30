using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ClassChooserController : MonoBehaviour
{
    public int classChooser;
    public GameObject canvasToDisable;

    public void OnButtonClicked(Button button)
    {
        TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            if (int.TryParse(tmpText.text, out int selectedClass))
            {
                Debug.Log("Seçilen class: " + selectedClass);
                classChooser = selectedClass;
                if (CSVDataCollector.csvInstance != null)
                    CSVDataCollector.csvInstance.classOfStudent = classChooser;
                if (canvasToDisable != null)
                {
                    Destroy(canvasToDisable);
                    GameManager.instance.isDestroyed = true;
                }
                else
                {
                    Debug.LogWarning("Canvas objesi atanmadý!");
                }
            }
            else
            {
                Debug.LogError("Buton text int'e çevrilemedi: " + tmpText.text);
            }
        }
        else
        {
            Debug.LogError("TMP component bulunamadý!");
        }
    }
    void Update()
    {
        if (GameManager.instance.isDestroyed)
        {
            Destroy(canvasToDisable);
        }
    }
}
