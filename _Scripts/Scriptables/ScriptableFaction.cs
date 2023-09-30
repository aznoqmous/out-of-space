using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Faction")]
public class ScriptableFaction : ScriptableObject
{
    public string Name;
    public Color Color;
    public Sprite BeingSprite;
    public Sprite ShipSprite;
    public Sprite HouseSprite;
}

public enum AIBehaviour
{
    Random,
    Aggressive,
    Balanced,
    Defensive
}