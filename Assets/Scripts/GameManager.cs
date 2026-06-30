using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score = 0;

    public List<GameObject> mainSceneObjects = new List<GameObject>();

    public List<string> completedEventIDs = new List<string>();

    public List<GameObject> collectedObjects = new List<GameObject>();
    public bool isDestroyed;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

