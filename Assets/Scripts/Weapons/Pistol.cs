using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    [Header("Visual")]
    public float drawDuration = 0.5f;
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothness = 6f;
    private Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        WeaponSway();
    }
    private void WeaponSway()
    {
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);
        Vector3 finalPosition = new Vector3(initialPosition.x + mouseX, initialPosition.y + mouseY, initialPosition.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * swaySmoothness);
    }
    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(65f, -30f, 0);
        StartCoroutine(DrawWeapon());
    }

    private IEnumerator DrawWeapon()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, startRotation.eulerAngles.z);
        while (elapsedTime < drawDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / drawDuration);
            yield return null;
        }
        transform.localRotation = targetRotation;
    }
}
