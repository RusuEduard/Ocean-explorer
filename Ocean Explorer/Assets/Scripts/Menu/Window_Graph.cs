using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class Window_Graph : MonoBehaviour
{
    [SerializeField]
    private Sprite circleSprite;
    private RectTransform graphContainer;

    private List<List<float>> data = new List<List<float>>();

    private void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        var fileData = System.IO.File.ReadAllLines(Application.dataPath + "/data.csv");

        foreach (var param in fileData[0].Trim().Split(','))
        {
            data.Add(new List<float>());
        }

        for (int i = 1; i < fileData.Count(); i++)
        {
            var lineData = fileData[i].Trim().Split(',');
            for (int j = 0; j < lineData.Count(); j++)
            {
                data[j].Add(float.Parse(lineData[j], CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        ShowGraph(data[(int)DataIndexingEnum.ALIGN_WEIGHT]);
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private void ShowGraph(List<float> valueList)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float maxValue = valueList.Max();
        float minValue = valueList.Min();
        GameObject lastGameObject = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = ((float)i / valueList.Count) * graphContainer.sizeDelta.x;
            float yPosition = (valueList[i] - minValue) / (maxValue - minValue) * graphHeight;
            GameObject circle = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastGameObject != null)
            {
                CreateDotConnection(lastGameObject.GetComponent<RectTransform>().anchoredPosition, circle.GetComponent<RectTransform>().anchoredPosition);
            }
            lastGameObject = circle;
        }
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }
}
