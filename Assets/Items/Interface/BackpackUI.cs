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
    /// <summary>
    /// Controls the backpack's display UI,
    /// using a grid system UI (<see cref="GridBackpack"/>).
    /// </summary>
    public class BackpackUI : MonoBehaviour
    {
        public struct ItemData
        {
            public GameObject UI;
            public StoredItem Item;

            public ItemData(GameObject ui, StoredItem item)
            {
                UI = ui;
                Item = item;
            }
        }

        // Public Propetries & Fields

        /// <summary>
        /// The time between (in seconds) that the backpack tabs updates.
        /// </summary>
        /// <remarks>
        /// Note: This doesn't affect the item grid, since they update based on events.
        /// </remarks>
        [Header("Backpack List")]
        public float BackpackTabUpdateRate;
        /// <summary>
        /// The UI that the <see cref="BackpackTab"/>'s are parented to.
        /// </summary>
        public GameObject BackpackDisplay;
        /// <summary>
        /// The button that is linked to a <see cref="GridBackpack"/>.
        /// </summary>
        public GameObject BackpackTab;

        /// <summary>
        /// The title text of element of the backpack ui.
        /// </summary>
        [Header("Grid")]
        public TextMeshProUGUI Title;
        /// <summary>
        /// The item element inside of the <see cref="ItemElement"/>.
        /// </summary>
        public GameObject ItemElement;
        /// <summary>
        /// The item grid that contains the items of a <see cref="GridBackpack"/>
        /// </summary>
        public GameObject GridDisplay;

        /// <summary>
        /// Displays the all actions that you could do with the selected item.
        /// </summary>
        [Header("Selection")]
        public GameObject InteractMenu;
        /// <summary>
        /// An interaction button where you can do things with buttons.
        /// </summary>
        public GameObject InteractAction;
        /// <summary>
        /// The distance between the <see cref="InteractMenu"/> and the mouse position for the menu to fade.
        /// </summary>
        public float InteractionMenuDisappearDistance = 250.0f;
        /// <summary>
        /// A list that handles different types of item's actions (see <see cref="ItemActionHandler.ItemAction"/>).
        /// </summary>
        public List<ItemActionHandler> ItemActionHandlers = new();
        /// <summary>
        /// Controls the transparency on the <see cref="InteractMenu"/> (see <see cref="InteractionMenuDisappearDistance"/>).
        /// </summary>
        public CanvasGroup InteractCanvas;

        // Private Propetries & Fields

        private PlayerBehaviour Player;
        private Canvas Canvas;
        private InputActionMap UIInputs;
        private InputActionMap GameplayInputs;
        private InputAction OpenAction;
        private InputAction ClickAction;
        private Camera Camera;
        /// <summary>
        /// The backpack selected.
        /// </summary>
        private GridBackpack CurrentBackpack;
        private ItemData? HoveredItem;

        /// <summary>
        /// Were is the interaction menu?
        /// </summary>
        private Vector2? LastInterMenuPos;
        /// <summary>
        /// The time has this script has been running.
        /// </summary>
        private float Timer = 0.0f;
        /// <summary>
        /// The last time has this UI updated (see <see cref="BackpackUpdateRate"/>).
        /// </summary>
        private float LastUpdate = float.MinValue;

        // Item Hovering

        private bool InsideOfGridDisplay(Vector2 worldPos)
        {
            RectTransform rect = GridDisplay.GetComponent<RectTransform>();

            bool inX = rect.anchoredPosition.x <= worldPos.x && worldPos.x < (rect.anchoredPosition.x + rect.sizeDelta.x),
            inY = rect.anchoredPosition.y <= worldPos.y && worldPos.y < (rect.anchoredPosition.y + rect.sizeDelta.y);

            return inX && inY;
        }

        private Vector2Int WorldPosToGridPos(Vector2 worldPos)
        {
            RectTransform rectTrans = GridDisplay.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTrans,
                worldPos,
                Camera,
                out var localPoint
            );

            Vector2 translation = GetGridTranslation(rectTrans),
            gridPos = localPoint / translation;

            return new Vector2Int(
                Mathf.FloorToInt(gridPos.x),
                Mathf.FloorToInt(gridPos.y)
            );
        }

        private void HoverItemFollowMouse()
        {
            if (!HoveredItem.HasValue) return;

            var hoveredItem = HoveredItem.Value;

            Vector2 mousePos = Mouse.current.position.value;

            var rectTrans = hoveredItem.UI.GetRectTransform();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTrans,
                mousePos,
                Camera,
                out var localPoint
            );

            rectTrans.anchoredPosition = localPoint;
        }

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

        private GameObject CreateInteractionTab(ItemActionHandler.ItemAction itemAction, StoredItem storedItem)
        {
            GameObject ui = Instantiate(InteractAction);
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
                DisableInteractionMenu();
            };

            return ui;
        }

        private void DisableInteractionMenu()
        {
            InteractMenu.SetActive(false);
            LastInterMenuPos = null;
        }

        private void SetInteractionMenuTransparency()
        {
            if (!LastInterMenuPos.HasValue) return;

            var pos = LastInterMenuPos.Value;
            var currentPos = Mouse.current.position.value;

            float dist = (pos - currentPos).magnitude;
            float alpha = 1 - (dist / InteractionMenuDisappearDistance);

            InteractCanvas.alpha = alpha;

            if (dist >= InteractionMenuDisappearDistance)
                DisableInteractionMenu();
        }

        // Grid

        private void OnItemLeftClick(ItemData selected)
        {
            HoveredItem = selected;
        }

        private void OnItemRightClick(PointerEventData data, StoredItem storedItem)
        {
            InteractMenu.SetActive(true);
            RectTransform rect = InteractMenu.GetComponent<RectTransform>();
            Vector2 screenPos = new(data.position.x, data.position.y);
            rect.position = screenPos;
            LastInterMenuPos = screenPos;

            for (int i = 0; i < InteractMenu.transform.childCount; i++)
            {
                var child = InteractMenu.transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var handler = GetItemActionHandler(storedItem.Item.GetType());
            foreach (var itemAction in handler.ItemActions)
            {
                GameObject itemActionUI = CreateInteractionTab(itemAction, storedItem);
                itemActionUI.transform.SetParent(rect);
                itemActionUI.transform.localScale = Vector3.one;
            }
        }

        private void OnItemClick(object sender, PointerEventData data, StoredItem storedItem)
        {
            if (sender is not ClickableObject clickable)
                throw new InvalidCastException($"Sender isn't a {typeof(ClickableObject).FullName}");

            switch (data.button)
            {
                case InputButton.Right:
                    OnItemRightClick(data, storedItem);
                    break;
                case InputButton.Left:
                    ItemData itemData = new(clickable.gameObject, storedItem);
                    OnItemLeftClick(itemData);
                    break;
            }
        }

        private readonly List<GameObject> ItemUIs = new();

        private GameObject CreateItemUI(StoredItem stored, Vector2 translation, RectTransform parentRect)
        {
            Item item = stored.Item;
            Vector2 pos = new(stored.Position.x, -stored.Position.y);

            GameObject ui = Instantiate(ItemElement);
            RectTransform rect = ui.GetComponent<RectTransform>();
            TextMeshProUGUI textUI = ui.GetComponentInChildren<TextMeshProUGUI>();
            ClickableObject clickable = ui.GetComponent<ClickableObject>();

            ui.transform.SetParent(parentRect);

            rect.sizeDelta = item.Size * translation;
            rect.anchoredPosition = pos * translation;
            rect.localScale = Vector3.one;
            textUI.text = $"{item.Name} ({stored.Amount})";
            ui.name = item.Name;

            clickable.OnClick += (sender, data) => { OnItemClick(sender, data, stored); };

            return ui;
        }

        private Vector2 GetGridTranslation(RectTransform rect)
        {
            return rect.rect.size / CurrentBackpack.Size;
        }

        private void RemoveAllItemUIs()
        {
            foreach (GameObject obj in ItemUIs)
                Destroy(obj);
            ItemUIs.Clear();
        }

        private void RefreshItemGrid(object sender, EventArgs eventArgs)
        {
            RemoveAllItemUIs();

            if (CurrentBackpack == null) return;

            RectTransform rect = GridDisplay.GetComponent<RectTransform>();
            Vector2 gridTranslation = GetGridTranslation(rect);

            DisableInteractionMenu();

            foreach (StoredItem strd in CurrentBackpack.StoredItems)
            {
                var itemUI = CreateItemUI(strd, gridTranslation, rect);
                ItemUIs.Add(itemUI);
            }
        }

        // Backpack List

        private readonly List<GameObject> BackpackUIs = new();

        private void RemoveAllBackpackTabs()
        {
            foreach (GameObject obj in BackpackUIs)
                Destroy(obj);
            BackpackUIs.Clear();
        }

        public void OnBackpackClick(GridBackpack backpack, PointerEventData eventData)
        {
            if (eventData.button != InputButton.Left)
                return;
            if (CurrentBackpack != null)
                CurrentBackpack.BackpackUpdated -= RefreshItemGrid;

            CurrentBackpack = backpack;

            RefreshItemGrid(backpack, EventArgs.Empty);
            CurrentBackpack.BackpackUpdated += RefreshItemGrid;
        }

        private GameObject CreateBackpackTab(GridBackpack backpack)
        {
            GameObject elem = Instantiate(BackpackTab);
            var text = elem.GetComponentInChildren<TextMeshProUGUI>();

            text.text = backpack.Name;
            elem.name = backpack.Name;

            var button = elem.GetComponent<ClickableObject>();
            button.OnClick += (sender, eventData) => { OnBackpackClick(backpack, eventData); };

            return elem;
        }

        private void RefreshBackpacks()
        {
            RemoveAllBackpackTabs();

            List<GridBackpack> backpacks = Player.DetectBackpacks();

            foreach (GridBackpack back in backpacks)
            {
                var backpackUI = CreateBackpackTab(back);

                BackpackUIs.Add(backpackUI);

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

        private void DragAndDropRelease(InputAction.CallbackContext _)
        {
            bool isHovering = HoveredItem.HasValue;

            if (!isHovering) return;

            ItemData hoveredItem = HoveredItem.Value;

            var rectTrans = hoveredItem.UI.GetRectTransform();

            bool isInside = InsideOfGridDisplay(rectTrans.anchoredPosition);

            //if (!isInside) return;

            Destroy(hoveredItem.UI);
            HoveredItem = null;

            Vector2Int pos = WorldPosToGridPos(rectTrans.anchoredPosition);

            print(pos);
            CurrentBackpack.MoveItem(hoveredItem.Item, pos);
        }

        void Start()
        {
            Canvas = GetComponentInParent<Canvas>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Player = playerObj.GetComponent<PlayerBehaviour>();
            Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            UIInputs = Player.Controls.FindActionMap("UI", true);
            ClickAction = UIInputs.FindAction("Click");

            GameplayInputs = Player.Controls.FindActionMap("Player", true);
            OpenAction = GameplayInputs.FindAction("Toggle Backpack");

            Canvas.enabled = false;
            OpenAction.performed += ToggleBackpack;

            ClickAction.performed += DragAndDropRelease;
        }

        void Update()
        {
            string titleName = CurrentBackpack == null ? "Backpack View" : CurrentBackpack.Name;
            Title.text = titleName;

            Timer += Time.deltaTime;
            if (Timer - LastUpdate >= BackpackTabUpdateRate)
            {
                LastUpdate = Timer;
                RefreshBackpacks();
            }

            SetInteractionMenuTransparency();
        }

        void LateUpdate()
        {
            HoverItemFollowMouse();
        }
    }
}