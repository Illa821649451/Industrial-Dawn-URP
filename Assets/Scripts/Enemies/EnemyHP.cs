using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHP : MonoBehaviour, IDamageable
{
    public float Health;
    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            EnemyRevolver enemyRevolver = GetComponent<EnemyRevolver>();
            GameObject ragdoll = transform.Find("Human").gameObject;
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (enemyRevolver != null)
            {
                Destroy(enemyRevolver);
            }
            ragdoll.SetActive(true);
            agent.enabled = false;
        }
    }
}
