using UnityEngine;

/// <summary>
/// NPC나 상호작용 오브젝트에 부착되어 대화를 시작시키는 트리거 클래스
/// 플레이어가 접근했을 때 대화를 시작할 수 있게 함
/// </summary>
/// sex
public class DialogueTrigger : MonoBehaviour
{
    #region 설정 변수들
    [Header("대화 데이터")]
    /// <summary>
    /// 이 트리거가 실행할 대화 데이터
    /// Inspector에서 직접 설정하거나 스크립트로 동적 할당 가능
    /// </summary>
    public DialogueData dialogue;
    
    [Header("상호작용 설정")]
    /// <summary>
    /// 상호작용 키 (기본값: E키)
    /// Inspector에서 변경 가능
    /// </summary>
    public KeyCode interactionKey = KeyCode.E;
    
    /// <summary>
    /// 자동으로 대화를 시작할지 여부
    /// true: 플레이어가 범위에 들어오면 즉시 대화 시작
    /// false: 플레이어가 상호작용 키를 눌러야 대화 시작
    /// </summary>
    public bool autoTrigger = false;
    
    /// <summary>
    /// 대화를 한 번만 실행할지 여부
    /// true: 한 번 대화 후 더 이상 실행되지 않음
    /// false: 여러 번 대화 가능
    /// </summary>
    public bool oneTimeOnly = false;
    #endregion
    
    #region UI 참조 (선택사항)
    [Header("상호작용 UI (선택사항)")]
    /// <summary>
    /// "E키를 눌러 대화하기" 같은 상호작용 안내 UI
    /// 플레이어가 범위에 들어왔을 때 표시
    /// </summary>
    public GameObject interactionUI;
    #endregion
    
    #region 내부 상태 변수들
    /// <summary>
    /// 씬에서 찾은 DialogueSystem 컴포넌트의 참조
    /// 대화 시작 시 이 참조를 통해 메서드 호출
    /// </summary>
    private DialogueSystem dialogueSystem;
    
    /// <summary>
    /// 플레이어가 상호작용 범위 내에 있는지 확인하는 플래그
    /// OnTriggerEnter/Exit에서 설정됨
    /// </summary>
    private bool playerInRange = false;
    
    /// <summary>
    /// 이미 대화를 실행했는지 확인하는 플래그
    /// oneTimeOnly가 true일 때 사용
    /// </summary>
    private bool hasTriggered = false;
    
    /// <summary>
    /// 현재 상호작용 범위 내에 있는 플레이어 오브젝트의 참조
    /// null이면 플레이어가 범위 밖에 있음
    /// </summary>
    private GameObject currentPlayer;
    #endregion
    
    #region Unity 생명주기 메서드들
    /// <summary>
    /// 컴포넌트 초기화 메서드
    /// 씬에서 DialogueSystem을 찾고 초기 설정 수행
    /// </summary>
    void Start()
    {
        // 씬에 있는 DialogueSystem 컴포넌트를 자동으로 찾기
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        
        // DialogueSystem을 찾지 못한 경우 경고 출력
        if (dialogueSystem == null)
        {
            Debug.LogError($"{gameObject.name}: 씬에서 DialogueSystem을 찾을 수 없습니다!");
        }
        
        // 대화 데이터 유효성 검사
        if (dialogue == null)
        {
            Debug.LogWarning($"{gameObject.name}: 대화 데이터가 설정되지 않았습니다!");
        }
        
        // 상호작용 UI 초기 상태 설정 (숨김)
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        // 트리거 콜라이더 확인
        ValidateTriggerSetup();
    }
    
    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 메서드
    /// 플레이어 입력을 감지하고 상호작용 처리
    /// </summary>
    void Update()
    {
        // 플레이어가 범위 내에 있고, 자동 트리거가 아니며, 
        // 아직 실행되지 않았거나 여러 번 실행 가능한 경우
        if (playerInRange && !autoTrigger && CanTriggerDialogue())
        {
            // 설정된 상호작용 키가 눌렸는지 확인
            if (Input.GetKeyDown(interactionKey))
            {
                TriggerDialogue();
            }
        }
    }
    #endregion
    
    #region 트리거 이벤트 메서드들
    /// <summary>
    /// 다른 콜라이더가 이 오브젝트의 트리거 영역에 들어왔을 때 호출
    /// 2D 게임용 - 3D 게임에서는 OnTriggerEnter 사용
    /// </summary>
    /// <param name="other">들어온 콜라이더</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            HandlePlayerEnter(other.gameObject);
        }
    }
    
    /// <summary>
    /// 다른 콜라이더가 이 오브젝트의 트리거 영역에서 나갔을 때 호출
    /// </summary>
    /// <param name="other">나간 콜라이더</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // 나간 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            HandlePlayerExit(other.gameObject);
        }
    }
    
    // 3D 게임용 트리거 이벤트들 (2D와 동일한 로직)
    /// <summary>
    /// 3D 게임용 트리거 진입 이벤트
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerEnter(other.gameObject);
        }
    }
    
    /// <summary>
    /// 3D 게임용 트리거 퇴장 이벤트
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerExit(other.gameObject);
        }
    }
    #endregion
    
    #region 플레이어 상호작용 처리 메서드들
    /// <summary>
    /// 플레이어가 상호작용 범위에 들어왔을 때의 처리
    /// </summary>
    /// <param name="player">들어온 플레이어 오브젝트</param>
    private void HandlePlayerEnter(GameObject player)
    {
        // 플레이어 정보 저장
        currentPlayer = player;
        playerInRange = true;
        
        // 상호작용 UI 표시
        ShowInteractionUI();
        
        // 자동 트리거가 활성화되어 있고 대화가 가능한 상태라면
        if (autoTrigger && CanTriggerDialogue())
        {
            // 즉시 대화 시작
            TriggerDialogue();
        }
        
        // 플레이어 진입 이벤트 (확장 가능)
        OnPlayerEnterRange();
    }
    
    /// <summary>
    /// 플레이어가 상호작용 범위에서 나갔을 때의 처리
    /// </summary>
    /// <param name="player">나간 플레이어 오브젝트</param>
    private void HandlePlayerExit(GameObject player)
    {
        // 플레이어 정보 초기화
        currentPlayer = null;
        playerInRange = false;
        
        // 상호작용 UI 숨김
        HideInteractionUI();
        
        // 플레이어 퇴장 이벤트 (확장 가능)
        OnPlayerExitRange();
    }
    
    /// <summary>
    /// 대화를 시작할 수 있는 조건인지 확인
    /// </summary>
    /// <returns>대화 시작 가능 여부</returns>
    private bool CanTriggerDialogue()
    {
        // 기본 조건들 확인
        if (dialogueSystem == null || dialogue == null)
            return false;
        
        // 이미 대화가 진행 중인지 확인
        if (dialogueSystem.IsDialogueActive())
            return false;
        
        // 한 번만 실행하는 옵션이 켜져있고 이미 실행되었는지 확인
        if (oneTimeOnly && hasTriggered)
            return false;
        
        return true;
    }
    #endregion
    
    #region 공개 메서드들
    /// <summary>
    /// 대화를 시작하는 메인 메서드
    /// 외부에서도 호출 가능 (다른 스크립트에서 강제로 대화 시작 시)
    /// </summary>
    public void TriggerDialogue()
    {
        // 대화 시작 조건 재확인
        if (!CanTriggerDialogue())
        {
            Debug.LogWarning($"{gameObject.name}: 대화를 시작할 수 없는 상태입니다.");
            return;
        }
        
        // 대화 시작 전 이벤트
        OnDialogueTriggered();
        
        // DialogueSystem에 대화 시작 요청
        dialogueSystem.StartDialogue(dialogue);
        
        // 한 번만 실행 옵션이 켜져있다면 플래그 설정
        if (oneTimeOnly)
        {
            hasTriggered = true;
            // 상호작용 UI도 영구적으로 숨김
            HideInteractionUI();
        }
        
        Debug.Log($"{gameObject.name}: 대화를 시작했습니다.");
    }
    
    /// <summary>
    /// 대화 데이터를 런타임에 변경하는 메서드
    /// 동적으로 대화 내용을 바꿀 때 사용
    /// </summary>
    /// <param name="newDialogue">새로운 대화 데이터</param>
    public void SetDialogue(DialogueData newDialogue)
    {
        dialogue = newDialogue;
        
        // 한 번만 실행 플래그 리셋 (새 대화이므로)
        if (oneTimeOnly)
        {
            hasTriggered = false;
        }
        
        Debug.Log($"{gameObject.name}: 대화 데이터가 변경되었습니다.");
    }
    
    /// <summary>
    /// 트리거를 다시 활성화하는 메서드
    /// oneTimeOnly 옵션이 켜져있어도 다시 사용 가능하게 함
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"{gameObject.name}: 트리거가 리셋되었습니다.");
    }
    #endregion
    
    #region UI 관리 메서드들
    /// <summary>
    /// 상호작용 UI를 표시하는 메서드
    /// </summary>
    private void ShowInteractionUI()
    {
        if (interactionUI != null && CanTriggerDialogue() && !autoTrigger)
        {
            interactionUI.SetActive(true);
            
            // UI 텍스트 동적 설정 (상호작용 키에 따라)
            UpdateInteractionUIText();
        }
    }
    
    /// <summary>
    /// 상호작용 UI를 숨기는 메서드
    /// </summary>
    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 상호작용 UI의 텍스트를 업데이트하는 메서드
    /// 설정된 키에 따라 "E키를 눌러..." 형태로 표시
    /// </summary>
    private void UpdateInteractionUIText()
    {
        if (interactionUI != null)
        {
            // UI 내부의 텍스트 컴포넌트 찾기
            var textComponent = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }
            
            // 텍스트 설정
            if (textComponent != null)
            {
                textComponent.text = $"{interactionKey}키를 눌러 대화하기";
            }
        }
    }
    #endregion
    
    #region 이벤트 메서드들 (확장 가능)
    /// <summary>
    /// 플레이어가 상호작용 범위에 들어왔을 때 호출되는 이벤트
    /// </summary>
    protected virtual void OnPlayerEnterRange()
    {
        // 상속받는 클래스에서 오버라이드하여 추가 기능 구현 가능
        // 예: 캐릭터 애니메이션 변경, 사운드 재생 등
    }
    
    /// <summary>
    /// 플레이어가 상호작용 범위에서 나갔을 때 호출되는 이벤트
    /// </summary>
    protected virtual void OnPlayerExitRange()
    {
        // 상속받는 클래스에서 오버라이드하여 추가 기능 구현 가능
    }
    
    /// <summary>
    /// 대화가 시작되기 직전에 호출되는 이벤트
    /// </summary>
    protected virtual void OnDialogueTriggered()
    {
        // 예: 캐릭터가 플레이어 쪽을 바라보게 하기, 특수 효과 재생 등
    }
    #endregion
    
    #region 유틸리티 및 디버그 메서드들
    /// <summary>
    /// 트리거 설정이 올바른지 검증하는 메서드
    /// </summary>
    private void ValidateTriggerSetup()
    {
        // 콜라이더 컴포넌트 확인
        Collider2D collider2D = GetComponent<Collider2D>();
        Collider collider3D = GetComponent<Collider>();
        
        if (collider2D == null && collider3D == null)
        {
            Debug.LogError($"{gameObject.name}: 트리거 기능을 위해 Collider 컴포넌트가 필요합니다!");
            return;
        }
        
        // 트리거 설정 확인
        bool isTrigger = false;
        if (collider2D != null)
        {
            isTrigger = collider2D.isTrigger;
        }
        else if (collider3D != null)
        {
            isTrigger = collider3D.isTrigger;
        }
        
        if (!isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider의 'Is Trigger' 옵션이 체크되지 않았습니다!");
        }
    }
    
    /// <summary>
    /// 현재 트리거 상태를 확인하는 메서드 (디버그용)
    /// </summary>
    /// <returns>트리거 상태 정보</returns>
    public string GetTriggerStatus()
    {
        return $"플레이어 범위 내: {playerInRange}, 실행됨: {hasTriggered}, 대화 가능: {CanTriggerDialogue()}";
    }
    #endregion
    
    #region Inspector 버튼들 (에디터 전용)
#if UNITY_EDITOR
    /// <summary>
    /// Inspector에서 테스트 대화 실행 버튼
    /// 에디터 모드에서만 사용 가능
    /// </summary>
    [ContextMenu("테스트 대화 실행")]
    private void TestTriggerDialogue()
    {
        if (Application.isPlaying)
        {
            TriggerDialogue();
        }
        else
        {
            Debug.Log("플레이 모드에서만 테스트 가능합니다.");
        }
    }
    
    /// <summary>
    /// Inspector에서 트리거 리셋 버튼
    /// </summary>
    [ContextMenu("트리거 리셋")]
    private void TestResetTrigger()
    {
        ResetTrigger();
    }
#endif
    #endregion
}