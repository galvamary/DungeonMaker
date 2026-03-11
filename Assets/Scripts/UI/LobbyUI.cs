using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 로비 화면 UI 관리 - 게임 시작, 오디오 설정, 종료 버튼
/// </summary>
public class LobbyUI : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnAudioSettingsButtonClicked()
    {
        if (SettingsUI.Instance != null)
        {
            SettingsUI.Instance.ToggleSettings();
        }
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
