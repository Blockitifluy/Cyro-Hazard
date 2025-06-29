using UnityEngine;
using CyroHazard.Items.Container;
using CyroHazard.Character.Player;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

using InputButton = UnityEngine.EventSystems.PointerEventData.InputButton;

namespace CyroHazard.Items.Interface
{
    [AddComponentMenu("Items/BackpackUI")]
    /// <summary>
    /// Controls the backpack's display UI,
    /// using a grid system UI (<see cref="GridBackpack"/>).
    /// </summary>
    public partial class BackpackUI : MonoBehaviour
    {
        private struct ItemData
        {
            public GameObject UI;
            public StoredItem Item;

            internal ItemData(GameObject ui, StoredItem item)
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

        private Vector2Int WorldPosToGridPos(Vector2 worldPos)
        {
            RectTransform gridTrans = GridDisplay.GetRectTransform();

            Vector2 translation = GetGridTranslation(gridTrans);

            Vector2Int gridPos = (worldPos / translation).Round()
            .Min(CurrentBackpack.Size - Vector2Int.one)
            .Max(Vector2Int.zero);

            return gridPos;
        }

        private void DragAndDrop()
        {
            if (!HoveredItem.HasValue) return;

            var hoveredItem = HoveredItem.Value;

            Vector2 mousePos = Mouse.current.position.value;

            var rectTrans = hoveredItem.UI.GetRectTransform();
            var gridRect = GridDisplay.GetRectTransform();

            Vector2 gridTrans = GetGridTranslation(gridRect),
            globalPos = new(gridRect.position.x, gridRect.position.y);

            rectTrans.position = ((mousePos - globalPos) / gridTrans).Round() * gridTrans + globalPos;
        }

        // Selection

        private GameObject CreateInteractionTab(Item.ActionMenuItem itemAction, StoredItem storedItem)
        {
            GameObject ui = Instantiate(InteractAction);
            var textMesh = ui.GetComponent<TextMeshProUGUI>();
            textMesh.text = itemAction.Name;

            var clickable = ui.GetComponent<ClickableObject>();
            clickable.OnClick += (sender, data) =>
            {
                Item.ItemActionParams activateParams = new()
                {
                    CallerObject = Player.gameObject,
                    StoredItem = storedItem,
                    Backpack = CurrentBackpack
                };

                itemAction.ItemAction(activateParams);
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

        // Unity Functions

        private void DragAndDropRelease(InputAction.CallbackContext _)
        {
            bool isHovering = HoveredItem.HasValue;

            if (!isHovering) return;

            ItemData hoveredItem = HoveredItem.Value;

            var rectTrans = hoveredItem.UI.GetRectTransform();

            StoredItem storedItem = hoveredItem.Item;
            Vector2Int pos = WorldPosToGridPos(rectTrans.anchoredPosition);

            try
            {
                CurrentBackpack.MoveItem(hoveredItem.Item, pos);
            }
            catch (GridBackpack.ModifingException)
            {
                var occupany = CurrentBackpack.GetOccupancy();
                Debug.Log($"{storedItem} couldn't be moved to {pos} (OccupanyID: {occupany})");
            }
            finally
            {
                Destroy(hoveredItem.UI);
                HoveredItem = null;
                RefreshItemGrid(this, EventArgs.Empty);
            }

        }

        void Start()
        {
            Canvas = GetComponentInParent<Canvas>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Player = playerObj.GetComponent<PlayerBehaviour>();

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
            DragAndDrop();
        }
    }
}