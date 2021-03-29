using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemInScene : MonoBehaviour
{
    public Item thisItem;
    public Inventory myBag;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")){
            AddItemIntoBag();
            BagUIManager.UpdateItemIntoBagPanel();
        }
    }

    private void AddItemIntoBag()
    {
        if (thisItem.itemHeldInBag != 0)
        {
            thisItem.itemHeldInBag++;
        }
        else
        {
            for (int i = 0; i < myBag.itemList.Count; i++)
            {
                if (myBag.itemList[i] == null)
                {
                    myBag.itemList[i] = thisItem;
                    thisItem.itemHeldInBag = 1;
                    break;
                }
            }
        }
    }
}
