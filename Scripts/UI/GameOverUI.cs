using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button restartButton;

    private void Start()
    {
        GameManager.Instance.onBaseDestroyed += () =>
        {
            Time.timeScale = 0;
            gameOverPanel.gameObject.SetActive(true);
        };
            
        restartButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }
}
