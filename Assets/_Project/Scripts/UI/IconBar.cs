using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconBar : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private GameObject iconPrefab; // Префаб одной иконки (Image)
    [SerializeField] private int maxIcons = 10; // Сколько всего иконок (например, 10 сердец)

    [Header("Sprites")] [SerializeField] private Sprite fullIcon; // Картинка (Красное сердце / Полная броня)
    [SerializeField] private Sprite icon2Of5;
    [SerializeField] private Sprite icon3Of5;
    [SerializeField] private Sprite icon4Of5;
    [SerializeField] private Sprite emptyIcon; // Картинка (Серое сердце / Пустая броня)

    private List<Image> icons = new List<Image>();

    private void Start()
    {
        InitializeBar();
    }

    private void InitializeBar()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        icons.Clear();

        for (int i = 0; i < maxIcons; i++)
        {
            GameObject newIcon = Instantiate(iconPrefab, transform);
            Image imgComponent = newIcon.GetComponent<Image>();

            imgComponent.sprite = fullIcon;
            icons.Add(imgComponent);
        }

        UpdateBar(0.82f);
    }

    public void UpdateBar(float percent)
    {
        percent = Mathf.Clamp01(percent);
        int activeCount = Mathf.RoundToInt(percent * maxIcons);
        float lastStage = percent * maxIcons - activeCount;

        for (int i = 0; i < icons.Count; i++)
            icons[i].sprite = (i < activeCount) ? fullIcon : emptyIcon;

        if (activeCount < icons.Count)
        {
            if (lastStage < Mathf.Epsilon) icons[activeCount].sprite = emptyIcon;
            else if (lastStage < 0.25) icons[activeCount].sprite = icon4Of5;
            else if (lastStage < 0.5) icons[activeCount].sprite = icon3Of5;
            else if (lastStage < 0.75) icons[activeCount].sprite = icon2Of5;
            else icons[activeCount].sprite = fullIcon;
        }
    }
}