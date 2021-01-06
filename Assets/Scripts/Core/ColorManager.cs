using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[ExecuteInEditMode]
public class ColorManager : MonoBehaviour
{
    public ColorItem[] Registry;

    public static ColorManager Instance;

    private void Start()
    {
        Instance = this;
    }

    public Color GetColor(string id)
    {
        var y = Registry.FirstOrDefault(x => x._ID == id)._Color;

        if (y != null)
            return y;
        else
            return Color.white;
    }
}

[System.Serializable]
public class ColorItem
{
    public string _ID;
    public Color _Color = Color.red;
}