using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zoneControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("enemies"))
        {
            towerCommon tower = transform.parent.GetComponent<towerCommon>();

            if (tower != null)
            {
                tower.enemyAdd(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("enemies"))
        {
            towerCommon tower = transform.parent.GetComponent<towerCommon>();

            if (tower != null)
            {
                tower.enemyRemove(other.gameObject.name);
            }
        }
    }
}
