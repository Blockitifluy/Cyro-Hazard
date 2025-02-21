using UnityEngine;
using CH.Items.Container;
using CH.Character.Player;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

using InputButton = UnityEngine.EventSystems.PointerEventData.InputButton;
using CH.Items.Interface.ActionHandler;

namespace CH.Items.Interface
{
    internal struct BackpackUIData
    {
        public GridBackpack Storage;
        public GameObject UI;

        public BackpackUIData(GridBackpack backpack, GameObject backpackUI)
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
        public Vector3 GridItemScale = Vector3.one;

        [Header("Selection")]
        public GameObject ItemInteractMenu;
        public GameObject ItemAction;
        public float InteractionMenuDisappearDistance;
        public List<ItemActionHandler> ItemActionHandlers = new();
        public CanvasGroup InteractCanvas;

        // Private Propetries & Fields

        private PlayerBehaviour Player;
        private Canvas Canvas;
        private InputActionMap UIInputs;
        private InputAction OpenAction;
        private GridBackpack CurrentBackpack;

        private Vector2? LastPos;
        private float Timer = 0.0f;
        private float LastUpdate = float.MinValue;

        // Selection

        private ItemActionHandler GetItemActionHandler(Type ItemType)
        {
            ItemActionHandler res = null;

            foreach (ItemActionHandler handler in ItemActionHandlers)
            {
                if (ItemType != handler.GetTargetType())
                    continue;
                res = handler;
            }

            return res;
        }

        private GameObject CreateItemActionUI(ItemActionHandler.ItemAction itemAction, StoredItem storedItem)
        {
            GameObject ui = Instantiate(ItemAction);
            var textMesh = ui.GetComponent<TextMeshProUGUI>();
            textMesh.text = itemAction.Name;

            var clickable = ui.GetComponent<ClickableObject>();
            clickable.OnClick += (sender, data) =>
            {
                ItemActionHandler.ActivateParams activateParams = new()
                {
                    CommanderObject = Player.gameObject,
                    StoredItem = storedItem,
                    Backpack = CurrentBackpack
                };

                itemAction.Activate(activateParams);
            };

            return ui;
        }

        private void OnItemRightClick(PointerEventData data, StoredItem storedItem)
        {
            ItemInteractMenu.SetActive(true);
            RectTransform rect = ItemInteractMenu.GetComponent<RectTransform>();
            Vector2 screenPos = new(data.position.x, data.position.y);
            rect.position = screenPos;
            LastPos = screenPos;

            for (int i = 0; i < ItemInteractMenu.transform.childCount; i++)
            {
                var child = ItemInteractMenu.transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var handler = GetItemActionHandler(storedItem.Item.GetType());
            foreach (var itemAction in handler.ItemActions)
            {
                GameObject itemActionUI = CreateItemActionUI(itemAction, storedItem);
                itemActionUI.transform.SetParent(rect);
                itemActionUI.transform.localScale = Vector3.one;
            }
        }

        private void OnItemClick(object sender, PointerEventData data, StoredItem storedItem)
        {
            if (sender is not ClickableObject)
                throw new InvalidCastException($"Sender isn't a {typeof(ClickableObject).FullName}");

            switch (data.button)
            {
                case InputButton.Right:
                    OnItemRightClick(data, storedItem);
                    break;
            }
        }

        private void DisableInteractionMenu()
        {
            ItemInteractMenu.SetActive(false);
        }

        // Grid

        private readonly List<ItemUIData> ItemDatas = new();

        private GameObject CreateItem(StoredItem stored, Vector2 translation, RectTransform parentRect)
        {
            Item item = stored.Item;
            Vector2 pos = new(stored.Position.x, -stored.Position.y);

            GameObject ui = Instantiate(GridItem);
            RectTransform rect = ui.GetComponent<RectTransform>();
            TextMeshProUGUI textUI = ui.GetComponentInChildren<TextMeshProUGUI>();
            ClickableObject clickable = ui.GetComponent<ClickableObject>();

            ui.transform.SetParent(parentRect);

            rect.sizeDelta = item.Size * translation;
            rect.anchoredPosition = pos * translation;
            rect.localScale = GridItemScale;
            textUI.text = $"{item.Name} ({stored.Amount})";
            ui.name = item.Name;

            clickable.OnClick += (sender, data) => { OnItemClick(sender, data, stored); };

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

        private void RefreshGrid(object sender, EventArgs eventArgs)
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

        public void OnButtonClick(GridBackpack backpack, PointerEventData eventData)
        {
            if (eventData.button != InputButton.Left)
                return;
            if (CurrentBackpack != null)
                CurrentBackpack.BackpackUpdated -= RefreshGrid;

            CurrentBackpack = backpack;

            RefreshGrid(backpack, EventArgs.Empty);
            CurrentBackpack.BackpackUpdated += RefreshGrid;
        }

        private GameObject CreateBackpack(GridBackpack backpack)
        {
            GameObject elem = Instantiate(BackpackElem);
            var text = elem.GetComponentInChildren<TextMeshProUGUI>();

            text.text = backpack.Name;
            elem.name = backpack.Name;

            var button = elem.GetComponent<ClickableObject>();
            button.OnClick += (sender, eventData) => { OnButtonClick(backpack, eventData); };

            return elem;
        }

        private void RefreshBackpacks()
        {
            RemoveAllBackpackUIs();

            List<GridBackpack> backpacks = Player.DetectBackpacks();

            foreach (GridBackpack back in backpacks)
            {
                var backpackUI = CreateBackpack(back);

                BackpackUIData data = new(back, backpackUI);
                BackpackDatas.Add(data);

                backpackUI.transform.SetParent(BackpackDisplay.transform);
                backpackUI.transform.localScale = Vector3.one;
            }
        }

        private void ToggleBackpack(InputAction.CallbackContext _)
        {
            bool enable = !Canvas.enabled;
            Canvas.enabled = enable;
            Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = enable;
            Player.ControlCamera = !enable;
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
            string titleName = CurrentBackpack == null ? "Backpack View" : CurrentBackpack.Name;
            Title.text = titleName;

            Timer += Time.deltaTime;
            if (Timer - LastUpdate >= BackpackUpdateRate)
            {
                LastUpdate = Timer;
                RefreshBackpacks();
            }

            if (LastPos.HasValue)
            {
                var pos = LastPos.Value;
                var currentPos = Mouse.current.position.value;

                float dist = (pos - currentPos).magnitude;
                float alpha = 1 - (dist / InteractionMenuDisappearDistance);

                InteractCanvas.alpha = alpha;

                if (dist >= InteractionMenuDisappearDistance)
                    DisableInteractionMenu();
            }
        }
    }
}