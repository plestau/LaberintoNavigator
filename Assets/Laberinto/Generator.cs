using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
    public int xMax, zMax; // Dimensiones del laberinto
    public GameObject laberinthPiece, npcPrefab, metaPrefab;
    public GameObject[,] map;
    public int limit;
    public static Generator gen;
    private NavMeshSurface navMeshSurface;
    public GameObject enemyPrefab;
    public int numberOfEnemies;

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
        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.ShowLoadingScreen();
            LoadingScreenManager.instance.SetProgress(0.1f);
        }

        GenerateFirstFloor();
        yield return new WaitForSeconds(0.1f);

        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.SetProgress(0.5f);
        }
        AdjustWalls();
        CreateExit();
        SpawnEnemies();

        yield return new WaitForEndOfFrame();

        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.SetProgress(1.0f);
            LoadingScreenManager.instance.HideLoadingScreen();
        }
    }
    
    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 randomPosition;
            NavMeshHit hit;
            do
            {
                randomPosition = new Vector3(Random.Range(0, xMax) * piezaEscala.x, 0, Random.Range(0, zMax) * piezaEscala.z);
            } while (!NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas));

            Instantiate(enemyPrefab, hit.position, Quaternion.identity);
        }
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
        // Calcular la posición de la salida
        Vector3 exitPosition = new Vector3(xMax * piezaEscala.x, 0, Random.Range(0, zMax) * piezaEscala.z);

        // Calcular la posición más alejada de la salida
        Vector3 playerStartPosition = new Vector3(0, 1, (zMax / 2) * piezaEscala.z);

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(playerStartPosition, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            InstantiatePlayer(hit.position);
        }
        else
        {
            Debug.LogError("Failed to find a valid NavMesh position for the player. Forcing spawn at default position.");
            // la posicion por defecto es el centro del laberinto
            Vector3 defaultPosition = new Vector3((xMax / 2) * piezaEscala.x, 0, (zMax / 2) * piezaEscala.z);
            InstantiatePlayer(defaultPosition);
        }
    }

    private void InstantiatePlayer(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Jugador");
        if (player == null)
        {
            // Instanciar el prefab del jugador si no existe
            player = Instantiate(npcPrefab, position, Quaternion.identity);
            player.tag = "Jugador";
        }
        else
        {
            player.transform.position = position;
            player.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(position);
        }

        StartCoroutine(AdjustCameraAfterDelay(player.transform));
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
