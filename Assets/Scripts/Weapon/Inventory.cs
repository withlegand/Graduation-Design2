using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //武器库
    //public List<GameObject> weapons = new List<GameObject>();
    // 替换原有武器库为三个分类库
    public List<GameObject> primaryWeapons = new List<GameObject>();
    public List<GameObject> secondaryWeapons = new List<GameObject>();
    public List<GameObject> meleeWeapons = new List<GameObject>();
    //当前武器编号
    //public int currentWeaponID  ;
    // 当前装备的武器
    private GameObject currentPrimary;
    private GameObject currentSecondary;
    private GameObject currentMelee;

    // 当前激活的武器类型
    public enum ActiveWeaponType { None, Primary, Secondary, Melee }
    private ActiveWeaponType currentActiveType = ActiveWeaponType.None;

    // Start is called before the first frame update
    void Start()
    {
        //currentWeaponID = -1;
    }

    // Update is called once per frame
    void Update()
    {
        //ChangeCurrentWeaponID();
        // 数字键切换武器类型
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(ActiveWeaponType.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(ActiveWeaponType.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(ActiveWeaponType.Melee);
    }

    //更新武器编号
    //public void ChangeCurrentWeaponID()
    //{
    //    //-0.1,0,0.1
    //    if(Input.GetAxis("Mouse ScrollWheel") < 0)
    //    {
    //        //下一把武器
    //        ChangeWeapon(currentWeaponID+1);
    //    }
    //    else if(Input.GetAxis("Mouse ScrollWheel") > 0)
    //    {
    //        //上一把武器
    //        ChangeWeapon(currentWeaponID - 1);
    //    }

    //    //通过数字键盘切换武器
    //    for (int i = 0; i < 10; i++)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Alpha0 + i))
    //        {
    //            int num = 0;
    //            if (i==10)
    //            {
    //                num = 10;
    //            }
    //            else
    //            {
    //                num = i - 1;
    //            }
    //            /*只有数字小于武器列表个数的时候才能进一步处理
    //             如果有3把武器，但按下4-9，那么是无意义的，不处理*/
    //            if (num < weapons.Count)
    //            {
    //                ChangeWeapon(num);
    //            }

    //        }
    //    }
    //}

    ////武器切换
    //public void ChangeWeapon(int weaponID)
    //{
    //    if (weapons.Count == 0) return;

    //    /*如果切换到最大索引编号的枪的时候，就调出第一把枪
    //      如果切换到最小索引编号的枪的时候，就调出最后一把枪*/

    //    //Indexof:获取列表中元素首次出现的索引
    //    //MAX list取最大元素
    //    if (weaponID > weapons.Max(weapons.IndexOf))
    //    {
    //        weaponID = weapons.Min(weapons.IndexOf);
    //    }
    //    else if(weaponID < weapons.Min(weapons.IndexOf))
    //    {
    //        weaponID = weapons.Max(weapons.IndexOf);
    //    }

    //    if (weaponID == currentWeaponID)
    //    {
    //        //只有一种武器的时候不进行切换
    //        return;
    //    }

    //    currentWeaponID = weaponID;//更新武器索引
    //    //根据武器编号，显示出对应的武器
    //    for (int i = 0; i < weapons.Count; i++)
    //    {
    //        if (weaponID == i)
    //        {
    //            weapons[i].gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            weapons[i].gameObject.SetActive(false);
    //        }
    //    }
    //}

    ////捡拾武器
    //public void AddWeapon(GameObject weapon)
    //{
    //    if (weapons.Contains(weapon))
    //    {
    //        print("集合里已存在此枪械");
    //        return;
    //    }
    //    else
    //    {
    //        //if(weapons.Count <3)
    //        //{

    //            weapons.Add(weapon);
    //            ChangeWeapon(currentWeaponID + 1);//显示武器
    //            weapon.gameObject.SetActive(true);
    //        //}
    //    }
    //}

    ////丢弃武器
    //public void ThrowWeapon(GameObject weapon)
    //{
    //    if (!weapons.Contains(weapon) || weapons.Count == 0)
    //    {
    //        print("没有这个武器，无法抛弃");
    //        return;
    //    }
    //    else
    //    {
    //        weapons.Remove(weapon);//删除对应的武器
    //        ChangeWeapon(currentWeaponID - 1);
    //        weapon.gameObject.SetActive(false);

    //    }
    //}

    public void AddWeapon(GameObject weapon, Weapon_AutomaticGun.WeaponType type)
    {
        var weaponScript = weapon.GetComponent<Weapon_AutomaticGun>();

        switch (type)
        {
            case Weapon_AutomaticGun.WeaponType.Primary:
                HandleWeaponAddition(ref currentPrimary, weapon, primaryWeapons);
                break;
            case Weapon_AutomaticGun.WeaponType.Secondary:
                HandleWeaponAddition(ref currentSecondary, weapon, secondaryWeapons);
                break;
            case Weapon_AutomaticGun.WeaponType.Melee:
                HandleWeaponAddition(ref currentMelee, weapon, meleeWeapons);
                break;
        }
        // 拾取后自动切换至该类型武器
        SwitchWeapon((ActiveWeaponType)(type + 1)); // 枚举值需对齐
    }

    private void HandleWeaponAddition(ref GameObject currentWeapon, GameObject newWeapon, List<GameObject> category)
    {
        // 替换逻辑
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);
            category.Remove(currentWeapon);
        }

        currentWeapon = newWeapon;
        category.Add(newWeapon);
        newWeapon.SetActive(false);// 先隐藏，由SwitchWeapon统一控制
    }

    // 武器切换核心逻辑
    public void SwitchWeapon(ActiveWeaponType type)
    {
        

        // 隐藏所有武器
        SetAllWeaponsActive(false);

        // 显示目标类型武器
        switch (type)
        {
            case ActiveWeaponType.Primary:
                if (currentPrimary != null) currentPrimary.SetActive(true);
                break;
            case ActiveWeaponType.Secondary:
                if (currentSecondary != null) currentSecondary.SetActive(true);
                break;
            case ActiveWeaponType.Melee:
                if (currentMelee != null) currentMelee.SetActive(true);
                break;
        }

        currentActiveType = type;
    }

    // 统一控制武器显示状态
    private void SetAllWeaponsActive(bool isActive)
    {
        foreach (var weapon in primaryWeapons) weapon.SetActive(isActive);
        foreach (var weapon in secondaryWeapons) weapon.SetActive(isActive);
        foreach (var weapon in meleeWeapons) weapon.SetActive(isActive);
    }
}
