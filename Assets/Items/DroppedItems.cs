using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItems : MonoBehaviour
{
    private ItemManager _ItemsManager;
    private ItemManager.Item _Item;
    private float _Health;

    public ItemManager.Item Item
    {
        get { return _Item; }
    }

    public float Health
    {
        get { return _Health; }
        set { _Health = Mathf.Clamp(value, 0, Item.MaxHealth); }
    }

    public string ItemID;

    // Start is called before the first frame update
    public void Start()
    {
        _ItemsManager = ItemManager.GetManager();
        _Item = _ItemsManager.GetItem(ItemID);
    }

    // Update is called once per frame
    public void Update()
    {

    }
}
