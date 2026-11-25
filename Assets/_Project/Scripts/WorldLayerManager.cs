using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerManager : MonoBehaviour
{
    public static WorldLayerManager Instance { get; private set; }
    public static event Action<WorldColor> ColorChanged;

    [SerializeField] private WorldColor startColor = WorldColor.Blue;

    private WorldColor currentColor;

    private readonly List<ColorFloor> blueFloors = new List<ColorFloor>();
    private readonly List<ColorFloor> redFloors = new List<ColorFloor>();

    public WorldColor CurrentColor => currentColor;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentColor = startColor;
    }

    void Start()
    {
        RefreshFloorsInScene();
        Apply();
    }

    public void RefreshFloorsInScene()
    {
        blueFloors.Clear();
        redFloors.Clear();

        // 호환성 높은 방식
        var all = FindObjectsOfType<ColorFloor>(true);
        foreach (var f in all)
        {
            if (f.floorColor == WorldColor.Blue) blueFloors.Add(f);
            else redFloors.Add(f);
        }
    }

    public void SetColor(WorldColor color)
    {
        if (currentColor == color) return;
        currentColor = color;
        Apply();
    }

    private void Apply()
    {
        bool blueOn = currentColor == WorldColor.Blue;

        foreach (var f in blueFloors)
            if (f) f.SetActiveCollision(blueOn);

        foreach (var f in redFloors)
            if (f) f.SetActiveCollision(!blueOn);

        ColorChanged?.Invoke(currentColor);
    }
}
