using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChange : MonoBehaviour
{
    public int GunIndex = 2;
    public GameObject RifleHands;
    public GameObject PistolHands;
    public GameObject SawHands;
    public GameObject CrossbowHands;
    public bool PistolObtained = true;
    public bool CrossbowObtained = true;
    public bool RifleObtained = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Index();

        if (GunIndex == 0 && RifleObtained)
        {
            RifleHands.gameObject.SetActive(true);
        }
        else
        {
            RifleHands.gameObject.SetActive(false);
        }
        if (GunIndex == 1 && PistolObtained)
        {
            PistolHands.gameObject.SetActive(true);
        }
        else
        {
            PistolHands.gameObject.SetActive(false);
        }
        if (GunIndex == 2)
        {
            SawHands.gameObject.SetActive(true);
        }
        else
        {
            SawHands.gameObject.SetActive(false);
        }
        if (GunIndex == 3 && CrossbowObtained)
        {
            CrossbowHands.gameObject.SetActive(true);
        }
        else
        {
            CrossbowHands.gameObject.SetActive(false);
        }
    }

    private void Index()
    {
        // Зміна зброї через клавіші
        if (Input.GetKeyDown(KeyCode.Alpha1) && RifleObtained) { GunIndex = 0; }
        if (Input.GetKeyDown(KeyCode.Alpha2) && PistolObtained) { GunIndex = 1; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { GunIndex = 2; }
        if (Input.GetKeyDown(KeyCode.Alpha4) && CrossbowObtained) { GunIndex = 3; }

        // Зміна зброї через колесико миші
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            GunIndex--;
            GunIndex = ClampToAvailableIndex(GunIndex, false);
        }
        else if (scroll < 0f)
        {
            GunIndex++;
            GunIndex = ClampToAvailableIndex(GunIndex, true);
        }
    }

    private int ClampToAvailableIndex(int currentIndex, bool scrollingDown)
    {
        if (scrollingDown && currentIndex > 3) currentIndex = 0;
        if (!scrollingDown && currentIndex < 0) currentIndex = 3;

        while ((currentIndex == 0 && !RifleObtained) ||
               (currentIndex == 1 && !PistolObtained) ||
               (currentIndex == 3 && !CrossbowObtained))
        {
            if (scrollingDown)
            {
                currentIndex++;
                if (currentIndex > 3) currentIndex = 0;
            }
            else
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex = 3;
            }
        }

        if (!RifleObtained && !PistolObtained && !CrossbowObtained)
        {
            currentIndex = 2;
        }

        return currentIndex;
    }
}
