using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PickUpItem : MonoBehaviour
{
    [Tooltip("ÎäÆ÷Ðý×ªµÄËÙ¶È")] private float rotateSpeed;
    [Tooltip("ÎäÆ÷±àºÅ")]public int itemID;
    private GameObject weaponModel;
    // Start is called before the first frame update
    void Start()
    {
        rotateSpeed = 100;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag =="Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            //
            weaponModel = GameObject.Find("Player/Assult_Rife_Arm/inventory").gameObject.transform.GetChild(itemID).gameObject;
            //Debug.Log(weaponModel.name);
            player.PickUpWeapon(itemID,weaponModel);
            Destroy(gameObject);
        }
    }
}
