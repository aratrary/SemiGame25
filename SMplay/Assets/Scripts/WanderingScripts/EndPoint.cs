using EasyTransition;
using NUnit.Framework;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    bool isLoading;
    public string how_transition = "Fade";

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("퍼리퍼리");
        if (isLoading) return;
        if (!collision.CompareTag("Player")) return;

        isLoading = true;
        TransitionManager.Instance().Transition("F2", how_transition, 0f);

        Debug.Log("퍼리");
    }
}
