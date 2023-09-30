using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Faction: MonoBehaviour
{
    ScriptableFaction _scriptable;

    public string Name { get { return _scriptable.Name; } }
    public Color Color { get { return _scriptable.Color; } }

    public Sprite BeingSprite { get { return _scriptable.BeingSprite; } }
    public Sprite ShipSprite { get { return _scriptable.ShipSprite; } }
    public Sprite HouseSprite { get { return _scriptable.HouseSprite; } }

    public List<Being> _beings = new List<Being>();
    public List<Planet> _planets = new List<Planet>();

    [SerializeField] TextMeshProUGUI _planetsCount;
    [SerializeField] TextMeshProUGUI _beingsCount;

    bool _isAI = false;
    AIBehaviour _behaviour = AIBehaviour.Balanced;

    public void Load(ScriptableFaction scriptable)
    {
        _scriptable = scriptable;
    }

    public void AddBeing(Being being)
    {
        _beings.Add(being);
        _beingsCount.text = $"{_beings.Count}";
    }

    public void RemoveBeing(Being being)
    {
        _beings.Remove(being);
        _beingsCount.text = $"{_beings.Count}";
    }

    public void AddPlanet(Planet planet)
    {
        _planets.Add(planet);
        _planetsCount.text = $"{_planets.Count}";
    }

    public void RemovePlanet(Planet planet)
    {
        _planets.Remove(planet);
        _planetsCount.text = $"{_planets.Count}";
    }

    private void Update()
    {
        if(_isAI) {
            HandleAI();
        }
    }

    float _lastCycle = 0f;
    float _cycleCooldown = 5f;
    void HandleAI()
    {
        if (Time.time - _lastCycle < _cycleCooldown) return;

        List<Planet> activePlanets = GetActivePlanets();
        if(activePlanets.Count <= 0)
        {
            _lastCycle = Time.time;
            return;
        }
        

        switch (_behaviour)
        {
            /*case AIBehaviour.Random:
                break;
            case AIBehaviour.Aggressive:
                break;
            case AIBehaviour.Balanced:
                break;
            case AIBehaviour.Defensive:
                break;*/
            default:
                List<Planet> nearestEmpty = activePlanets[0].GetNearestEmptyPlanets();
                if (nearestEmpty.Count > 0)
                {
                    float amount = Mathf.Max(Mathf.Min(0.5f * activePlanets[0].MaxCapacity, activePlanets[0].MaxCapacity), 2f);
                    activePlanets[0].SpawnShip();
                    activePlanets[0].RotateShip(activePlanets[0].transform.position.Angle(nearestEmpty[0].transform.position));
                    activePlanets[0].Select(amount);
                    activePlanets[0].LaunchShip();
                    _lastCycle = Time.time + UnityEngine.Random.value * _cycleCooldown;
                    return;
                }
                List<Planet> nearestEnemy = activePlanets[0].GetNearestEnemyPlanets();
                if(nearestEnemy.Count > 0)
                {
                    foreach(Planet planet in nearestEnemy)
                    {
                        float amount = Mathf.Max(Mathf.Min(0.5f * activePlanets[0].MaxCapacity, activePlanets[0].MaxCapacity), 2f);
                        if (planet.Population <= amount)
                        {
                            activePlanets[0].SpawnShip();
                            activePlanets[0].RotateShip(activePlanets[0].transform.position.Angle(nearestEmpty[0].transform.position));
                            activePlanets[0].Select(amount);
                            activePlanets[0].LaunchShip();
                            _lastCycle = Time.time + UnityEngine.Random.value * _cycleCooldown;
                            return;
                        }
                    }
                }
                break;
        }
        _lastCycle = Time.time + UnityEngine.Random.value * _cycleCooldown;
    }

    public List<Planet> GetActivePlanets()
    {
        float threshold = 0.8f;

        switch (_behaviour)
        {
            case AIBehaviour.Random:
                threshold = UnityEngine.Random.value;
                break;
            case AIBehaviour.Aggressive:
                threshold = 0.2f + UnityEngine.Random.value * 0.5f;
                break;
            case AIBehaviour.Balanced:
                threshold = 0.3f + UnityEngine.Random.value * 0.5f;
                break;
            case AIBehaviour.Defensive:
                threshold = 0.5f + UnityEngine.Random.value * 0.5f;
                break;
            default:
                break;
        }
        List<Planet> planets = _planets.Where((Planet a) => a.Population >= 2 && threshold < a.CapacityRatio).ToList();
        planets.Sort((Planet a, Planet b) => Mathf.FloorToInt(a.CapacityRatio - b.CapacityRatio));
        return planets;
    }

    public void SetAI(bool isAI)
    {
        _isAI = isAI;
        Array values = Enum.GetValues(typeof(AIBehaviour));
        _behaviour = (AIBehaviour)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        Debug.Log(_behaviour);
    }
    
}