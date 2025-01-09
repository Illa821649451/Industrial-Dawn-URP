using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    protected static List<EnemyParent> allEnemies = new List<EnemyParent>();

    [Header("Enemy parameters")]
    [SerializeField] protected bool isElite;

    protected NavMeshAgent agent;
    [SerializeField] protected List<Vector3> patrolPoints;
    protected int currentPatrolIndex;
    private bool goingForward = false;
    private bool changingPoint = false;
    private bool goingToLastKnown;

    public GameObject playerRef;

    [HideInInspector] public bool canSeePlayer;

    private Coroutine increaseDetectionCoroutine;
    private Coroutine decreaseDetectionCoroutine;

    protected Slider DetectionSlider;
    protected bool isDetected = false;

    [Header("Enemy parameters")]
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public Vector3 lastKnownPosition;


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public virtual void Start()
    {
        allEnemies.Add(this);
        agent = GetComponent<NavMeshAgent>();
        GameObject slider = transform.Find("Canvas/DetectionSlider").gameObject;
        playerRef = GameObject.FindGameObjectWithTag("Player");
        DetectionSlider = slider.GetComponent<Slider>();
        StartCoroutine("FindTargetsWithDelay", .01f);
    }
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    void FindVisibleTargets()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, (target.position + new Vector3(0,2,0)));

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    lastKnownPosition = playerRef.transform.position;
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                    /*if (isElite)
                    {
                        Debug.Log("IsElite activated");
                        isElite = false;
                        changingPoint = true;
                        agent.SetDestination(lastKnownPosition);
                        goingToLastKnown = true;
                    }*/
                }
            }
        }
        else if (canSeePlayer)
        {           
            canSeePlayer = false;
        }
    }


    public virtual void Update()
    {
        PathWalking();
        PatrolingArea();
        if (goingToLastKnown)
        {
            if (agent.remainingDistance == 0)
            {
                StartCoroutine(InspectingDelay());
            }
        }
    }
    public virtual void PatrolingArea()
    {
        if (canSeePlayer && !isDetected)
        {
            /*if (increaseDetectionCoroutine == null)
            {
                increaseDetectionCoroutine = StartCoroutine(IncreaseDetectionSlider());
            }

            if (decreaseDetectionCoroutine != null)
            {
                StopCoroutine(decreaseDetectionCoroutine);
                decreaseDetectionCoroutine = null;
            }*/

            agent.isStopped = true;
            Vector3 lookPosition = new Vector3(playerRef.transform.position.x, transform.position.y, playerRef.transform.position.z);
            transform.LookAt(lookPosition);

            if (DetectionSlider.value >= DetectionSlider.maxValue)
            {
                isDetected = true;
                SetDetectionForAll();
                StopCoroutine(increaseDetectionCoroutine);
                increaseDetectionCoroutine = null;
                decreaseDetectionCoroutine = null;
            }
        }
        else if (!canSeePlayer)
        {
            if (decreaseDetectionCoroutine == null)
            {
                decreaseDetectionCoroutine = StartCoroutine(DecreaseDetectionSlider());
            }

            if (increaseDetectionCoroutine != null)
            {
                StopCoroutine(increaseDetectionCoroutine);
                increaseDetectionCoroutine = null;
            }

            agent.isStopped = false;
        }
    }
    private IEnumerator IncreaseDetectionSlider()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            DetectionSlider.value += 1;
        }
    }

    private IEnumerator DecreaseDetectionSlider()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (DetectionSlider.value > 0)
            {
                DetectionSlider.value -= 1;
            }
        }
    }
    public virtual void PathWalking()
    {
        if (agent.remainingDistance == agent.stoppingDistance && !agent.pathPending && changingPoint == false)
        {
            changingPoint = true;
            StartCoroutine(WalkingDelay());
        }
    }

    private IEnumerator WalkingDelay()
    {
        yield return new WaitForSeconds(3f);
        if (!goingToLastKnown)
        {
            if (goingForward)
            {
                currentPatrolIndex++;
                if (currentPatrolIndex >= patrolPoints.Count)
                {
                    currentPatrolIndex = patrolPoints.Count - 1;
                    goingForward = false;
                }
            }
            else
            {
                currentPatrolIndex--;
                if (currentPatrolIndex < 0)
                {
                    currentPatrolIndex = 0;
                    goingForward = true;
                }
            }
            agent.SetDestination(patrolPoints[currentPatrolIndex]);
            changingPoint = false;
        }
        else { yield return null; }
    }

    private IEnumerator InspectingDelay()
    {
        Debug.Log("Inspection started");
        goingToLastKnown = false;
        yield return new WaitForSeconds(5);
        agent.SetDestination(patrolPoints[currentPatrolIndex]);
        changingPoint = false;
        isElite = true;
        Debug.Log("Inspection ended");
    }
    public static void SetDetectionForAll()
    {
        foreach (var enemy in allEnemies)
        {
            enemy.isDetected = true;
        }
    }
    public virtual void OnDestroy()
    {
        allEnemies.Remove(this);
    }
}
