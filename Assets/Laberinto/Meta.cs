using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Meta : MonoBehaviour
{
    private Coroutine restartCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jugador"))
        {
            Debug.Log("Terminado");
            restartCoroutine = StartCoroutine(RestartSceneAfterDelay(3f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Jugador"))
        {
            if (restartCoroutine != null)
            {
                StopCoroutine(restartCoroutine);
                restartCoroutine = null;
            }
        }
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}