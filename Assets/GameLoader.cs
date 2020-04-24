using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameLoader : MonoBehaviour
{
    public GameObject loadingBar;
    public Slider slider;

    public AudioMixer mixer;

    private bool volumeSet = false;

    private void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex != 2) { 
            if (PlayerPrefs.HasKey("musicVolume"))
            {
                //Debug.Log(PlayerPrefs.GetFloat("musicVolume"));
                mixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("musicVolume")) * 20);
            }
            if (PlayerPrefs.HasKey("sfxVolume"))
            {
                //mixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("sfxVolume"));
                volumeSet = true;
            }
        }
    }

    public void LoadGame()
    {
        StartCoroutine(LoadAsynchronously());
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    IEnumerator LoadAsynchronously()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);

        loadingBar.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            yield return null;
        }
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
