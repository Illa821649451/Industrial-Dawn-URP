using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPipeAnim : MonoBehaviour
{
    public Transform CamTransform;
    public Transform CamHolder;

    public Transform Player;
    public Transform ArmsPlayer;

    public Transform TrackRightPalm;
    public Transform TrackLeftPalm;

    private Animator anim;
    private PlayerMovementAdvanced PM;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        PM = GetComponentInParent<PlayerMovementAdvanced>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PM.onPipe)
        {
            ArmsPlayer.SetParent(Player);
            CamHolder.SetParent(ArmsPlayer);
            ArmsPlayer.localPosition = new Vector3(0f, 0.535f, 0f);
            ArmsPlayer.localRotation = Quaternion.Euler(0f, 90f, 0f);
            /*if(CamHolder.localRotation.y < -20f)
            {
                TrackLeftPalm.SetParent(CamHolder);
            }
            else if(CamHolder.localRotation.y > 20f)
            {
                TrackRightPalm.SetParent(CamHolder);
            }*/
        }
        else
        {
            CamHolder.SetParent(Player);
            ArmsPlayer.SetParent(CamTransform);
            ArmsPlayer.localPosition = new Vector3(0f, -0.215f, 0f);
            ArmsPlayer.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        anim.SetBool("OnPipe", PM.onPipe);
    }
}
