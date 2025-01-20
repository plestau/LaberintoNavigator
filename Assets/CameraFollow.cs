using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 10, -5); // Adjust the offset for a higher and slightly behind position
    public Quaternion angulosCamara = Quaternion.Euler(60, 0, 0);
    
    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Jugador");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Player GameObject with tag 'Jugador' not found.");
            }
        }
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 newPosition = playerTransform.position + offset;
            transform.position = newPosition;
            transform.rotation = angulosCamara;
        }
    }
}