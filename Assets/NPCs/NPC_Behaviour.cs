using UnityEngine;
using UnityEngine.AI;

public class NPC_Behaviour : MonoBehaviour
{
    [SerializeField] private Vector3 destination;
    [SerializeField] private Transform path;
    [SerializeField] private int childrenIndex;
    [SerializeField] private Vector3 min, max;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        #region Mouse Button Click
        if(Input.GetButtonDown("Fire1"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if(Physics.Raycast(ray, out hit, 1000))
            {
                GetComponent<NavMeshAgent>().SetDestination(hit.point);
            }
        }
        #endregion
        /*
        #region Patroll Movement

        if (Vector3.Distance(transform.position, destination) < 1f)
        {
            childrenIndex++;
            childrenIndex = childrenIndex % path.childCount;

            destination = path.GetChild(childrenIndex).position;
            GetComponent<NavMeshAgent>().SetDestination(destination);
        }
        #endregion
        */
        
        #region Random Patroll
        if (Vector3.Distance(transform.position, destination) < 1f)
        {
            destination = RandomDestination();
            GetComponent<NavMeshAgent>().SetDestination(destination);
        }
        #endregion
    }

    #region RandomDestination
    Vector3 RandomDestination()
    {
        return new Vector3(Random.Range(min.x, max.x), 0, Random.Range(min.z, max.z));
    }
    #endregion
}