using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUtility : MonoBehaviour
{
    // Phát âm thanh click khi nhấn nút bất kỳ
    public void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void LoadScene(string sceneName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSelectCategory()
    {
        LoadScene("SelectCategory");
    }

    public void LoadGameScene()
    {
        LoadScene("GameScene");
    }

    public void LoadStartScene()
    {
        LoadScene("StartScene");
    }

    // Bật/tắt âm thanh (gán vào nút Music/Mute trong scene)
    public void ToggleMute()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ToggleMute();
    }
}
