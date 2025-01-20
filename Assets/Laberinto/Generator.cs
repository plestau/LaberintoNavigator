using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int xMax, zMax; // Dimensiones del laberinto
    public GameObject piece, laberinthPiece, npcPrefab, metaPrefab;
    public GameObject[,] map;
    public int limit;
    public static Generator gen;
    private NavMeshSurface navMeshSurface;

    private Vector3 piezaEscala = new Vector3(5f, 0.2f, 5f);

    void Start()
    {
        gen = this;
        map = new GameObject[xMax, zMax];
        navMeshSurface = GetComponent<NavMeshSurface>();
        StartCoroutine(GenerateLabyrinth());
    }

    private IEnumerator GenerateLabyrinth()
    {
        GenerateFirstFloor();
        yield return new WaitForSeconds(0.1f);

        AdjustWalls();
        CreateExit();

        yield return new WaitForEndOfFrame();
    }
    
    private void UpdateNavMeshBounds()
    {
        Vector3 laberintoSize = new Vector3(xMax * piezaEscala.x, 1, zMax * piezaEscala.z);
        Vector3 laberintoCenter = new Vector3((xMax - 1) * piezaEscala.x / 2, 0, (zMax - 1) * piezaEscala.z / 2);

        navMeshSurface.size = laberintoSize;
        navMeshSurface.center = laberintoCenter;

        navMeshSurface.BuildNavMesh();
    }

    public void SpawnNPC()
    {
        // Calcular la posición inicial del jugador en el extremo opuesto de la salida
        Vector3 playerStartPosition = new Vector3(0, 1, (zMax / 2) * piezaEscala.z);

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(playerStartPosition, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Jugador");
            if (player == null)
            {
                // Instanciar el prefab del jugador si no existe
                player = Instantiate(npcPrefab, hit.position, Quaternion.identity);
                player.tag = "Jugador";
            }
            else
            {
                player.transform.position = hit.position;
                player.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(hit.position);
            }

            StartCoroutine(AdjustCameraAfterDelay(player.transform));
        }
        else
        {
            Debug.LogError("Failed to find a valid NavMesh position for the player.");
        }
    }

    private IEnumerator AdjustCameraAfterDelay(Transform playerTransform)
    {
        yield return new WaitForSeconds(1);

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.playerTransform = playerTransform;
        }
    }

    public void GenerateFirstFloor()
    {
        Vector3 posicionInicial = new Vector3((xMax / 2) * piezaEscala.x, 0, (zMax / 2) * piezaEscala.z);
        LaberinthPiece newPiece = Instantiate(laberinthPiece, posicionInicial, Quaternion.identity).GetComponent<LaberinthPiece>();
        newPiece.n = true;
        newPiece.s = true;
        newPiece.e = true;
        newPiece.w = true;
        newPiece.x = xMax / 2;
        newPiece.z = zMax / 2;
        map[xMax / 2, zMax / 2] = newPiece.gameObject;
    }

    public void GenerateNextPiece(int x, int z)
    {
        if (map[x, z] == null)
        {
            Vector3 posicion = new Vector3(x * piezaEscala.x, 0, z * piezaEscala.z);
            LaberinthPiece newPiece = Instantiate(laberinthPiece, posicion, Quaternion.identity).GetComponent<LaberinthPiece>();
            newPiece.x = x;
            newPiece.z = z;
            map[x, z] = newPiece.gameObject;
        }
    }


    private void AdjustWalls()
    {
        foreach (var piece in map)
        {
            if (piece != null)
            {
                piece.GetComponent<LaberinthPiece>().CheckWalls();
            }
        }
    }
    
    private void CreateExit()
    {
        bool exitCreated = false;

        while (!exitCreated)
        {
            int exitX = xMax;
            int exitZ = Random.Range(0, zMax);

            if (map[xMax - 1, exitZ] != null)
            {
                LaberinthPiece exitPiece = map[xMax - 1, exitZ].GetComponent<LaberinthPiece>();
                exitPiece.e = true;
                exitPiece.CheckWalls();
                exitCreated = true;

                // Instanciar el prefab Meta fuera del laberinto
                Vector3 exitPosition = new Vector3(exitX * piezaEscala.x, 0, exitZ * piezaEscala.z);
                Instantiate(metaPrefab, exitPosition, Quaternion.identity);
            }
        }

        UpdateNavMeshBounds();
        SpawnNPC();
    }
}
