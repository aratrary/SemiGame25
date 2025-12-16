using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
<<<<<<< Updated upstream
    // Start is called once before the first execution of Update after the MonoBehaviour is created
=======
    // === 플레이어 체력 설정 ===
    public float maxHealth = 100f; // 플레이어의 최대 체력
    public float currentHealth; // 플레이어의 현재 체력

    // === UI 연결 설정 ===
    // Inspector에서 체력 바 UI를 관리하는 HealthBarDisplay 스크립트를 연결합니다.
    public HealthBarDisplay healthBarUI;

>>>>>>> Stashed changes
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
