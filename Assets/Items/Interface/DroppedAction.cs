using CH.Items.Container;
using UnityEngine;


namespace CH.Items.Interface.ActionHandler
{
    using ActivateParams = ItemActionHandler.ActivateParams;

    [CreateAssetMenu(fileName = "DroppedAction", menuName = "Items/UI/Actions/Drop Action")]
    public class DroppedAction : ItemActionHandler.ItemAction
    {
        public override void Activate(ActivateParams activate)
        {
            var backpack = activate.Backpack;
            var storedItem = activate.StoredItem;
            var pos = activate.CommanderObject.transform.position;
            backpack.DropItem(storedItem, pos);
        }
    }
}