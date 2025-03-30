using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PickUpItem : MonoBehaviour
{
    [Tooltip("武器旋转的速度")] private float rotateSpeed;
    [Tooltip("武器编号")]public int itemID;
    [Tooltip("武器类型")] public Weapon_AutomaticGun.WeaponType weaponType;
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
        if (other.tag == "Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            weaponModel = GameObject.Find("Player/Assult_Rife_Arm/inventory").transform.GetChild(itemID).gameObject;
            
            var weaponScript = weaponModel.GetComponent<Weapon_AutomaticGun>();

            player.PickUpWeapon(itemID, weaponModel, weaponScript.weaponType);
            Destroy(gameObject);
        }
    }
}
