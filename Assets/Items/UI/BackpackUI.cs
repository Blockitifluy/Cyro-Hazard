using UnityEngine;
using CH.Items;
using CH.Items.Container;
using CH.Character.Player;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEditor.Events;

// TODO - Clean this messy code I have released on to github
// TODO - Add Documentation

namespace CH.Items.UI
{
    internal struct BackpackUIData
    {
        public Backpack Storage;
        public GameObject UI;

        public BackpackUIData(Backpack backpack, GameObject backpackUI)
        {
            Storage = backpack;
            UI = backpackUI;
        }
    }

    internal struct ItemUIData
    {
        public StoredItem Item;
        public GameObject UI;

        public ItemUIData(StoredItem item, GameObject itemUI)
        {
            Item = item;
            UI = itemUI;
        }
    }

    public class BackpackUI : MonoBehaviour
    {
        // Public Propetries & Fields

        public float BackpackUpdateRate;

        [Header("Backpack List")]
        public GameObject BackpackDisplay;
        public GameObject BackpackElem;

        [Header("Grid")]
        public TextMeshProUGUI Title;
        public GameObject GridItem;
        public GameObject Grid;

        // Private Propetries & Fields

        private PlayerBehaviour Player;
        private Canvas Canvas;
        private InputActionMap UIInputs;
        private InputAction OpenAction;

        private Backpack CurrentBackpack;
        private float Timer = 0.0f;
        private float LastUpdate = 0.0f;

        // Grid

        private readonly List<ItemUIData> ItemDatas = new();

        private GameObject CreateItem(StoredItem stored, Vector2 translation, RectTransform parentRect)
        {
            ItemManager.Item item = stored.Item;

            GameObject ui = Instantiate(GridItem);
            RectTransform rect = ui.GetComponent<RectTransform>();
            TextMeshProUGUI textUI = ui.GetComponentInChildren<TextMeshProUGUI>();

            ui.transform.SetParent(parentRect);
            print(stored.Position * translation);

            rect.sizeDelta = item.Size * translation;
            rect.anchoredPosition = stored.Position * translation;
            rect.localScale = Vector3.one;
            textUI.text = $"{item.Name} ({stored.Amount})";
            ui.name = item.Name;

            return ui;
        }

        private void RemoveAllItemUIs()
        {
            foreach (ItemUIData data in ItemDatas)
            {
                Destroy(data.UI);
            }

            ItemDatas.Clear();
        }

        private void RefreshGrid() // TODO - Connect to event
        {
            RemoveAllItemUIs();

            if (CurrentBackpack == null) return;

            RectTransform rect = Grid.GetComponent<RectTransform>();
            Vector2 gridTranslation = rect.rect.size / CurrentBackpack.Size;

            foreach (StoredItem strd in CurrentBackpack.StoredItems)
            {
                var itemUI = CreateItem(strd, gridTranslation, rect);

                ItemUIData data = new(strd, itemUI);
                ItemDatas.Add(data);
            }
        }

        // Backpack List

        private readonly List<BackpackUIData> BackpackDatas = new();

        private void RemoveAllBackpackUIs()
        {
            foreach (BackpackUIData back in BackpackDatas)
            {
                Destroy(back.UI);
            }

            BackpackDatas.Clear();
        }

        public void OnButtonClick(Backpack backpack)
        {
            print("Hello World!");
            CurrentBackpack = backpack;
        }

        private GameObject CreateBackpack(Backpack backpack)
        {
            GameObject elem = Instantiate(BackpackElem);
            var text = elem.GetComponentInChildren<TextMeshProUGUI>();

            text.text = backpack.Name;
            elem.name = backpack.Name;

            var button = elem.GetComponent<Button>();
            UnityEventTools.AddObjectPersistentListener(button.onClick, OnButtonClick, backpack);

            return elem;
        }

        private void RefreshBackpacks()
        {
            RemoveAllBackpackUIs();

            List<Backpack> backpacks = Player.DetectBackpacks();

            foreach (Backpack back in backpacks)
            {
                var backpackUI = CreateBackpack(back);

                BackpackUIData data = new(back, backpackUI);
                BackpackDatas.Add(data);

                backpackUI.transform.SetParent(BackpackDisplay.transform);
            }
        }

        private void ToggleBackpack(InputAction.CallbackContext _)
        {
            bool enable = !Canvas.enabled;
            Canvas.enabled = enable;
            Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = enable;

            if (!enable) CurrentBackpack = null;
        }

        // Unity Functions

        void Start()
        {
            Canvas = GetComponentInParent<Canvas>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Player = playerObj.GetComponent<PlayerBehaviour>();

            UIInputs = Player.Controls.FindActionMap("ui", true);
            OpenAction = UIInputs.FindAction("open-backpack");

            Canvas.enabled = false;
            OpenAction.performed += ToggleBackpack;
        }

        void Update()
        {
            string titleName = CurrentBackpack == null ? "No Backpack Selected" : CurrentBackpack.Name;
            Title.text = titleName;

            Timer += Time.deltaTime;
            if (Timer - LastUpdate >= BackpackUpdateRate)
            {
                LastUpdate = Timer;
                RefreshBackpacks();
                RefreshGrid();
            }
        }
    }
}