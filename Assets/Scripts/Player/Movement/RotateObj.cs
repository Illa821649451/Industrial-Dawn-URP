using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    void Update()
    {
        transform.rotation = targetObject.rotation;
    }
}
