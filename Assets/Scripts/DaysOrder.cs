using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class DaysOrder : MonoBehaviour
{
    public List<Button> dayButtons;
    public Button completeButton;
    public Button clearButton;
    private List<string> correctOrder = new List<string> { "Pazartesi", "Salý", "Çarþamba", "Perþembe", "Cuma", "Cumartesi", "Pazar" };
    public List<string> userSelection = new List<string>();
    public List<GameObject> availablePlaceholders;
    public GameObject[] placeholders;
    public Button anasayfaButton;
    public static DaysOrder instance { get; set; }
    public bool isCorrect;
    void Start()
    {
        anasayfaButton.onClick.AddListener(ReturnHomePage);
        anasayfaButton.gameObject.SetActive(false);
        foreach (Button button in dayButtons)
        {
            button.onClick.AddListener(() => OnDaySelection(button.GetComponentInChildren<Text>().text));
            button.onClick.AddListener(() => SetUnactiveChosenDay(button));
        }
        completeButton.gameObject.SetActive(false);
        completeButton.onClick.AddListener(CheckSelections);
        clearButton.onClick.AddListener(ResetGame);
        PlaceButtonsRandomly();
    }

    // Update is called once per frame
    void Update()
    {
        if (userSelection.Count == 7)
        {
            completeButton.gameObject.SetActive(true);
        }
        else
        {
            completeButton.gameObject.SetActive(false);
        }
    }

    void OnDaySelection(string day)
    {
        userSelection.Add(day);
        Debug.Log(day);
    }
    void SetUnactiveChosenDay(Button chosenDay)
    {
        chosenDay.gameObject.SetActive(false);
    }
    public void CheckSelections()
    {
        isCorrect = true;
        if (userSelection.Count != correctOrder.Count)
        {
            isCorrect = false;
        }
        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (userSelection[i] != correctOrder[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("Hepsi doðru");
        }
        else
        {
            Debug.Log("Yanlýþ");
            GameManager.instance.score += 2;
        }
        if (CSVDataCollector.csvInstance != null)
            CSVDataCollector.csvInstance.ReceiveDaysResult(isCorrect);
        anasayfaButton.gameObject.SetActive(true);

    }

    void ResetGame()
    {
        userSelection.Clear();
        Debug.Log("Temizlendi");
        foreach (Button button in dayButtons)
        {
            button.gameObject.SetActive(true);
        }
        completeButton.gameObject.SetActive(false);
        anasayfaButton.gameObject.SetActive(false);
    }
    void PlaceButtonsRandomly()
    {
        availablePlaceholders = new List<GameObject>(placeholders);
        foreach (Button button in dayButtons)
        {
            int randomIndex = Random.Range(0, availablePlaceholders.Count);
            GameObject selectedPlaceholder = availablePlaceholders[randomIndex];
            button.transform.position = selectedPlaceholder.transform.position;
            availablePlaceholders.RemoveAt(randomIndex);
            button.gameObject.SetActive(true);

        }
    }

    void ReturnHomePage()
    {
        SceneManager.LoadScene(0);

    }
}

