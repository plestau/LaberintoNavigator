using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemigo : MonoBehaviour
{
    public float viewRadius = 10f;
    public float viewAngle = 45f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public Transform player;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;

    private NavMeshAgent agent;
    private Vector3 patrolDestination;
    private Light spotlight;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolDestination = GetRandomPatrolPoint();
        agent.speed = patrolSpeed;

        // Buscar al jugador dinámicamente por etiqueta
        GameObject playerObject = GameObject.FindGameObjectWithTag("Jugador");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró el jugador en la escena.");
        }

        // Configurar el spotlight
        spotlight = gameObject.AddComponent<Light>();
        spotlight.type = LightType.Spot;
        spotlight.range = viewRadius;
        spotlight.spotAngle = viewAngle;
        spotlight.intensity = 5f; // Ajusta la intensidad según sea necesario
        spotlight.color = Color.yellow; // Ajusta el color según sea necesario
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed = chaseSpeed;
            }
            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed = patrolSpeed;
            }
            Patrol();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        float dstToPlayer = Vector3.Distance(transform.position, player.position);

        Debug.Log($"Angle to player: {angleToPlayer}, Distance to player: {dstToPlayer}");

        if (angleToPlayer < viewAngle / 2)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // Ajusta la altura según sea necesario
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, dirToPlayer, out hit, dstToPlayer, obstacleMask))
            {
                Debug.DrawRay(rayOrigin, dirToPlayer * hit.distance, Color.red, 1f); // Rayo bloqueado
                Debug.Log($"Player is blocked by: {hit.collider.name}");
                return false; // El jugador está bloqueado por un obstáculo
            }
            else
            {
                Debug.DrawRay(rayOrigin, dirToPlayer * dstToPlayer, Color.green, 1f); // Rayo exitoso
                Debug.Log("Player is visible");
                return true; // El jugador es visible
            }
        }
        else
        {
            Debug.Log("Player is outside the view angle");
        }
        return false;
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, patrolDestination) < 1f || !agent.hasPath)
        {
            patrolDestination = GetRandomPatrolPoint();
            agent.SetDestination(patrolDestination);
        }
    }

    Vector3 GetRandomPatrolPoint()
    {
        int x, z;
        do
        {
            x = Random.Range(0, Generator.gen.xMax);
            z = Random.Range(0, Generator.gen.zMax);
        } while (Generator.gen.map[x, z] == null);

        return new Vector3(x * 5f, 0, z * 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jugador"))
        {
            LoadingScreenManager.instance.ShowLoadingScreen();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("Toque al jugador");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }

    Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}