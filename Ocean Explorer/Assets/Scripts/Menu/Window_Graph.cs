using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using TMPro;

public class Window_Graph : MonoBehaviour
{
    [SerializeField]
    private Sprite circleSprite;
    private RectTransform graphContainer;

    private List<List<float>> data = new List<List<float>>();

    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    private List<GameObject> labelsX = new List<GameObject>();
    private List<GameObject> labelsY = new List<GameObject>();
    private List<GameObject> dashesX = new List<GameObject>();
    private List<GameObject> dashesY = new List<GameObject>();

    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private GameObject tooltipGameObject;

    public Button minSpeedButton;
    public Button maxSpeedButton;
    public Button alignmentButton;
    public Button cohesionButton;
    public Button separationButton;
    public Button avoidanceButton;

    public int separatorCountX;
    public int separatorCountY;

    private Button disabledButton;

    private void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        tooltipGameObject = graphContainer.Find("tooltip").gameObject;

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
        ShowGraph(data[(int)DataIndexingEnum.MIN_SPEED]);
        minSpeedButton.interactable = false;
        disabledButton = minSpeedButton;
    }

    public void ShowMinSpeed()
    {
        ClearGraph();
        disabledButton.interactable = true;
        minSpeedButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.MIN_SPEED]);
        disabledButton = minSpeedButton;
    }

    public void ShowMaxSpeed()
    {
        ClearGraph();
        disabledButton.interactable = true;
        maxSpeedButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.MAX_SPEED]);
        disabledButton = maxSpeedButton;
    }

    public void ShowAlignment()
    {
        ClearGraph();
        disabledButton.interactable = true;
        alignmentButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.ALIGN_WEIGHT]);
        disabledButton = alignmentButton;
    }

    public void ShowCohesion()
    {
        ClearGraph();
        disabledButton.interactable = true;
        cohesionButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.COHESION_WEIGHT]);
        disabledButton = cohesionButton;
    }

    public void ShowSeparation()
    {
        ClearGraph();
        disabledButton.interactable = true;
        separationButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.SEPARATE_WEIGHT]);
        disabledButton = separationButton;
    }

    public void ShowAvoidance()
    {
        ClearGraph();
        disabledButton.interactable = true;
        avoidanceButton.interactable = false;
        ShowGraph(data[(int)DataIndexingEnum.AVOID_COLLISION_WEIGHT]);
        disabledButton = avoidanceButton;
    }


    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private void HideTooltip()
    {
        tooltipGameObject.SetActive(false);
    }

    private void ShowGraph(List<float> valueList)
    {
        if (valueList == null || valueList.Count == 0)
        {
            return;
        }
        float graphHeight = graphContainer.sizeDelta.y;
        float maxValue = valueList.Max();
        float minValue = valueList.Min();
        int labelSpacerX = Round(valueList.Count / separatorCountX);
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = ((float)i / valueList.Count) * graphContainer.sizeDelta.x;
            float yPosition = (valueList[i] - minValue) / (maxValue - minValue) * graphHeight;

            string tooltipText = valueList[i].ToString();

            GameObject circle = CreateCircle(new Vector2(xPosition, yPosition));
            if (points.Count != 0)
            {
                CreateDotConnection(points.Last().GetComponent<RectTransform>().anchoredPosition, circle.GetComponent<RectTransform>().anchoredPosition);
            }
            points.Add(circle);

            if (valueList.Count >= this.separatorCountX)
            {
                if (i % labelSpacerX == 0)
                {
                    CreateLabelX(xPosition, (i * 5).ToString());
                    CreateDashX(xPosition);
                }
            }
            else
            {
                CreateLabelX(xPosition, (i * 5).ToString());
                CreateDashX(xPosition);
            }
        }

        for (int i = 0; i < separatorCountY; i++)
        {
            float normalizedValue = i * 1f / separatorCountY;
            CreateLabelY(normalizedValue * graphHeight, (normalizedValue * maxValue + minValue).ToString());
            CreateDashY(normalizedValue * graphHeight);
        }
    }

    private int Round(int n)
    {
        int a = (n / 10) * 10;
        int b = a + 10;
        return (n - a > b - n) ? b : a;
    }

    private void CreateLabelX(float position, string text)
    {
        RectTransform labelX = Instantiate(labelTemplateX);
        labelX.SetParent(graphContainer, false);
        labelX.gameObject.SetActive(true);
        labelX.anchoredPosition = new Vector2(position, -7f);
        labelX.GetComponent<Text>().text = text;
        labelsX.Add(labelX.gameObject);
    }

    private void CreateDashX(float position)
    {
        RectTransform dashX = Instantiate(dashTemplateX);
        dashX.SetParent(graphContainer, false);
        dashX.gameObject.SetActive(true);
        dashX.anchoredPosition = new Vector2(position, 0);
        dashesX.Add(dashX.gameObject);
    }

    private void CreateDashY(float position)
    {
        RectTransform dashY = Instantiate(dashTemplateY);
        dashY.SetParent(graphContainer, false);
        dashY.gameObject.SetActive(true);
        dashY.anchoredPosition = new Vector2(0, position);
        dashesY.Add(dashY.gameObject);
    }

    private void CreateLabelY(float position, string text)
    {
        RectTransform labelY = Instantiate(labelTemplateY);
        labelY.SetParent(graphContainer, false);
        labelY.gameObject.SetActive(true);
        labelY.anchoredPosition = new Vector2(-7f, position);
        labelY.GetComponent<Text>().text = text;
        labelsY.Add(labelY.gameObject);
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
        lines.Add(gameObject);
    }

    private void ClearGraph()
    {
        foreach (var point in this.points)
        {
            Destroy(point);
        }
        foreach (var line in this.lines)
        {
            Destroy(line);
        }
        foreach (var label in this.labelsX)
        {
            Destroy(label);
        }
        foreach (var label in this.labelsY)
        {
            Destroy(label);
        }
        foreach (var dash in this.dashesX)
        {
            Destroy(dash);
        }
        foreach (var dash in this.dashesY)
        {
            Destroy(dash);
        }

        this.points.Clear();
        this.lines.Clear();
        this.labelsX.Clear();
        this.labelsY.Clear();
        this.dashesX.Clear();
        this.dashesY.Clear();
    }
}
