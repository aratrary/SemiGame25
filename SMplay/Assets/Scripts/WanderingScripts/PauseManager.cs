using UnityEngine;

public class PauseManager : MonoBehaviour
{
    bool pausing = false;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!pausing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        pausing = true;
        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }

    void ResumeGame()
    {
        pausing = false;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (pausing)
        {
            Time.timeScale = 1f;
            pausing = false;
        }
    }
}
