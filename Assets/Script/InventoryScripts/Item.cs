using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/New Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemImage;

    // may need itemHeldInAnyOtherInventory....
    public int itemHeldInBag;
    [TextArea]
    public string itemInfo;
        
    
}
