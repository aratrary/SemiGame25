using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public string sceneName = "MainScene"; //디폴트값(얘는의미없고EndPoint가의미잇음)
    public float duration = 1f;
    public GameObject player;
    public Transform playerTF;

    private bool isTransitioning;

    void Awake()
    {
        //DontDestroyOnLoad(transform.root.gameObject); //씬 옮겨도 안부서지게
        DontDestroyOnLoad(gameObject);
    }

    public void Falling(string sceneName, /*float duration*/float x, float y)
    {
        if (isTransitioning) return;
        StartCoroutine(Eclipse(sceneName, x, y));
    }

    IEnumerator Eclipse(string sceneName, float x, float y)
    {
        /*
        Camera cam = Camera.main;
        if (cam == null) yield break;

        isTransitioning = true;
        transform.position = cam.transform.position + new Vector3(0, 10, 0); //카메라위치+(0,10,0)로 이동
        
        float kenka = 0f;
        while (kenka < duration)
        {
            //clamp는 최소/최대 잡아서 범위 벗어나면 최솟값최댓값으로 자동정리해주는함수(0~1버전)
            //Lerp는 뭐라하지 내분점찾는느낌임 kenka/duration은 완료율이니까 뭔느낌인지알지
            kenka += Time.deltaTime;
            transform.position = Vector3.Lerp(cam.transform.position + new Vector3(0, 10, 0), cam.transform.position, Mathf.Clamp01(kenka/duration));
            yield return null;//1프레임쉬기
        }
        transform.position = cam.transform.position;

        */
        Debug.Log("아무거나");
        var load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone) yield return null;

        player = GameObject.FindGameObjectWithTag("JK");
        playerTF = player.GetComponent<Transform>();
        
        playerTF.transform.position = new Vector3(x, y, 0);
        Destroy(gameObject);
        /*
        yield return null; //한프레임대기
        cam = Camera.main;
        if (cam == null) yield break;

        transform.position = cam.transform.position;//뭔지알지???위에보샘
        
        kenka = 0f;
        while (kenka < duration)
        {
            kenka += Time.deltaTime;
            transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position-new Vector3(0, 10, 0), Mathf.Clamp01(kenka/duration));
            yield return null;
        }
        transform.position = cam.transform.position-new Vector3(0, 10, 0);

        isTransitioning = false;
        */
    }

    internal void Falling(string sceneName, Collider2D collision, float x, float y)
    {
        throw new NotImplementedException();
    }
}
