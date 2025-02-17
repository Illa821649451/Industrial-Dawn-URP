using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Revolver : MonoBehaviour
{
    public float vertical = 0f; // Поточне значення vertical
    public float targetVertical = 0.5f; // Цільове значення vertical
    public float speed = 2f;

    private PlayerMovementAdvanced pm;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInParent<Animator>();
        pm = GetComponentInParent<PlayerMovementAdvanced>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimationController();
    }

    private void AnimationController()
    {
        if (pm.isSprinting)
            targetVertical = 1f;
        else
            targetVertical = 0.5f;
        if (Input.GetKey(KeyCode.W))
        {
            // Плавно збільшуємо значення vertical до targetVertical
            vertical = Mathf.MoveTowards(vertical, targetVertical, speed * Time.deltaTime);
            anim.SetFloat("vertical", vertical);
        }
        else
        {
            // Якщо клавіша не затиснута, повертаємо значення vertical до 0
            vertical = Mathf.MoveTowards(vertical, 0f, speed * Time.deltaTime);
            anim.SetFloat("vertical", vertical);
        }
    }
}
