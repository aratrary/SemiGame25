using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    //주석쓰기귀찮
    //어차피코드직관적인데굳이써야할까??????(사실써야함,근데안쓸거임)
    bool pausing = false;
    public GameObject panel;

    void Awake()
    {
        panel.SetActive(false);
        pausing = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausing) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausing = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
    }

    public void ResumeGame()
    {
        pausing = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
    }

    public void GoToMain()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy() //메인메뉴 같은걸로 나갔는데 멈춰있는거 방지
    {
        if (pausing)
        {
            Time.timeScale = 1f;
            pausing = false;
        }
    }
}
