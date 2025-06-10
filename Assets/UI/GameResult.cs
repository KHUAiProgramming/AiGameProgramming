using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityEngine;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject ResultCanvasDefender;
    [SerializeField] private GameObject ResultCanvasAttacker;

    void Start()
    {
        ResultCanvasAttacker.SetActive(false);
        ResultCanvasDefender.SetActive(false);
    }

    public void ShowResult(bool attackerWon)
    {
        Debug.Log($"ShowResult called — attackerWon: {attackerWon}");

        ResultCanvasAttacker.SetActive(!attackerWon);
        ResultCanvasDefender.SetActive(attackerWon);

        Time.timeScale = 0f;

    }

    public void RestartGame()
    {
        Debug.Log("Restart Game called");

        Time.timeScale = 1f;

        ResultCanvasAttacker.SetActive(false);
        ResultCanvasDefender.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
