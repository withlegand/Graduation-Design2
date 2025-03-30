using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //������
    //public List<GameObject> weapons = new List<GameObject>();
    // �滻ԭ��������Ϊ���������
    public List<GameObject> primaryWeapons = new List<GameObject>();
    public List<GameObject> secondaryWeapons = new List<GameObject>();
    public List<GameObject> meleeWeapons = new List<GameObject>();
    //��ǰ�������
    //public int currentWeaponID  ;
    // ��ǰװ��������
    private GameObject currentPrimary;
    private GameObject currentSecondary;
    private GameObject currentMelee;

    // ��ǰ�������������
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
        // ���ּ��л���������
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(ActiveWeaponType.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(ActiveWeaponType.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(ActiveWeaponType.Melee);
    }

    //�����������
    //public void ChangeCurrentWeaponID()
    //{
    //    //-0.1,0,0.1
    //    if(Input.GetAxis("Mouse ScrollWheel") < 0)
    //    {
    //        //��һ������
    //        ChangeWeapon(currentWeaponID+1);
    //    }
    //    else if(Input.GetAxis("Mouse ScrollWheel") > 0)
    //    {
    //        //��һ������
    //        ChangeWeapon(currentWeaponID - 1);
    //    }

    //    //ͨ�����ּ����л�����
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
    //            /*ֻ������С�������б������ʱ����ܽ�һ������
    //             �����3��������������4-9����ô��������ģ�������*/
    //            if (num < weapons.Count)
    //            {
    //                ChangeWeapon(num);
    //            }

    //        }
    //    }
    //}

    ////�����л�
    //public void ChangeWeapon(int weaponID)
    //{
    //    if (weapons.Count == 0) return;

    //    /*����л������������ŵ�ǹ��ʱ�򣬾͵�����һ��ǹ
    //      ����л�����С������ŵ�ǹ��ʱ�򣬾͵������һ��ǹ*/

    //    //Indexof:��ȡ�б���Ԫ���״γ��ֵ�����
    //    //MAX listȡ���Ԫ��
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
    //        //ֻ��һ��������ʱ�򲻽����л�
    //        return;
    //    }

    //    currentWeaponID = weaponID;//������������
    //    //����������ţ���ʾ����Ӧ������
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

    ////��ʰ����
    //public void AddWeapon(GameObject weapon)
    //{
    //    if (weapons.Contains(weapon))
    //    {
    //        print("�������Ѵ��ڴ�ǹе");
    //        return;
    //    }
    //    else
    //    {
    //        //if(weapons.Count <3)
    //        //{

    //            weapons.Add(weapon);
    //            ChangeWeapon(currentWeaponID + 1);//��ʾ����
    //            weapon.gameObject.SetActive(true);
    //        //}
    //    }
    //}

    ////��������
    //public void ThrowWeapon(GameObject weapon)
    //{
    //    if (!weapons.Contains(weapon) || weapons.Count == 0)
    //    {
    //        print("û������������޷�����");
    //        return;
    //    }
    //    else
    //    {
    //        weapons.Remove(weapon);//ɾ����Ӧ������
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
        // ʰȡ���Զ��л�������������
        SwitchWeapon((ActiveWeaponType)(type + 1)); // ö��ֵ�����
    }

    private void HandleWeaponAddition(ref GameObject currentWeapon, GameObject newWeapon, List<GameObject> category)
    {
        // �滻�߼�
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);
            category.Remove(currentWeapon);
        }

        currentWeapon = newWeapon;
        category.Add(newWeapon);
        newWeapon.SetActive(false);// �����أ���SwitchWeaponͳһ����
    }

    // �����л������߼�
    public void SwitchWeapon(ActiveWeaponType type)
    {
        

        // ������������
        SetAllWeaponsActive(false);

        // ��ʾĿ����������
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

    // ͳһ����������ʾ״̬
    private void SetAllWeaponsActive(bool isActive)
    {
        foreach (var weapon in primaryWeapons) weapon.SetActive(isActive);
        foreach (var weapon in secondaryWeapons) weapon.SetActive(isActive);
        foreach (var weapon in meleeWeapons) weapon.SetActive(isActive);
    }
}
