using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{

    public void OnPlayMode0Button()
    {
        MatchController.CurrentGameMode = GameMode.none;
        MatchController.NumberLives = 1;
        StartCoroutine(ChangeScene(1));
    }


    public void OnPlayMode1Button()
    {
        MatchController.CurrentGameMode = GameMode.genetic;
        MatchController.UpdateWarmUp(true);
        MatchController.CurrentDifficulty = 0;
        MatchController.NumberLives = 2;
        StartCoroutine(ChangeScene(1));
    }

    public void OnPlayMode2Button()
    {
        MatchController.CurrentGameMode = GameMode.classical;
        MatchController.CurrentDifficulty = 0;
        MatchController.NumberLives = 4;
        StartCoroutine(ChangeScene(1));
    }

    public void OnPlayMode3Button()
    {
        MatchController.CurrentGameMode = GameMode.manyEnemies;
        MatchController.CurrentDifficulty = 0;
        MatchController.NumberLives = 4;
        StartCoroutine(ChangeScene(1));
    }

    public void OnEndEditInput(string playerID)
    {
        if (playerID == "Enter your ID...")
        {
            MatchController.PlayerID = "";
        }
        else
        {
            MatchController.PlayerID = playerID;
        }
    }

    private IEnumerator ChangeScene(int sceneNum) 
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneNum);
    }
}
