using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControllerSystem : MonoBehaviour
{
    public GameObject handGun;
    public GameObject shotGun;
    public GameObject machineGun;
    public string activeWeapon;

    void Start()
    {
        activeWeapon = "Handgun";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            handGun.SetActive(true);
            shotGun.SetActive(false);
            machineGun.SetActive(false);
            activeWeapon = "Handgun";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            handGun.SetActive(false);
            shotGun.SetActive(true);
            machineGun.SetActive(false);
            activeWeapon = "Shotgun";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            handGun.SetActive(false);
            shotGun.SetActive(false);
            machineGun.SetActive(true);
            activeWeapon = "Machinegun";
        }
    }
}
