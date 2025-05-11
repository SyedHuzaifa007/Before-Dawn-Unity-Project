using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseTrigger : MonoBehaviour
{
    public GameObject levelLoseMsg;
    public AudioSource yamete;

    private void Start()
    {
        // Try to find the object once at start instead of every frame
        if (levelLoseMsg == null)
        {
            levelLoseMsg = GameObject.Find("Canvas With Win and Lose Msg/LevelLose");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Debug.Log should have capital L
            Debug.Log(levelLoseMsg);

            if (levelLoseMsg != null)
            {
                levelLoseMsg.SetActive(true);

                // Play sound if assigned
                if (yamete != null)
                {
                    yamete.Play();
                }

                StartCoroutine(Close(3));
            }
            else
            {
                Debug.LogError("levelLoseMsg is not assigned and couldn't be found!");
            }
        }
    }

    public IEnumerator Close(float x)
    {
        yield return new WaitForSeconds(x - 1);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        // No need to set inactive since the scene is reloading
    }
}