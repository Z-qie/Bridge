using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotInfo : MonoBehaviour
{
   
    public int slotID;

    public void SlotOnClick()
    {
        BagUIManager.UpdateItemDescription(slotID);
    }
   
}
