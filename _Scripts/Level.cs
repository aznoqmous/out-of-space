using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public List<ScriptableFaction> Factions = new List<ScriptableFaction>();
    public List<Planet> Planets = new List<Planet>();
    [TextArea(15, 20)]
    public List<string> Tutorials = new List<string>();

    private void Start()
    {
        GameManager.Instance.StartLevel(this);
        GameManager.Instance.PlayerFaction.SetTutorials(Tutorials);
        GameManager.Instance.PlayerFaction.NextTutorial();
    }
}
