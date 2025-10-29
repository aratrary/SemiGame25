using UnityEngine;

//이게 HP 조절하는겁니다

public class PlayerHealthController : MonoBehaviour
{
    // === 플레이어 체력 설정 ===
    public float maxHealth = 100f; // 플레이어의 최대 체력
    private float currentHealth; // 플레이어의 현재 체력

    // === UI 연결 설정 ===
    // Inspector에서 체력 바 UI를 관리하는 HealthBarDisplay 스크립트를 연결합니다.
    public HealthBarDisplay healthBarUI;

    void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;
        // 체력 바 UI를 초기 상태로 업데이트
        UpdateHealthUI();
    }

    /// <summary>
    /// 플레이어가 데미지를 입었을 때 호출하는 함수.
    /// </summary>
    /// <param name="damageAmount">입을 데미지 양.</param>
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount; // 체력 감소
        // 체력이 0 미만으로 내려가지 않도록 보정
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthUI(); // UI 업데이트

        // 체력이 0 이하면 사망 처리
        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " 사망! 게임 오버!");
            // TODO: 여기에 캐릭터 사망 또는 게임 오버 관련 로직 추가
            // (예: 캐릭터 애니메이션, 게임 재시작 화면 등)
        }
    }

    /// <summary>
    /// 플레이어가 체력을 회복할 때 호출하는 함수.
    /// </summary>
    /// <param name="healAmount">회복할 체력 양.</param>
    public void Heal(float healAmount)
    {
        currentHealth += healAmount; // 체력 증가
        // 체력이 최대치를 넘지 않도록 보정
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI(); // UI 업데이트
    }

    /// <summary>
    /// 현재 체력 값에 따라 체력 바 UI를 업데이트하는 내부 함수.
    /// </summary>
    private void UpdateHealthUI()
    {
        // healthBarUI가 Inspector에서 연결되어 있는지 확인
        if (healthBarUI != null)
        {
            // 현재 체력과 최대 체력을 사용하여 0.0f ~ 1.0f 사이의 비율 계산 후 UI에 전달
            healthBarUI.UpdateHealthBar(currentHealth / maxHealth);
        }
        else
        {
            Debug.LogWarning("HealthBarDisplay UI가 연결되지 않았습니다. Inspector에서 'Health Bar UI' 슬롯에 HealthBarDisplay 스크립트가 붙은 오브젝트를 연결해주세요.");
        }
    }

    // === 개발 중 테스트를 위한 임시 입력 ===
    // 실제 게임에서는 다른 스크립트나 이벤트에서 이 함수들을 호출하게 됩니다.
    void Update()
    {
        // Space 키를 누르면 데미지 10 입기
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10f);
        }
        // H 키를 누르면 체력 10 회복
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10f);
        }
    }
}
