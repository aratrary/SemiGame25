using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 필요합니다.

public class SkillCooldownManager : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;              // 스킬 이름 (관리용)
        public KeyCode key;                   // 스킬 단축키 (Q, W, E, R 등)
        public float cooldownTime = 5f;       // 스킬의 총 쿨타임 시간(초)

        [HideInInspector] public float currentCooldown = 0f; // 현재 남은 쿨타임 (Inspector에서 숨김)

        // 쿨타임을 시각화할 오버레이 Image 컴포넌트 (Filled 타입이어야 함)
        public Image skillCooldownOverlayImage; 
    }

    // 인스펙터에서 스킬 목록을 설정할 수 있도록 배열로 만듭니다.
    public Skill[] skills; 

    void Update()
    {
        // 각 스킬의 키 입력을 감지하고 쿨타임 진행 상태를 업데이트합니다.
        foreach (var skill in skills)
        {
            // 키를 눌렀고, 현재 쿨타임이 0이거나 스킬이 준비된 상태일 때만 스킬 사용 시도
            if (Input.GetKeyDown(skill.key) && skill.currentCooldown <= 0f)
            {
                UseSkill(skill);
            }

            // 쿨타임이 진행 중일 때만 업데이트
            if (skill.currentCooldown > 0f)
            {
                skill.currentCooldown -= Time.deltaTime; // 남은 쿨타임을 시간 흐름에 따라 감소

                // 쿨타임이 0보다 작아지면 0으로 고정 (음수 방지)
                if (skill.currentCooldown < 0f)
                {
                    skill.currentCooldown = 0f;
                }

                // 오버레이 이미지의 fillAmount를 조정하여 쿨타임을 시각화합니다.
                // 쿨타임이 시작(currentCooldown == cooldownTime)될 때 fillAmount는 1이 되고,
                // 쿨타임이 끝나면(currentCooldown == 0) fillAmount는 0이 되어 오버레이가 사라집니다.
                if (skill.skillCooldownOverlayImage != null)
                {
                    skill.skillCooldownOverlayImage.fillAmount = skill.currentCooldown / skill.cooldownTime;
                }
            }
            else // 쿨타임이 끝났을 때
            {
                // 오버레이를 완전히 사라지게 합니다 (fillAmount 0). 스킬 사용 가능 상태를 의미합니다.
                if (skill.skillCooldownOverlayImage != null)
                {
                    skill.skillCooldownOverlayImage.fillAmount = 0f;
                }
            }
        }
    }

    // 스킬 사용 로직 (쿨타임을 시작하고 필요하다면 다른 스킬 동작을 여기에 추가합니다)
    void UseSkill(Skill skill)
    {
        skill.currentCooldown = skill.cooldownTime; // 쿨타임 시작

        // 스킬 사용 직후 오버레이를 완전히 덮습니다 (fillAmount 1). 쿨타임 시작을 시각적으로 알림.
        if (skill.skillCooldownOverlayImage != null)
        {
            skill.skillCooldownOverlayImage.fillAmount = 1f;
        }

        Debug.Log($"{skill.skillName} 스킬 사용! 쿨타임: {skill.cooldownTime}초");
        // 여기에 스킬 발동 시 실제로 캐릭터가 행동하는 로직(애니메이션, 이펙트 등)을 추가할 수 있습니다.
    }
}