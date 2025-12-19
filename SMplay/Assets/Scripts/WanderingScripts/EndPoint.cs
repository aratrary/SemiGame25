using System;
using Unity.Collections;
using UnityEditor.Analytics;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public Transition transition;
    public string sceneName = "MainMenu"; // 디폴트값
    //public float duration = 1f;
    public float x;
    public float y;
/*
    private void Awake()
    {
        transition = transform.root.GetComponentInChildren<Transition>(true);
    }
*/
    void OnTriggerEnter2D(Collider2D collision)
    {
        haaaaaamsu(collision);
    }
    
    public void haaaaaamsu(Collider2D collision)
    {
        if (!collision.CompareTag("JK")) return;
        Debug.Log("Trying to loading "+sceneName+"...");

        if (transition != null)
        {
            Debug.Log("loading "+sceneName+"...");
            transition.Falling(sceneName, x, y);
        }
    }
}
