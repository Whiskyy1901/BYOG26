using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   [SerializeField] private SceneAsset gameScene;
   [SerializeField] private SceneAsset creditsScene;

   public void OnStart()
   {
      SceneManager.LoadScene(gameScene.name);
   }

   public void OnCredits()
   {
      SceneManager.LoadScene(creditsScene.name);
   }
   public void OnQuit()
   {
      Application.Quit();
   }
}
