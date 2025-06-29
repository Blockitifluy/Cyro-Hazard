using System.Collections.Generic;
using CyroHazard.Items.Container;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using TMPro;

using InputButton = UnityEngine.EventSystems.PointerEventData.InputButton;

namespace CyroHazard.Items.Interface
{
    public partial class BackpackUI : MonoBehaviour
    {
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

            var backpacks = Player.DetectBackpacks();

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
    }
}