using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject[] redTeam;
    public GameObject[] blueTeam;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        redTeam = GameObject.FindGameObjectsWithTag("Red");
        blueTeam = GameObject.FindGameObjectsWithTag("Blue");
        if (redTeam.Length == 0 || blueTeam.Length == 0)
        {
            Debug.Log("game end");
            SceneManager.LoadScene(0);
        }
    }
}
