using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임의 대화 시스템을 관리하는 메인 클래스
/// 타이핑 효과, 대화 진행, UI 제어 등을 담당
/// </summary>
/// sex


[DefaultExecutionOrder(-200)]
public class DialogueSystem : MonoBehaviour
{
    #region UI 참조 변수들
    [Header("UI 참조")]
    /// <summary>
    /// 대화창 전체를 감싸는 패널 오브젝트
    /// 대화 시작/종료 시 활성화/비활성화됨
    /// </summary>
    public GameObject dialoguePanel;
    
    /// <summary>
    /// 캐릭터 이름을 표시하는 TextMeshPro 컴포넌트
    /// 대화창 상단에 위치
    /// </summary>
    public TextMeshProUGUI characterNameText;
    
    /// <summary>
    /// 실제 대화 내용을 표시하는 TextMeshPro 컴포넌트
    /// 타이핑 효과가 적용되는 메인 텍스트
    /// </summary>
    public TextMeshProUGUI dialogueText;
    
    /// <summary>
    /// "계속하려면 스페이스바를 누르세요" 같은 표시기
    /// 타이핑 완료 후에만 활성화됨
    /// </summary>
    public GameObject continueIndicator;

    /// <summary>
    /// 캐릭터 초상화를 표시하는 Image 컴포넌트 (선택사항)
    /// </summary>
    public Image characterPortrait;
    #endregion
    
    #region 대화 설정 변수들
    [Header("대화 시스템 설정")]
    /// <summary>
    /// 타이핑 효과의 속도 (초 단위)
    /// 0.02f = 한 글자당 0.02초 간격으로 표시
    /// 값이 작을수록 빠르게 타이핑됨
    /// </summary>
    [Range(0.01f, 0.1f)]
    public float typingSpeed = 0.02f;
    
    /// <summary>
    /// 대화창 등장/퇴장 애니메이션 속도
    /// </summary>
    [Range(0.1f, 2.0f)]
    public float animationSpeed = 0.5f;
    #endregion
    
    #region 내부 상태 변수들
    /// <summary>
    /// 대화 문장들을 순서대로 관리하는 큐(Queue) 자료구조
    /// FIFO(First In, First Out) 방식으로 문장들을 처리
    /// </summary>
    private Queue<string> sentences;
    
    /// <summary>
    /// 현재 타이핑 효과가 진행 중인지 확인하는 플래그
    /// true: 타이핑 중, false: 타이핑 완료 또는 대기 중
    /// </summary>
    private bool isTyping = false;
    
    /// <summary>
    /// 대화 시스템이 현재 활성화되어 있는지 확인하는 플래그
    /// true: 대화 진행 중, false: 대화 종료 상태
    /// </summary>
    private bool dialogueActive = false;
    
    /// <summary>
    /// 현재 타이핑 중인 코루틴의 참조
    /// 타이핑 중단 시 사용
    /// </summary>
    private Coroutine typingCoroutine;
    /// <summary>
    /// 현재 출력 중인 문장 캐시 (타이핑 스킵용)
    /// </summary>
    private string currentSentence = string.Empty;
    private enum UIState { None, Dialogue }
    // ⭐ UI 상태 정의 ⭐
    private UIState currentUIState = UIState.None; 
    // ⭐ 현재 UI 상태 변수 ⭐
    [Header("게임 대화 시퀀스 목록")]
    public List<DialogueData> gameDialogueSequences = new List<DialogueData>();
    #endregion
    
    #region Unity 생명주기 메서드들
    /// <summary>
    /// 게임 시작 시 한 번 호출되는 초기화 메서드
    /// </summary>
    void Awake()
    {
        // 필드가 비어있으면 자동으로 찾아서 할당
        AutoAssignFields();
    }

    void Start()
    {
        // Awake에서 필드를 잡았지만 혹시 모르니 한 번 더 확인
        if (dialoguePanel == null || dialogueText == null || characterNameText == null)
        {
            AutoAssignFields();
        }
        
        // 문장 큐 초기화 (빈 큐 생성)
        sentences = new Queue<string>();
        
        // 게임 시작 시 대화창을 숨김 상태로 설정 (null 안전 처리)
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DialogueSystem: dialoguePanel이 연결되지 않았습니다. 자동 연결 이름(\"Dialogue\")을 확인하세요.");
        }
        
        // 계속하기 표시기도 초기에는 숨김
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // 초상화도 초기에는 숨김
        if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }

        currentUIState = UIState.None; // ⭐ 시작 시 UI 상태는 None ⭐

        if (gameDialogueSequences.Count > 0)
    {
        StartDialogue(gameDialogueSequences[0]);
    }
    }
    
    /// <summary>
    /// Inspector 필드가 비어있으면 자동으로 씬에서 찾아서 할당
    /// </summary>
    private void AutoAssignFields()
    {
        // dialoguePanel
        if (dialoguePanel == null)
        {
            // 1) Canvas 하위에서 이름으로 직접 찾기
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var dialogueTransform = canvas.transform.Find("Dialogue");
                if (dialogueTransform != null)
                    dialoguePanel = dialogueTransform.gameObject;
            }

            // 2) 씬 전체에서 이름으로 찾기 (활성 오브젝트)
            if (dialoguePanel == null)
            {
                dialoguePanel = GameObject.Find("Dialogue");
            }

            // 3) 흔한 변형 이름으로 검색 (대소문자 무시, 활성 오브젝트만)
            if (dialoguePanel == null)
            {
                foreach (var go in FindObjectsOfType<GameObject>())
                {
                    var name = go.name.ToLowerInvariant();
                    if (name.Contains("dialog") && go.GetComponent<RectTransform>() != null)
                    {
                        dialoguePanel = go;
                        break;
                    }
                }
            }

            // 4) dialogueText의 부모를 패널로 사용 (마지막 안전장치)
            if (dialoguePanel == null && dialogueText != null)
            {
                var parent = dialogueText.transform.parent;
                if (parent != null)
                {
                    dialoguePanel = parent.gameObject;
                }
            }

            if (dialoguePanel == null)
                Debug.LogWarning("DialoguePanel을 찾을 수 없습니다! 이름을 'Dialogue'로 하거나 DialogueText의 부모를 패널로 사용하세요.");
        }
        
        // characterNameText (먼저 찾음: 이름 텍스트를 우선 확정해야 대사 텍스트와 혼동 방지)
        if (characterNameText == null)
        {
            TextMeshProUGUI TryFindNameIn(Transform root)
            {
                if (root == null) return null;
                var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    var n = t.gameObject.name.ToLowerInvariant();
                    if (n.Contains("charactername") || n.Equals("name") || n.Contains("speaker"))
                        return t;
                }
                return null;
            }

            // 1) 패널 내부 우선 탐색
            characterNameText = TryFindNameIn(dialoguePanel != null ? dialoguePanel.transform : null);

            // 2) 정확한 이름 탐색
            if (characterNameText == null)
            {
                var nameObj = GameObject.Find("CharacterName");
                if (nameObj != null) characterNameText = nameObj.GetComponent<TextMeshProUGUI>();
            }

            // 3) 전체 탐색 (대소문자 무시 키워드)
            if (characterNameText == null)
            {
                foreach (var t in FindObjectsOfType<TextMeshProUGUI>(true))
                {
                    var n = t.gameObject.name.ToLowerInvariant();
                    if (n.Contains("charactername") || n.Equals("name") || n.Contains("speaker"))
                    {
                        characterNameText = t;
                        break;
                    }
                }
            }

            if (characterNameText == null)
                Debug.LogWarning("CharacterNameText를 찾을 수 없습니다! 이름 오브젝트를 'CharacterName' 또는 'Name'으로 지정하면 자동 연결됩니다.");
        }

        // dialogueText (대사 텍스트는 이름 텍스트와 다른 컴포넌트여야 함)
        if (dialogueText == null)
        {
            TextMeshProUGUI TryFindDialogueIn(Transform root)
            {
                if (root == null) return null;
                var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (characterNameText != null && t == characterNameText) continue;
                    var n = t.gameObject.name.ToLowerInvariant();
                    // 이름 후보를 제외하고, 대사에 자주 쓰는 키워드 선호
                    if (!n.Contains("name") && !n.Contains("speaker") &&
                        (n.Contains("dialog") || n.Contains("text") || n.Contains("content") || n.Contains("body") || n.Contains("line") || n.Contains("message")))
                        return t;
                }
                // 키워드가 없으면 이름 텍스트와 다른 첫 번째 TMP를 반환
                foreach (var t in tmps)
                {
                    if (characterNameText != null && t == characterNameText) continue;
                    var n = t.gameObject.name.ToLowerInvariant();
                    if (!n.Contains("name") && !n.Contains("speaker"))
                        return t;
                }
                return null;
            }

            // 1) 정확한 이름 우선
            var textObj = GameObject.Find("DialogueText");
            if (textObj != null) dialogueText = textObj.GetComponent<TextMeshProUGUI>();

            // 2) 패널 내부 탐색
            if (dialogueText == null)
                dialogueText = TryFindDialogueIn(dialoguePanel != null ? dialoguePanel.transform : null);

            // 3) 전체 탐색 (이름 텍스트와 다른 컴포넌트 선택)
            if (dialogueText == null)
            {
                foreach (var t in FindObjectsOfType<TextMeshProUGUI>(true))
                {
                    if (characterNameText != null && t == characterNameText) continue;
                    var n = t.gameObject.name.ToLowerInvariant();
                    if (!n.Contains("name") && !n.Contains("speaker"))
                    {
                        dialogueText = t;
                        break;
                    }
                }
            }

            if (dialogueText == null)
                Debug.LogWarning("DialogueText를 찾을 수 없습니다! 대사 텍스트 오브젝트를 'DialogueText'로 이름 지정하면 자동 연결됩니다.");
        }

        // 최종 안전장치: 두 참조가 같은 컴포넌트를 가리키면 재시도
        if (dialogueText != null && characterNameText != null && dialogueText == characterNameText)
        {
            Debug.LogWarning("DialogueSystem: characterNameText와 dialogueText가 동일한 컴포넌트를 참조합니다. 재검색을 시도합니다.");
            // 이름 텍스트를 유지하고, 대사 텍스트만 다시 엄격 기준으로 탐색
            TextMeshProUGUI strict = null;
            if (dialoguePanel != null)
            {
                foreach (var t in dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (t == characterNameText) continue;
                    var n = t.gameObject.name.ToLowerInvariant();
                    if (n.Contains("dialog") || n.Equals("dialoguetext") || n.Contains("content") || n.Contains("body") || n.Contains("line") || n.Contains("message"))
                    {
                        strict = t; break;
                    }
                }
            }
            if (strict != null) dialogueText = strict;
        }
        
        // continueIndicator (버튼)
        if (continueIndicator == null)
        {
            continueIndicator = GameObject.Find("ContinueButton");
            if (continueIndicator == null)
                continueIndicator = GameObject.Find("ContinueIndicator");
            
            if (continueIndicator == null)
                Debug.LogWarning("ContinueIndicator를 찾을 수 없습니다!");
        }
        
        // characterPortrait
        if (characterPortrait == null)
        {
            var portraitObj = GameObject.Find("CharacterPortrait");
            if (portraitObj != null)
                characterPortrait = portraitObj.GetComponent<Image>();
            else
            {
                var allImages = FindObjectsOfType<Image>();
                foreach (var img in allImages)
                {
                    if (img.gameObject.name.Contains("Portrait"))
                    {
                        characterPortrait = img;
                        break;
                    }
                }
            }
            
            if (characterPortrait == null)
                Debug.LogWarning("CharacterPortrait를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 메서드
    /// 사용자 입력을 감지하고 대화 진행을 제어
    /// </summary>
    void Update()
    {
        // 대화가 활성화되어 있고 스페이스바가 눌렸을 때만 처리
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarInput();
        }

        if (Input.GetKeyDown(KeyCode.T))
    {
        Debug.Log("T 키 눌림 감지!"); // ⭐ 임시로 추가하여 T 키 입력 자체를 확인해 보세요 ⭐
        ToggleUIPanel();
    }
        
        // ESC 키로 대화 강제 종료 (선택사항)
        if (dialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }
    #endregion
    
    #region 공개 메서드들 (외부에서 호출 가능)
    /// <summary>
    /// 새로운 대화를 시작하는 메서드
    /// 외부 스크립트(DialogueTrigger 등)에서 호출
    /// </summary>
    /// <param name="dialogue">표시할 대화 데이터</param>
    /// <param name="keepPanelOpen">패널이 이미 열려있으면 유지 (연속 대화용)</param>
    public void StartDialogue(DialogueData dialogue, bool keepPanelOpen = false)
    {
        if (dialoguePanel == null)
        {
            AutoAssignFields();
            if (dialoguePanel == null)
            {
                Debug.LogError("DialogueSystem: dialoguePanel이 여전히 null입니다. 씬에 대화 패널이 있는지 확인하세요.");
                return;
            }
        }
        Debug.Log($"StartDialogue 호출됨 - keepPanelOpen: {keepPanelOpen}, dialogueActive: {dialogueActive}");
        
        // 이미 대화가 진행 중이고 keepPanelOpen이 아니면 새 대화 시작 방지
        if (dialogueActive && !keepPanelOpen)
        {
            Debug.LogWarning("대화가 이미 진행 중입니다!");
            return;
        }
        
        // 대화 데이터 유효성 검사
        if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0)
        {
            Debug.LogError("유효하지 않은 대화 데이터입니다!");
            return;
        }


        // 대화 시스템 활성화
        dialogueActive = true;
        currentUIState = UIState.Dialogue; // 대화 시작하면 무조건 대화창 상태
        
        // 대화창 패널 활성화 (화면에 표시) - keepPanelOpen이면 이미 열려있으므로 스킵
        if (!keepPanelOpen && dialoguePanel != null)
        {
            Debug.Log("대화 패널을 활성화합니다.");
            dialoguePanel.SetActive(true);
        }
        else
        {
            Debug.Log("대화 패널 유지 (keepPanelOpen=true)");
            // keepPanelOpen이어도 실제로 꺼져있으면 켜야 함
            if (dialoguePanel != null && !dialoguePanel.activeSelf)
            {
                Debug.Log("패널이 꺼져있어서 다시 활성화합니다.");
                dialoguePanel.SetActive(true);
            }
        }
        
        // 캐릭터 이름 설정 (빈 문자열이면 "???" 표시)
        characterNameText.text = string.IsNullOrEmpty(dialogue.characterName) ? "???" : dialogue.characterName;
        
        // 캐릭터 초상화 설정
        SetCharacterPortrait(dialogue.characterPortrait);
        
        // 기존 큐에 남아있던 문장들을 모두 제거 (이전 대화 정리)
        sentences.Clear();
        
        // 새로운 대화의 모든 문장을 큐에 순서대로 추가
        foreach (string sentence in dialogue.sentences)
        {
            // 빈 문장이 아닌 경우에만 큐에 추가
            if (!string.IsNullOrEmpty(sentence.Trim()))
            {
                sentences.Enqueue(sentence);
            }
        }
        
        // 첫 번째 문장 표시 시작
        DisplayNextSentence();
        
        // 대화 시작 이벤트 (다른 시스템에서 사용할 수 있음)
        OnDialogueStart();
    }
    
    /// <param name="keepPanelOpen">패널을 열어둔 채로 종료 (연속 대화용)</param>
    public void EndDialogue(bool keepPanelOpen = false)
    {
        Debug.Log("EndDialogue 호출됨!");
        
        // 타이핑 코루틴이 실행 중이면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 모든 상태 플래그 리셋
        dialogueActive = false;
        isTyping = false;

        // UI 요소들 비활성화 - keepPanelOpen이면 패널은 유지
        if (!keepPanelOpen && dialoguePanel != null)
        {
            Debug.Log("dialoguePanel을 비활성화합니다.");
            dialoguePanel.SetActive(false);
        }
        currentUIState = UIState.None; // 대화 종료하면 UI 상태는 None
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        // 초상화 숨기기
        if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }
        
        // 텍스트 초기화
        dialogueText.text = "";
        characterNameText.text = "";
        
        // 큐 정리
        sentences.Clear();
        currentSentence = string.Empty;
        
        // 대화 종료 이벤트
        OnDialogueEnd();
    }

    /// <summary>
    /// UI 버튼에서도 호출 가능하도록 공개한 진행 메서드
    /// </summary>
    public void AdvanceDialogueViaButton()
    {
        if (!dialogueActive)
        {
            Debug.LogWarning("대화가 활성화되어 있지 않아 버튼 입력을 무시합니다.");
            return;
        }
        Debug.Log("AdvanceDialogueViaButton 호출됨");
        HandleSpacebarInput();
    }

    /// <summary>
    /// 버튼 OnClick 연결 여부를 빠르게 확인하기 위한 디버그용 메서드
    /// </summary>
    public void DebugButtonPing()
    {
        Debug.Log("DialogueSystem DebugButtonPing - OnClick 연결 확인됨");
    }
    #endregion
    
    #region 내부 메서드들 (private)
    /// <summary>
    /// 스페이스바 입력 처리 로직
    /// 상황에 따라 다른 동작 수행
    /// </summary>
    private void HandleSpacebarInput()
    {
        if (isTyping)
        {
            // 타이핑 중이면 즉시 완성
            CompleteCurrentTyping();
        }
        else
        {
            // 타이핑 완료 상태면 다음 문장으로 진행
            DisplayNextSentence();
        }
    }
    
    /// <summary>
    /// 큐에서 다음 문장을 가져와서 표시하는 메서드
    /// 더 이상 문장이 없으면 대화 종료
    /// </summary>
    private void DisplayNextSentence()
    {
        // 큐가 비어있으면 (더 이상 표시할 문장이 없으면)
        if (sentences.Count == 0)
        {
            Debug.Log("문장이 모두 끝났습니다. 대화를 종료합니다.");
            // 대화 상태만 비활성화 (패널은 GameStarter가 관리)
            dialogueActive = false;
            isTyping = false;
            return;
        }
        
        Debug.Log($"남은 문장 수: {sentences.Count}");
        
        // 큐에서 다음 문장을 추출 (FIFO 방식)
        string sentence = sentences.Dequeue();
        currentSentence = sentence;
        
        // 이전 타이핑 코루틴이 있다면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 새로운 문장 타이핑 시작
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }
    
    /// <summary>
    /// 문장을 한 글자씩 타이핑하는 효과를 구현하는 코루틴
    /// </summary>
    /// <param name="sentence">타이핑할 문장</param>
    /// <returns>코루틴 IEnumerator</returns>
    private IEnumerator TypeSentence(string sentence)
    {
        // 타이핑 시작 상태 설정
        isTyping = true;
        
        // 계속하기 표시기 숨김 (타이핑 중에는 표시하지 않음)
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        // 대화 텍스트 초기화 (빈 문자열로 시작)
        dialogueText.text = "";
        
        // 문장을 문자 배열로 변환하여 한 글자씩 처리
        foreach (char letter in sentence.ToCharArray())
        {
            // 현재 텍스트에 새 글자 추가
            dialogueText.text += letter;
            
            // 다음 글자까지 지정된 시간만큼 대기
            // yield return은 다음 프레임까지 실행을 일시 중단
            yield return new WaitForSeconds(typingSpeed);
        }
        
        // 타이핑 완료 상태로 변경
        isTyping = false;
        
        // 계속하기 표시기 활성화 (사용자가 다음으로 진행할 수 있음을 알림)
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }
    
    /// <summary>
    /// 현재 타이핑을 즉시 완성하는 메서드
    /// 사용자가 스페이스바를 눌러 타이핑을 건너뛸 때 호출
    /// </summary>
    private void CompleteCurrentTyping()
    {
        // 타이핑 코루틴 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 현재 문장 즉시 완성
        dialogueText.text = currentSentence;
        isTyping = false;

        // 계속하기 표시기 활성화
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }
    
    /// <summary>
    /// 캐릭터 초상화를 설정하는 메서드
    /// </summary>
    /// <param name="portrait">설정할 초상화 스프라이트</param>
    private void SetCharacterPortrait(Sprite portrait)
    {
        if (characterPortrait != null)
        {
            if (portrait != null)
            {
                // 초상화가 있으면 설정하고 활성화
                characterPortrait.sprite = portrait;
                characterPortrait.gameObject.SetActive(true);
            }
            else
            {
                // 초상화가 없으면 숨김
                characterPortrait.gameObject.SetActive(false);
            }
        }
    }
    #endregion
    
    #region 이벤트 메서드들 (확장 가능)
    /// <summary>
    /// 대화 시작 시 호출되는 이벤트 메서드
    /// 다른 시스템에서 대화 시작을 감지할 때 사용
    /// </summary>
    private void OnDialogueStart()
    {
        // 예: 게임 일시정지, BGM 변경, 캐릭터 이동 제한 등
        Debug.Log("대화가 시작되었습니다.");
        
        // 플레이어 이동 제한 (예시)
        // PlayerController.instance?.SetMovementEnabled(false);
    }
    
    /// <summary>
    /// 대화 종료 시 호출되는 이벤트 메서드
    /// </summary>
    private void OnDialogueEnd()
    {
        // 예: 게임 재개, 원래 BGM 복원, 캐릭터 이동 허용 등
        Debug.Log("대화가 종료되었습니다.");
        
        // 플레이어 이동 허용 (예시)
        // PlayerController.instance?.SetMovementEnabled(true);
    }
    #endregion
    
    #region 디버그 및 유틸리티 메서드들
    /// <summary>
    /// 현재 대화 시스템의 상태를 확인하는 메서드 (디버그용)
    /// </summary>
    /// <returns>대화 활성화 여부</returns>
    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    /// <summary>
    /// 남은 문장 수를 반환하는 메서드 (디버그용)
    /// </summary>
    /// <returns>큐에 남은 문장 수</returns>
    public int GetRemainingMessageCount()
    {
        return sentences.Count;
    }
    #endregion
    
        /// <summary>
    /// <summary>
    /// T 키 입력 처리 (필요시 구현)
    /// </summary>
    private void ToggleUIPanel()
    {
        Debug.Log("ToggleUIPanel 호출됨");
        // 필요한 UI 전환 로직 추가
    }
}