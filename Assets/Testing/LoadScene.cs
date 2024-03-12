using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] Button LoadSceneBtn;
    [SerializeField] string SceneName;

    private void Start()
    {
        LoadSceneBtn.onClick.AddListener(HandleLoadSceneBtnClicked);
    }

    private void HandleLoadSceneBtnClicked()
    {
        SceneManager.LoadScene(SceneName);
    }
}
