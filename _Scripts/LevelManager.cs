using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else Destroy(gameObject);
    }

    [SerializeField] Animator _sceneTransitionAnimator;

    [SerializeField] int _currentLevel = 0;
    [SerializeField] int _levelCount = 5;

    [SerializeField] GraphicRaycaster _thatOneCanvasBlockingTheBuildOmg;


    IEnumerator LoadScene(string sceneName)
    {
        GameManager.Instance.Clean();
        _sceneTransitionAnimator.SetTrigger("Start");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(sceneName);
        _sceneTransitionAnimator.SetTrigger("End");
    }

    void LoadLevel(int level)
    {
        _thatOneCanvasBlockingTheBuildOmg.enabled = true;
        StartCoroutine(LoadScene($"Level-{level + 1}"));
    }
    public void LoadCurrentLevel()
    {
        LoadLevel(_currentLevel);
    }
    public void LoadNextLevel()
    {
        _currentLevel++;
        if(_currentLevel >= _levelCount ) { _currentLevel--;  }
        LoadCurrentLevel();
    }

    public void LoadTitleScreen()
    {
        _thatOneCanvasBlockingTheBuildOmg.enabled = false;
        StartCoroutine(LoadScene("TitleScreen"));
    }

    void LoadRandomLevel()
    {

    }

    public void ReloadLevel()
    {
        _currentLevel = SceneManager.GetActiveScene().buildIndex - 1;
        LoadCurrentLevel();
    }

    public void SetLoaderState(bool state)
    {
        _sceneTransitionAnimator.SetTrigger(state ? "End" : "Start");
    }

}
