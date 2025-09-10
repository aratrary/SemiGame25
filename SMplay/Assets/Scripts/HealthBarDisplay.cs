using UnityEngine;
using UnityEngine.UI; // UI.Image 컴포넌트를 사용하기 위해 필요합니다.

//이 함수는 UI만 조작하는거지 체력에 직접적으로 관여하는건 아니비다

public class HealthBarDisplay : MonoBehaviour
{
    // 이 변수에 체력 바 이미지(UI.Image) 컴포넌트를 연결합니다.
    // 이 스크립트가 붙어있는 오브젝트의 Image 컴포넌트를 자동으로 가져옵니다.
    private Image healthGaugeImage;

    void Awake() // Start()보다 먼저 호출되어 초기화를 확실히 합니다.
    {
        // 스크립트가 붙어있는 GameObject에서 Image 컴포넌트를 가져옵니다.
        // 이 Image 컴포넌트의 'Image Type'이 'Filled'로 설정되어 있어야 합니다.
        healthGaugeImage = GetComponent<Image>();

        // 만약 Image 컴포넌트를 찾지 못했다면 경고 메시지를 출력합니다.
        if (healthGaugeImage == null)
        {
            Debug.LogError("HealthBarDisplay 스크립트는 Image 컴포넌트가 있는 오브젝트에 붙여야 합니다.");
            enabled = false; // 스크립트 비활성화
        }
    }

    /// <summary>
    /// 체력 바의 현재 비율을 업데이트하는 함수.
    /// 이 함수는 플레이어 체력 스크립트에서 호출됩니다.
    /// </summary>
    /// <param name="currentHealthRatio">현재 체력 / 최대 체력 값 (0.0f ~ 1.0f).</param>
    public void UpdateHealthBar(float currentHealthRatio)
    {
        if (healthGaugeImage != null)
        {
            healthGaugeImage.fillAmount = currentHealthRatio; // Image의 fillAmount 속성을 조절
        }
    }
}