using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField] private Button _playButton;

    void Start()
    {
		_playButton.onClick.AddListener(OnPlayButton);
	}

    private void OnPlayButton()
    {
        StartCoroutine(ChangeScene(1));
    }

    private IEnumerator ChangeScene(int sceneNum) 
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneNum);
    }
}
