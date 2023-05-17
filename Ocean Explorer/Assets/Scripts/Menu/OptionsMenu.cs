using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{

    public Toggle fullScreenTog, vsyncTog;

    public List<ResItem> resolutions = new List<ResItem>();

    public TMP_Text resolutionLabel;

    private int selectedRes;
    void Start()
    {
        fullScreenTog.isOn = Screen.fullScreen;
        vsyncTog.isOn = QualitySettings.vSyncCount != 0;

        bool foundResolution = false;
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                this.selectedRes = i;
                foundResolution = true;
                UpdateResLabel();
            }
        }

        if (!foundResolution)
        {
            ResItem newRes = new ResItem();
            newRes.horizontal = Screen.width;
            newRes.vertical = Screen.height;

            resolutions.Add(newRes);
            selectedRes = resolutions.Count - 1;
            UpdateResLabel();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResLeft()
    {
        selectedRes--;
        if (selectedRes < 0)
        {
            selectedRes = 0;
        }

        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedRes++;
        if (selectedRes > this.resolutions.Count - 1)
        {
            selectedRes = this.resolutions.Count - 1;
        }

        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedRes].horizontal.ToString() + " x " + resolutions[selectedRes].vertical.ToString();
    }

    public void ApplyGraphics()
    {
        Screen.fullScreen = fullScreenTog.isOn;

        QualitySettings.vSyncCount = vsyncTog.isOn ? 1 : 0;

        Screen.SetResolution(this.resolutions[selectedRes].horizontal, this.resolutions[selectedRes].vertical, fullScreenTog.isOn);
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}