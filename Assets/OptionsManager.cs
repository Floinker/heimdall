using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class OptionsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    public Text musicPercent;
    public Text sfxPercent;

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            float value = PlayerPrefs.GetFloat("musicVolume");
            musicSlider.value = value;

            value = value * 100;
            musicPercent.text = "" + (int)value + " %";
        }
            
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            float value = PlayerPrefs.GetFloat("sfxVolume");
            sfxSlider.value = value;

            value = value * 100;
            sfxPercent.text = "" + (int)value + " %";
        }
            
    }

    public void UpdatePercent()
    {
        float value = sfxSlider.value * 100;
        sfxPercent.text = "" + (int)value + " %";

        value = musicSlider.value * 100;
        musicPercent.text = "" + (int)value + " %";
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }
}
