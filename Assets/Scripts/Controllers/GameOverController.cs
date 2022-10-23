using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _menuButton;

    private bool _buttonsAvailable;

    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _playerText;
    [SerializeField] private TextMeshProUGUI _botText;

    void Start()
    {
		_retryButton.onClick.AddListener(OnRetryButton);
        _menuButton.onClick.AddListener(OnMenuButton);

        if (MatchController.PlayerLives > MatchController.BotLives)
        {
            _winnerText.text = "Player won!";
        }
        else
        {
            _winnerText.text = "Bot won!";
        }
        _playerText.text = "Player: " + MatchController.PlayerLives.ToString();
        _botText.text = "AI: " + MatchController.BotLives.ToString();

        StartCoroutine(WaitToMakeAvailable());

	}

    private void OnRetryButton()
    {
        if (_buttonsAvailable)
        {
            StartCoroutine(ChangeScene(1));
        }
    }

    private void OnMenuButton()
    {
        if (_buttonsAvailable)
        {
           StartCoroutine(ChangeScene(0));    
        }
    }

    private IEnumerator ChangeScene(int sceneNum) 
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneNum);
    }

    private IEnumerator WaitToMakeAvailable()
    {
        yield return new WaitForSeconds(0.5f);
        _buttonsAvailable = true;
    }
}
