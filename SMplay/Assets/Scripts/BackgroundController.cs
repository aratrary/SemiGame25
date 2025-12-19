using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    // UI 배경(Image) 또는 월드 배경(SpriteRenderer) 중 원하는 컴포넌트를 연결하세요.
    public Image uiBackground;
    public SpriteRenderer worldBackground;

    // 파일명으로 런타임 로드를 원할 경우 사용 (Resources 폴더 필요)
    public bool useResources = false;
    public string resourcesFolder = "Images"; // 예: Assets/Resources/Images/...

    [System.Serializable]
    public class SignalSprite
    {
        public string signal;   // 예: "문이잠겼어", "문을부쉈어", "선생님배경", "레이저건발사", "선생님기절", "앤디펀치", "EMP이후"
        public Sprite sprite;   // 해당 신호에 매핑할 스프라이트
    }

    // 인스펙터에서 신호와 스프라이트를 매핑하세요.
    public List<SignalSprite> mappings = new List<SignalSprite>();

    // 신호로 배경 변경 (대소문자/문자 동일 비교)
    public void SetBySignal(string signal)
    {
        Sprite s = FindMappedSprite(signal);
        if (s == null && useResources)
        {
            s = LoadSpriteByName(signal);
        }
        ApplySprite(s);
    }

    // 파일명으로 배경 변경 (매핑 또는 Resources 로드)
    public void SetByFileName(string fileName)
    {
        Sprite s = FindMappedSprite(fileName);
        if (s == null && useResources)
        {
            s = LoadSpriteByName(fileName);
        }
        ApplySprite(s);
    }

    // 빠른 호출 헬퍼 (요청한 3개 신호)
    public void LockDoor() => SetBySignal("문이잠겼어");
    public void BreakDoor() => SetBySignal("문을부쉈어");
    public void TeacherBackground() => SetBySignal("선생님배경");
    public void FireLaserGun() => SetBySignal("레이저건발사");
    public void TeacherFainted() => SetBySignal("선생님기절");
    public void AndyPunch() => SetBySignal("앤디펀치");
    public void AfterEMP() => SetBySignal("EMP이후");

    // --- 내부 유틸리티 ---
    private Sprite FindMappedSprite(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        for (int i = 0; i < mappings.Count; i++)
        {
            var m = mappings[i];
            if (m != null && m.sprite != null && string.Equals(m.signal, key, System.StringComparison.Ordinal))
            {
                return m.sprite;
            }
        }
        return null;
    }

    private Sprite LoadSpriteByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string folder = string.IsNullOrEmpty(resourcesFolder) ? string.Empty : resourcesFolder.TrimEnd('/') + "/";
        return Resources.Load<Sprite>(folder + name);
    }

    private void ApplySprite(Sprite s)
    {
        if (uiBackground != null)
        {
            uiBackground.sprite = s;
            uiBackground.enabled = s != null;
        }
        if (worldBackground != null)
        {
            worldBackground.sprite = s;
            worldBackground.enabled = s != null;
        }
    }
}
