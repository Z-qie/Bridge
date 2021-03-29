using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairController : MonoBehaviour
{
    public Transform triggerOn;
    public Transform triggerSwitch;

    public Vector2 triggerOnSize;
    public Vector2 triggerSwitchSize;

    public BoxCollider2D triggerSwitchCollider;
    public LayerMask whatIsPlayer;
    public PolygonCollider2D stair;

    // Start is called before the first frame update
    void Start()
    {
        stair.enabled = false;
    }

    void FixedUpdate()
    {
        triggerSwitchCollider.size = triggerSwitchSize;
        if (Physics2D.OverlapBox(triggerOn.position, triggerOnSize, 0, whatIsPlayer))
        {
            stair.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(triggerOn.position, (Vector3)triggerOnSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(triggerSwitch.position, (Vector3)triggerSwitchSize);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        stair.enabled = !stair.enabled;
    }
}
