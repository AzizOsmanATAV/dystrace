using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneInitializer : MonoBehaviour
{
    public GameObject[] eventObjects; // Inspector üzerinden 4 tane event nesnesini ata.

    void Start()
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.mainSceneObjects.Clear();
            foreach(GameObject obj in eventObjects)
            {
                EventObjects eo = obj.GetComponent<EventObjects>();
                if(eo != null)
                {
                    // Eğer bu event daha önce tamamlanmışsa, nesneyi kapat.
                    if(GameManager.instance.completedEventIDs.Contains(eo.eventID))
                    {
                        obj.SetActive(false);
                    }
                    else
                    {
                        obj.SetActive(true);
                        GameManager.instance.mainSceneObjects.Add(obj);
                    }
                }
                else
                {
                    // Eğer event nesnesi scripti yoksa direkt ekle.
                    GameManager.instance.mainSceneObjects.Add(obj);
                }
            }
        }
    }
}
