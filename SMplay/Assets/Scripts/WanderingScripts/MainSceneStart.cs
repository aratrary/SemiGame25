using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneStart : MonoBehaviour
{
    public void Transition(string toScene)
    {
        SceneManager.LoadScene(toScene);
    }
    /* 개
    public Camera caemura;
    private float duration = 0.5f; //떨어지는데 걸리는 시간(사실 저거보다 조금 더 오래걸림)

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public void Transition(string toScene)
    {
        StartCoroutine(Falling(toScene));
    }

    IEnumerator Falling(string toScene) //떨어지는거
    {
        float runTime = 0;
        Vector3 defPos = transform.position;
        caemura = Camera.main;
        
        while (runTime < duration)
        {
            runTime += Time.deltaTime;
            transform.position = Vector3.Lerp(defPos, caemura.transform.position, runTime/duration);
            yield return null;
        }

        SceneManager.LoadScene(toScene);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
    */
}
