using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Faction: MonoBehaviour
{
    ScriptableFaction _scriptable;

    public string Name { get { return _scriptable.Name; } }
    public Color Color { get { return _scriptable.Color; } }

    public Sprite BeingSprite { get { return _scriptable.BeingSprite; } }
    public Sprite ShipSprite { get { return _scriptable.ShipSprite; } }
    public Sprite HouseSprite { get { return _scriptable.HouseSprite; } }
    public TMP_FontAsset Font { get { return _scriptable.Font; } }
    public List<string> PlanetConqueredDialog {get {return _scriptable.PlanetConqueredDialog;}}
    public List<string> PlanetLoosedDialog {get {return _scriptable.PlanetLoosedDialog;}}
    public List<string> DeadDialog {get {return _scriptable.DeadDialog;}}
    public List<string> TauntDialog {get {return _scriptable.TauntDialog;}}
    public Sprite LordSprite { get { return _scriptable.LordSprite; } }
    public Sprite LordDeadSprite { get { return _scriptable.LordDeadSprite; } }


    public float DialogPitch { get { return _scriptable.DialogPitch; } }

    List<Being> _beings = new List<Being>();
    List<Planet> _planets = new List<Planet>();
    List<Ship> _ships = new List<Ship>();
    public List<Being> Beings { get { return _beings; } }
    public List<Planet> Planets { get { return _planets; } }
    public List<Ship> Ships { get { return _ships; } }

    [SerializeField] TextMeshProUGUI _planetsCount;
    [SerializeField] TextMeshProUGUI _beingsCount;
    [SerializeField] TextMeshProUGUI _shipsCount;
    [SerializeField] Image _planetImage;
    [SerializeField] List<Image> _beingImages;
    [SerializeField] Image _shipImage;
    [SerializeField] Image _lordImage;

    [SerializeField] Dialog _dialogPrefab;
    [SerializeField] Transform _dialogContainer;

    bool _isAI = false;
    AIBehaviour _behaviour = AIBehaviour.Balanced;

    bool _isDead = false;
    public bool IsDead()
    {
        return _beings.Count <= 1 && _ships.Count <= 0;
    }

    public void Load(ScriptableFaction scriptable)
    {
        _scriptable = scriptable;
        _planetImage.color = Color;
        _shipImage.sprite = ShipSprite;
        _shipImage.color = Color;
        _lordImage.sprite = scriptable.LordSprite;
        _lordImage.SetNativeSize();

        foreach (Image image in _beingImages)
        {
            image.sprite = BeingSprite;
            image.color = Color;
        }

    }

    IEnumerator Hurt()
    {
        _lordImage.sprite = LordDeadSprite;
        yield return new WaitForSeconds(1f);
        if (!_isDead) _lordImage.sprite = LordSprite;
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
        SpeakPlanetConqueredDialog();
        _planets.Add(planet);
        _planetsCount.text = $"{_planets.Count}";
    }

    public void RemovePlanet(Planet planet)
    {
        SpeakPlanetLoosedDialog();
        _planets.Remove(planet);
        _planetsCount.text = $"{_planets.Count}";
        StartCoroutine(Hurt());
    }

    public void AddShip(Ship ship)
    {
        _ships.Add(ship);
        _shipsCount.text = $"{_ships.Count}";
    }

    public void RemoveShip(Ship ship)
    {
        _ships.Remove(ship);
        _shipsCount.text = $"{_ships.Count}";
    }

    private void Update()
    {
        if(_isAI) {
            HandleAI();
        }
        if(!_isDead && IsDead())
        {
            SpeakDeadDialog();
            StartCoroutine(Hurt());
            _isDead = true;
        }
    }

    float _lastCycle = 0f;
    float _cycleCooldown = 10f;
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
                    float amount = Mathf.Max(Mathf.Min(0.5f * activePlanets[0].MaxCapacity, activePlanets[0].MaxCapacity - 1f), 2f);
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
                        float amount = Mathf.Max(Mathf.Min(0.5f * activePlanets[0].MaxCapacity, activePlanets[0].MaxCapacity - 1f), 2f);
                        if (planet.Population <= amount)
                        {
                            activePlanets[0].SpawnShip();
                            activePlanets[0].RotateShip(activePlanets[0].transform.position.Angle(planet.transform.position));
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
    }

    public void Erase()
    {
        Destroy(gameObject);
    }

    public List<string> _tutorials = new List<string>();
    public int _currentTutorial = 0;
    public void SetTutorials(List<string> tutorials)
    {
        _currentTutorial = 0;
        _tutorials = tutorials;
    }

    public void NextTutorial()
    {
        if (_currentTutorial < _tutorials.Count) Speak(_tutorials[_currentTutorial], true);
        else _isTutorialOver = true;
        _currentTutorial++;
    }

    static float LastDialog = 0f;
    float _minInterval = 2f;
    bool _isTutorialOver = false;
    public void Speak(string text, bool isTutorial=false)
    {
        if (!isTutorial && this == GameManager.Instance.PlayerFaction && !_isTutorialOver) return;
        if (!isTutorial && Time.time - LastDialog < _minInterval) return;
        Dialog _dialog = Instantiate(_dialogPrefab, _dialogContainer);
        _dialog.SetFont(Font);
        _dialog.SetText(text);
        _dialog.SetFaction(this);
        _dialog.SetIsTutorial(isTutorial);
        LastDialog = Time.time;
    }

    

    public void DialogClick(string text, bool isTutorial=false)
    {
        if(isTutorial)
        {
            NextTutorial();
        }
    }

    public void SpeakPlanetConqueredDialog()
    {
        Speak(PlanetConqueredDialog.PickRandom());
    }

    public void SpeakPlanetLoosedDialog()
    {
        Speak(PlanetLoosedDialog.PickRandom());
    }

    public void SpeakDeadDialog()
    {
        Speak(DeadDialog.PickRandom());
    }

    public void SpeakTauntDialog()
    {
        Speak(TauntDialog.PickRandom());
    }
}