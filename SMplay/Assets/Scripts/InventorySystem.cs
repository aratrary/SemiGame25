using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용
using System.Collections.Generic; // List 사용
using TMPro; // TextMeshPro를 사용하는 경우 추가 (기본 Text 사용 시 생략)

public class InventoryManager : MonoBehaviour
{
    // 인스펙터에서 할당할 UI 요소들
    public GameObject inventoryGridParent; // GridLayoutGroup이 붙어있는 GameObject (InventoryGrid)
    public GameObject inventorySlotPrefab; // 인벤토리 슬롯 Prefab

    // 인벤토리 데이터를 저장할 리스트 (얻은 순서대로 저장됨)
    private List<InventoryItem> items = new List<InventoryItem>();

    // UI에 생성된 슬롯 오브젝트들을 관리할 리스트
    private List<GameObject> currentUISlots = new List<GameObject>();

    // ------------ Item 데이터 구조 정의 ------------
    // 아이템의 정보를 담을 클래스 (필요에 따라 더 많은 속성 추가 가능)
    [System.Serializable] // 인스펙터에서 테스트용으로 아이템을 볼 수 있게 함
    public class InventoryItem
    {
        public string itemName;      // 아이템 이름
        public Sprite itemIcon;      // 아이템 아이콘 스프라이트
        public int quantity;         // 아이템 수량

        // 생성자
        public InventoryItem(string name, Sprite icon, int qty)
        {
            itemName = name;
            itemIcon = icon;
            quantity = qty;
        }
    }
    // ---------------------------------------------


    // ------------ 테스트용 아이템 및 추가 로직 ------------
    // 인스펙터에서 테스트용 아이템 아이콘을 할당해주세요.
    public Sprite testItemIcon1;
    public Sprite testItemIcon2;
    public Sprite testItemIcon3;

    void Start()
    {
        // 게임 시작 시 테스트용 아이템 몇 개 추가
        AddItem("철", testItemIcon1, 1);
        AddItem("여고 리모콘", testItemIcon2, 1);
        AddItem("앤디안경", testItemIcon3, 1);
        AddItem("철", testItemIcon1, 1); // 같은 아이템 추가 시 수량 증가 로직 (AddItem 함수에 포함)
    }

    void Update()
{
    if (Input.GetKeyDown(KeyCode.A))
    {
        // 미리 만들어둔 테스트용 아이템 이름과 아이콘을 배열로 선언
        string[] testItemNames = { "철", "여고 리모콘", "앤디안경" };
        Sprite[] testItemIcons = { testItemIcon1, testItemIcon2, testItemIcon3 };

        // 배열에서 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, testItemNames.Length);

            // 선택된 아이템으로 1개 추가
        AddItem(testItemNames[randomIndex], testItemIcons[randomIndex], 1);

        Debug.Log("A 키를 눌러 아이템 추가: " + testItemNames[randomIndex]);
    }
}

    // 랜덤 테스트 아이콘을 가져오는 헬퍼 함수
    private Sprite GetRandomTestIcon()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0) return testItemIcon1;
        if (rand == 1) return testItemIcon2;
        return testItemIcon3;
    }
    // ---------------------------------------------


    // ------------ 핵심 인벤토리 관리 함수들 ------------

    // 인벤토리에 아이템을 추가하는 함수
    public void AddItem(string name, Sprite icon, int quantityToAdd)
    {
        // 동일한 아이템이 있는지 찾기
        InventoryItem existingItem = items.Find(item => item.itemName == name);

        if (existingItem != null)
        {
            // 이미 있으면 수량만 증가
            existingItem.quantity += quantityToAdd;
        }
        else
        {
            // 없으면 새로 추가
            items.Add(new InventoryItem(name, icon, quantityToAdd));
        }

        // UI 업데이트
        UpdateInventoryUI();
    }

    void UpdateInventoryUI()
    {
        // 기존 슬롯 모두 삭제
        foreach (GameObject slot in currentUISlots)
        {
            Destroy(slot);
        }
        currentUISlots.Clear();

        // 아이템 리스트 기준으로 슬롯 생성 (중복 없음)
        foreach (InventoryItem item in items)
        {
            GameObject newSlot = Instantiate(inventorySlotPrefab, inventoryGridParent.transform);
            currentUISlots.Add(newSlot);

            Image itemIconImage = newSlot.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI quantityText = newSlot.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.itemIcon;
                itemIconImage.gameObject.SetActive(item.itemIcon != null);
            }

            if (quantityText != null)
            {
                quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
            }
        }
    }
}