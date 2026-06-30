using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    private float xBound = 8;
    private float yBound = 4;
    public float horizontalInput;
    public float verticalInput;
    
    void Start()
    {
        
    }

    void Update()
    {
        Movement();
        BoundControl();
    }
    void Movement()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        transform.Translate( new Vector2(horizontalInput, verticalInput) * speed * Time.deltaTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
        SceneManager.LoadScene(int.Parse(collision.gameObject.name));
        if (collision.gameObject.CompareTag("EventObject"))
        {
            EventObjects eo = collision.gameObject.GetComponent<EventObjects>();
            if(GameManager.instance.mainSceneObjects.Contains(collision.gameObject))
            {
                GameManager.instance.collectedObjects.Add(collision.gameObject);
                GameManager.instance.mainSceneObjects.Remove(collision.gameObject);
                if(eo != null)
                {
                    if(!GameManager.instance.completedEventIDs.Contains(eo.eventID))
                    {
                        GameManager.instance.completedEventIDs.Add(eo.eventID);
                    }
                }
                Destroy(collision.gameObject);
            }
        }
    }
    void BoundControl()
    {
        if(transform.position.x > xBound)
        {
            transform.position = new Vector2(xBound, transform.position.y);
        }
        if (transform.position.x < -xBound)
        {
            transform.position = new Vector2(-xBound, transform.position.y);
        }
        if (transform.position.y > yBound)
        {
            transform.position = new Vector2(transform.position.x, yBound);
        }
        if (transform.position.y < -yBound)
        {
            transform.position = new Vector2(transform.position.x, -yBound);
        }
    }
}
