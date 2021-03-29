using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemOnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Inventory myBag;

    private Transform originalParent;
    private int originalSlotID;


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlotID = transform.parent.gameObject.GetComponent<SlotInfo>().slotID;
        transform.SetParent(transform.parent.parent);
        transform.position = eventData.position;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // UI之外
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }
        else if (eventData.pointerCurrentRaycast.gameObject.name == "Item")
        {
            transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform.parent);
            transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;

            eventData.pointerCurrentRaycast.gameObject.transform.SetParent(originalParent);
            eventData.pointerCurrentRaycast.gameObject.transform.position = originalParent.position;
            Swap();
        }
        //else if (eventData.pointerCurrentRaycast.gameObject.name == "Slot(Clone)")
        //{
        //    transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform);
        //    transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;

        //    eventData.pointerCurrentRaycast.gameObject.transform.GetChild(0).SetParent(originalParent);
        //    eventData.pointerCurrentRaycast.gameObject.transform.GetChild(0).position = originalParent.position;

        //    Swap();
        //}
        // UI之内
        else
        {
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }

        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    void Swap()
    {
        var temp = myBag.itemList[originalSlotID];
        myBag.itemList[originalSlotID] = myBag.itemList[transform.parent.gameObject.GetComponent<SlotInfo>().slotID];
        myBag.itemList[transform.parent.gameObject.GetComponent<SlotInfo>().slotID] = temp;
    }
}
