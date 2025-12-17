using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Button에 붙여서 DialogueSystem으로 대화 진행(다음 문장/타이핑 스킵)을 보장하는 헬퍼
/// 수동 OnClick 연결이 잘못되었을 때도 자동으로 연결합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class DialogueAdvanceButton : MonoBehaviour
{
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private bool requireDialogueActive = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
        }

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueAdvanceButton: DialogueSystem을 찾을 수 없습니다.");
            return;
        }

        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueAdvanceButton: DialogueSystem 참조가 없습니다.");
            return;
        }

        if (requireDialogueActive && !dialogueSystem.IsDialogueActive())
        {
            Debug.LogWarning("DialogueAdvanceButton: 대화가 활성화되어 있지 않아 클릭을 무시합니다.");
            return;
        }

        Debug.Log("DialogueAdvanceButton: 버튼 클릭 → 대화 진행 시도");
        dialogueSystem.AdvanceDialogueViaButton();
    }
}
