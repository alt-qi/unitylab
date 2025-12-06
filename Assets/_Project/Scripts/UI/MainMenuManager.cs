using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    // [SerializeField] private Button exitButton;
    
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        // exitButton.onClick.AddListener(ExitGame);
    }
    
    
    private void StartGame()
    {
        Debug.Log("StartGame");
        SceneManager.LoadScene("_Project/Scenes/Dev/IgorAndUi");
    }
}
