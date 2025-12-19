using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject starty;
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        starty.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("충돌 감지됨");
        starty.SetActive(true);
        player.transform.position = new Vector3(-5, 18, 0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
