using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealTimeLatencyChart : MonoBehaviour
{
    [Header("Latency Settings")]
    public int maxBars = 100;
    public Vector2 chartSize = new Vector2(1000, 400);
    public float maxLatency = 200f;

    [Header("Thresholds (ms)")]
    public float orangeThreshold = 100f;
    public float redThreshold = 140f;

    private RectTransform graphPanel;
    private List<RectTransform> bars = new List<RectTransform>();
    private List<Image> barImages = new List<Image>();
    private float barWidth;

    void Start()
    {
        InitUI();
    }

    void InitUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // canvasGO.AddComponent<GraphicRaycaster>();
        canvas.sortingOrder = 1;
        // Panel
        GameObject panelGO = new GameObject("GraphPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform);
        graphPanel = panelGO.GetComponent<RectTransform>();
        graphPanel.sizeDelta = chartSize;
        graphPanel.anchorMin = new Vector2(0f, 0f);   // ?? Aligné en bas à gauche
        graphPanel.anchorMax = new Vector2(0f, 0f);
        graphPanel.pivot = new Vector2(0f, 0f);
        graphPanel.anchoredPosition = Vector2.zero;
        panelGO.GetComponent<Image>().color = new Color(0, 0, 0, 0); // transparent

        // Bar layout
        barWidth = chartSize.x / maxBars;

        for (int i = 0; i < maxBars; i++)
        {
            GameObject barGO = new GameObject("Bar_" + i, typeof(RectTransform), typeof(Image));
            barGO.transform.SetParent(graphPanel, false);

            RectTransform rt = barGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(barWidth - 1f, 3f);
            rt.anchoredPosition = new Vector2(i * barWidth, 0); // ?? plus de centrage

            Image img = barGO.GetComponent<Image>();
            img.color = Color.green;

            bars.Add(rt);
            barImages.Add(img);
        }
    }

    /// <summary>
    /// Appelle cette méthode avec la latence que tu veux afficher (à chaque tick réseau par exemple)
    /// </summary>
    public void AddLatencyValue(float latency)
    {
        float height = 0;

        if (latency < 0f)
            height = 10;
        else
            height = Mathf.Clamp01(latency / maxLatency) * chartSize.y;

        // Recycler la première barre
        RectTransform oldBar = bars[0];
        Image oldImage = barImages[0];

        bars.RemoveAt(0);
        barImages.RemoveAt(0);

        bars.Add(oldBar);
        barImages.Add(oldImage);

        // Hauteur
        oldBar.sizeDelta = new Vector2(barWidth - 1f, height);

        // Couleur dynamique
        oldImage.color = GetColorForLatency(latency);

        // Position (aligné à gauche)
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i].anchoredPosition = new Vector2(i * barWidth, 0);
        }
    }

    Color GetColorForLatency(float latency)
    {
        if (latency == -1)
            return Color.gray;
        else if (latency >= redThreshold)
            return Color.red;
        else if (latency >= orangeThreshold)
            return new Color(1f, 0.5f, 0f); // orange
        else
            return Color.green;
    }
}
