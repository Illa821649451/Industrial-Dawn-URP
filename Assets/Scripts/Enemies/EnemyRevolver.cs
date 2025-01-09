using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRevolver : EnemyParent
{
    public override void Update()
    {
        base.Update();
        if(isDetected)
        {
            PlayerNoticed();
        }
    }
    public void PlayerNoticed()
    {
        agent.SetDestination(playerRef.transform.position);
    }
}
