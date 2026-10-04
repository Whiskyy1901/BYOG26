using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private string _Ishanturl = "https://";
    [SerializeField] private string _Altaafurl = "https://";

    public Animator transition;

    public float transitionTime = 1f;

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(gameSceneName);
    }

    public void Ishant()
    {
        if (!string.IsNullOrWhiteSpace(_Ishanturl))
            Application.OpenURL(_Ishanturl);
    }

    public void Altaaf()
    {
        if (!string.IsNullOrWhiteSpace(_Altaafurl))
            Application.OpenURL(_Altaafurl);
    }

    public void OnStart()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    

    public void OnQuit()
    {
        Application.Quit();
    }
}