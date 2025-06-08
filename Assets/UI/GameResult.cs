using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{

    public static void ShowResult(bool attackerWon)
    {
        if (attackerWon)
        {
            GameObject canvas = GameObject.Find("ResultCanvasAttack");
            if (canvas != null)
            {
                canvas.SetActive(true);
                canvas.transform.Find("ResultText").GetComponent<Text>().text = "ATTACKER WIN!";
            }
        }
        else
        {
            GameObject canvas = GameObject.Find("ResultCanvasDefend");
            if (canvas != null)
            {
                canvas.SetActive(true);
                canvas.transform.Find("ResultText").GetComponent<Text>().text = "DEFENDER WIN!";
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
