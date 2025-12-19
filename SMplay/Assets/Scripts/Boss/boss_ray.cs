using UnityEngine;

public class boss_ray : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("2D 충돌 감지!");
            PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(15); // TakeDamage 호출!
            }
        }
    }
}
