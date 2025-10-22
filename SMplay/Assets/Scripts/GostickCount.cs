using UnityEngine;
using TMPro; // TextMeshPro 기능을 사용하기 위해 필요합니다.

public class GostickCountManager : MonoBehaviour
{
    [Header("UI References")] // 인스펙터에서 구분을 위한 헤더
    [SerializeField] private TextMeshProUGUI gostickCountText; // Gostick 개수를 표시할 TextMeshPro 컴포넌트

    [Header("Gostick Settings")] // 인스펙터에서 구분을 위한 헤더
    [SerializeField] private int initialGostickCount = 0; // 게임 시작 시 초기 고스틱 개수
    private int currentGostickCount; // 현재 고스틱 개수를 저장할 변수

    // 싱글톤 패턴 (선택 사항): 이 매니저에 쉽게 접근할 수 있도록 하는 패턴입니다.
    // 필요 없으면 제거해도 되지만, 일반적으로 이런 매니저는 싱글톤으로 많이 만듭니다.
    public static GostickCountManager Instance { get; private set; }

    void Awake()
    {
        // 싱글톤 패턴 초기화 (선택 사항)
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // 게임 시작 시 고스틱 개수를 초기화하고 UI를 업데이트합니다.
        currentGostickCount = initialGostickCount;
        UpdateGostickDisplay();
    }

    /// <summary>
    /// 현재 고스틱 개수를 반환합니다.
    /// </summary>
    public int GetCurrentGostickCount()
    {
        return currentGostickCount;
    }

    /// <summary>
    /// 고스틱 개수를 추가합니다.
    /// </summary>
    /// <param name="amount">추가할 고스틱 개수 (음수 가능)</param>
    public void AddGostick(int amount)
    {
        currentGostickCount += amount;
        // 고스틱 개수가 음수가 되지 않도록 방지 (필요 시)
        if (currentGostickCount < 0)
        {
            currentGostickCount = 0;
        }
        UpdateGostickDisplay(); // UI 업데이트
        Debug.Log($"Gostick 추가됨: {amount}, 현재 개수: {currentGostickCount}");
    }

    /// <summary>
    /// 고스틱 개수를 설정합니다.
    /// </summary>
    /// <param name="newCount">새로운 고스틱 개수</param>
    public void SetGostickCount(int newCount)
    {
        currentGostickCount = newCount;
        // 고스틱 개수가 음수가 되지 않도록 방지 (필요 시)
        if (currentGostickCount < 0)
        {
            currentGostickCount = 0;
        }
        UpdateGostickDisplay(); // UI 업데이트
        Debug.Log($"Gostick 개수 설정됨: {newCount}");
    }

    /// <summary>
    /// TextMeshPro UI에 현재 고스틱 개수를 반영합니다.
    /// </summary>
    private void UpdateGostickDisplay()
    {
        if (gostickCountText != null)
        {
            // string interpolation을 사용하여 가독성 좋게 텍스트를 업데이트합니다.
            gostickCountText.text = $"Gostick:{currentGostickCount}";
            // 또는 gostickCountText.text = $"고스틱: {currentGostickCount}개"; (디스플레이 형식 선택)
        }
        else
        {
            Debug.LogError("GostickCount TextMeshProUGUI가 GostickCountManager에 할당되지 않았습니다!");
        }
    }

    // 테스트용 예시 (나중에 제거하거나 필요에 따라 수정하세요)
    void Update()
    {
     // I 키를 누르면 고스틱 1개 추가
         if (Input.GetKeyDown(KeyCode.I))
         {
             AddGostick(1);
         }
         // O 키를 누르면 고스틱 1개 감소
         if (Input.GetKeyDown(KeyCode.O))
         {
             AddGostick(-1);
         }
     }
}
