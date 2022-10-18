using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _menuButton;

    void Start()
    {
		_retryButton.onClick.AddListener(OnRetryButton);
        _menuButton.onClick.AddListener(OnMenuButton);
	}

    private void OnRetryButton()
    {
        StartCoroutine(ChangeScene(1));
    }

    private void OnMenuButton()
    {
        StartCoroutine(ChangeScene(0));
    }

    private IEnumerator ChangeScene(int sceneNum) 
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneNum);
    }
}
