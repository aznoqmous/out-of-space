using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Faction")]
public class ScriptableFaction : ScriptableObject
{
    public string Name;
    public Color Color;
    public Sprite BeingSprite;
    public Sprite ShipSprite;
    public Sprite HouseSprite;
    public Sprite LordSprite;
    public Sprite LordDeadSprite;

    public TMP_FontAsset Font;
    public List<string> PlanetConqueredDialog;
    public List<string> PlanetLoosedDialog;
    public List<string> DeadDialog;
    public List<string> TauntDialog;
    public float DialogPitch = 1f;
}

public enum AIBehaviour
{
    Random,
    Aggressive,
    Balanced,
    Defensive
}