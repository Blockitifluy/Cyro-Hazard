using UnityEngine;
using UnityEngine.EventSystems;
using CyroHazard.Items.Container;
using TMPro;
using System;
using System.Collections.Generic;

using InputButton = UnityEngine.EventSystems.PointerEventData.InputButton;

namespace CyroHazard.Items.Interface
{
    public partial class BackpackUI : MonoBehaviour
    {
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

        private void OnItemRightClick(PointerEventData data, StoredItem storedItem)
        {
            InteractMenu.SetActive(true);
            RectTransform rect = InteractMenu.GetRectTransform();
            Vector2 screenPos = new(data.position.x, data.position.y);
            rect.position = screenPos;
            LastInterMenuPos = screenPos;

            for (int i = 0; i < InteractMenu.transform.childCount; i++)
            {
                var child = InteractMenu.transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var itemActions = storedItem.Item.GetItemActions();
            foreach (var itemAction in itemActions)
            {
                GameObject itemActionUI = CreateInteractionTab(itemAction, storedItem);
                itemActionUI.transform.SetParent(rect);
                itemActionUI.transform.localScale = Vector3.one;
            }
        }

        private void OnItemClick(object sender, PointerEventData data, StoredItem storedItem, RectTransform rect)
        {
            if (sender is not ClickableObject clickable)
                throw new InvalidCastException($"Sender isn't a {typeof(ClickableObject).FullName}");

            switch (data.button)
            {
                case InputButton.Right:
                    OnItemRightClick(data, storedItem);
                    break;
                case InputButton.Left:
                    HoveredItem = new(clickable.gameObject, storedItem);
                    break;
            }
        }

        private readonly List<GameObject> ItemUIs = new();

        private GameObject CreateItemUI(StoredItem stored, Vector2 translation, RectTransform parentRect)
        {
            Item item = stored.Item;
            Vector2 pos = stored.Position;

            GameObject ui = Instantiate(ItemElement);
            RectTransform rect = ui.GetRectTransform();
            TextMeshProUGUI textUI = ui.GetComponentInChildren<TextMeshProUGUI>();
            ClickableObject clickable = ui.GetComponent<ClickableObject>();

            ui.transform.SetParent(parentRect);

            rect.sizeDelta = item.Size * translation;
            rect.anchoredPosition = pos * translation;
            rect.localScale = Vector3.one;
            textUI.text = $"{item.Name} ({stored.Amount})";
            ui.name = item.Name;

            clickable.OnClick += (sender, data) => { OnItemClick(sender, data, stored, rect); };

            return ui;
        }

        private Vector2 GetGridTranslation(RectTransform rect) => rect.rect.size / CurrentBackpack.Size;

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

            RectTransform rect = GridDisplay.GetRectTransform();
            Vector2 gridTranslation = GetGridTranslation(rect);

            DisableInteractionMenu();

            foreach (StoredItem strd in CurrentBackpack.StoredItems)
            {
                var itemUI = CreateItemUI(strd, gridTranslation, rect);
                ItemUIs.Add(itemUI);
            }
        }
    }
}