using System;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public Transition transition;
    public string sceneName = "MainMenu"; // 디폴트값
    public float duration = 1f;
/*
    private void Awake()
    {
        transition = transform.root.GetComponentInChildren<Transition>(true);
    }
*/
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (transition != null)
        {
            transition.Falling(sceneName, duration);
        }
    }
}
