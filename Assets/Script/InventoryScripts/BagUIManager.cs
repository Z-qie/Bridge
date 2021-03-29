using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagUIManager : MonoBehaviour
{
    public static BagUIManager bagUIManagerSingleton;
    public Inventory myBag;
    public GameObject myBagPanel;
    public GameObject grids;
    public GameObject itemDescription;
    public GameObject slotPrefab;

    public bool isBagOpen;

    private void Awake()
    {
        if (bagUIManagerSingleton != null)
            Destroy(this);
        bagUIManagerSingleton = this;

        for (int i = 0; i < bagUIManagerSingleton.myBag.itemList.Count; i++)
        {
            GameObject slot = Instantiate(bagUIManagerSingleton.slotPrefab, bagUIManagerSingleton.grids.transform);
            slot.GetComponent<SlotInfo>().slotID = i;
        }
    }

    void Update()
    {
        OpenCloseBagByKey();
    }

    void OpenCloseBagByKey()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isBagOpen)
            {
                myBagPanel.SetActive(false);
                itemDescription.SetActive(false);
                isBagOpen = false;
            }
            else
            {
                UpdateItemIntoBagPanel();
                myBagPanel.SetActive(true);
                isBagOpen = true;
            }
        }
    }

    public void CloseBagByClick()
    {
        myBagPanel.SetActive(false);
        itemDescription.SetActive(false);
        isBagOpen = false;
    }


    public static void UpdateItemIntoBagPanel()
    {
       
        for (int i = 0; i < bagUIManagerSingleton.grids.transform.childCount; i++)
        {
            if (bagUIManagerSingleton.myBag.itemList[i] != null)
            {
                //set item image in bag slot
                bagUIManagerSingleton.grids.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().sprite = bagUIManagerSingleton.myBag.itemList[i].itemImage;
                //set item amount in bag slot
                bagUIManagerSingleton.grids.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = bagUIManagerSingleton.myBag.itemList[i].itemHeldInBag.ToString();
                bagUIManagerSingleton.grids.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            }
            else
            {
                bagUIManagerSingleton.grids.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
                bagUIManagerSingleton.grids.transform.GetChild(i).GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = null;
            }
        }
    }


    public static void UpdateItemDescription(int slotID)
    {
        Item thisItem = bagUIManagerSingleton.myBag.itemList[slotID];

        if (thisItem != null)
        {
            bagUIManagerSingleton.itemDescription.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = thisItem.itemImage;
            bagUIManagerSingleton.itemDescription.transform.GetChild(1).gameObject.GetComponent<Text>().text = thisItem.itemName;
            bagUIManagerSingleton.itemDescription.transform.GetChild(2).gameObject.GetComponent<Text>().text = thisItem.itemInfo;
            bagUIManagerSingleton.itemDescription.SetActive(true);
        }
        else
        {
            bagUIManagerSingleton.itemDescription.SetActive(false);
        }
    }
}
