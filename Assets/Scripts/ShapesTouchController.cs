using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShapesTouchController : MonoBehaviour
{
    public List<GameObject> shapes;
    public List<AudioClip> voices = new List<AudioClip>();
    public AudioSource audioSource;
    public TextMeshProUGUI text;
    public Button anasayfaButton;
    public bool hasScored;
    public bool isCorrect;
    [SerializeField] int falseCount;
    [SerializeField] List<int> numberPool = new List<int>();
    [SerializeField] int currentIndex;
    [SerializeField] int orderPosition = 0;
    void Start()
    {
        falseCount = 0;
        audioSource = GetComponent<AudioSource>();

        for (int i = 0; i <= 19; i++)
        {
            numberPool.Add(i);
        }

        ShufflePool();

        Play();
        anasayfaButton.gameObject.SetActive(false);
        anasayfaButton.onClick.AddListener(ReturnHomePage);
        isCorrect = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        { 
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null) 
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    if (hit.collider.gameObject == shapes[i].gameObject)
                    {
                        if (currentIndex == i)
                        {
                            Debug.Log("Doðru");
                        }
                        else
                        {
                            Debug.Log("Yanlýþ");
                            falseCount++;
                        }

                        GetNextNumber();

                        if (currentIndex == -1)
                        {
                            Debug.Log("Tamamlandý");
                            text.SetText("Tamamlandý!");
                            anasayfaButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            Play();
                        }

                        break;
                    }
                }
            }

        }
        CSVDataCollector.csvInstance.colorsFalseCount = falseCount;
    }
    void Play()
    {
        if (voices.Count > 0)
        {
            audioSource.clip = voices[currentIndex];
            audioSource.Play();
        }
    }
    void ReturnHomePage()
    {
        SceneManager.LoadScene(0);
    }
    void ShufflePool()
    {
        for (int i = 0; i < numberPool.Count; i++)
        {
            int randomIndex = Random.Range(i, numberPool.Count);

            int temp = numberPool[i];
            numberPool[i] = numberPool[randomIndex];
            numberPool[randomIndex] = temp;
        }

        currentIndex = numberPool[0];
        orderPosition = 0;
    }
    public int GetNextNumber()
    {
        orderPosition++;

        if (orderPosition >= numberPool.Count)
        {
            currentIndex = -1; // Oyun bitti.
            return -1;
        }

        currentIndex = numberPool[orderPosition];
        return currentIndex;
    }
}
