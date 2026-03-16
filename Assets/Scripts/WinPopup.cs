using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPopup : MonoBehaviour
{
    public GameObject winPopup;
    private bool _returnToSelectCategory = false;

    void Start()
    {
        winPopup.SetActive(false);
    }

    void OnEnable()
    {
        GameEvents.OnBoardCompleted += ShowWinPopup;
        GameEvents.OnCategoryCompleted += ShowCategoryCompletePopup;
    }

    void OnDisable()
    {
        GameEvents.OnBoardCompleted -= ShowWinPopup;
        GameEvents.OnCategoryCompleted -= ShowCategoryCompletePopup;
    }

    private void ShowWinPopup()
    {
        _returnToSelectCategory = false;
        winPopup.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWin();
    }

    private void ShowCategoryCompletePopup()
    {
        _returnToSelectCategory = true;
        winPopup.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCategoryComplete();
    }

    public void LoadNextLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (_returnToSelectCategory)
        {
            // Hoàn thành category → quay về màn chọn category
            SceneManager.LoadScene("SelectCategory");
        }
        else
        {
            // Vẫn còn màn trong category → tải lại GameScene (màn tiếp theo)
            GameEvents.LoadNextLevelMethod();
        }
    }
}
