using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorCtl : MonoBehaviour
{

	private UnitObj parentUnit;

    // Start is called before the first frame update
    void Start()
    {
        parentUnit = transform.parent.GetComponent<UnitObj>();
    }

	//視覚検知
    void OnTriggerEnter2D(Collider2D other)
    {
    	if (parentUnit != null)
    	parentUnit.SsDiscover(other);
    }
    
    //視覚検知中
    void OnTriggerStay2D(Collider2D other)
    {
    	if (parentUnit != null)
    	parentUnit.SsNowdiscover(other);
    }
    
    //視覚検知外
    void OnTriggerExit2D(Collider2D other)
    {
    	if (parentUnit != null)
    	parentUnit.SsUndiscover(other);
    }
    
}
