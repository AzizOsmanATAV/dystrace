using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjects : MonoBehaviour
{
    public string eventID;

    void Awake()
    {
        // Eğer Inspector'da boş bırakılmışsa, otomatik olarak gameObject'in ismini ata.
        if (string.IsNullOrEmpty(eventID))
        {
            eventID = gameObject.name;
        }
    }
}
