using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] Rigidbody2D _rigidbody;
    [SerializeField] Being _beingPrefab;
    [SerializeField] SpriteRenderer _selection;
    [SerializeField] SpriteRenderer _innerSelection;
    [SerializeField] List<Sprite> _planetSprites = new List<Sprite>();

    Faction _faction;
    public Faction Faction { get { return _faction; } }

    float _population = 0f;
    public float Population { get { return _population;  } }
    float _size = 1f;
    public float MaxCapacity { get { return Mathf.FloorToInt(_size * 8f);  } }
    public float CapacityRatio { get { return Population / MaxCapacity; } }
    float _birth;
    public void Init()
    {
        _birth = Time.time;
        foreach (Faction faction in GameManager.Instance.Factions) _beings.Add(faction, new List<Being>());
        ShowSelection(false);
        _spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, Random.value * 360f);
        _spriteRenderer.sprite = _planetSprites.PickRandom();
    }

    public Dictionary<Faction, List<Being>> _beings = new Dictionary<Faction, List<Being>>();

    public Color TargetColor { get {
            return Faction == null ? Color.white : Faction.Color;
        } }
    
    public void Update()
    {
        if(Time.time - _birth > 1f && _rigidbody.velocity.sqrMagnitude <= 0.1f)
        {
            _collider.radius = 0.5f;
            SetStatic();
        }
        _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, TargetColor, Time.deltaTime);

    }
    public bool IsPlayer()
    {
        return _faction == GameManager.Instance.PlayerFaction;
    }

    public void SetFaction(Faction faction)
    {
        _faction = faction;
    }

    public void SetSize(float size)
    {
        _size = size;
        transform.localScale = new Vector3(_size, _size, _size);
    }

    public void SetStatic(bool isStatic = true)
    {
        _rigidbody.bodyType = isStatic ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
    }

    public void Erase()
    {
        Destroy(gameObject);
    }

    bool _underAttack = false;
    public bool IsUnderAttack { get { return _underAttack; } }
    public void UpdateFaction()
    {
        Faction lastFaction = _faction;
        _population = 0f;
        _faction = null;
        _underAttack = false;
        float factionCount = 0f;
        foreach (Faction f in _beings.Keys)
        {
            if (_faction != null && _beings[f].Count > 0f) _underAttack = true;
            if (_beings[f].Count > factionCount)
            {
                _faction = f;
                factionCount = _beings[f].Count;
            }
        }
        if (lastFaction != _faction)
        {
            if (lastFaction != null) lastFaction.RemovePlanet(this);
            SetFaction(_faction);
            if (_faction != null) _faction.AddPlanet(this);
        }
        
    }

    public void UpdatePopulation()
    {
        _population = _faction != null ? _beings[_faction].Count : 0;
        float direction = 1f;
        foreach(List<Being> beings in _beings.Values)
        {
            foreach (Being being in beings)
            {
                direction *= -1f;
                being.SetDirection(direction);
            }
        }
    }

    float _lastDirection = 1f;
    public void AddBeing(Faction f, float startAngle=-999f)
    {
        if (_beings[f].Count >= MaxCapacity) return;

        Being newBeing = Instantiate(_beingPrefab, transform);
        newBeing.SetFaction(f);
        _beings[f].Add(newBeing);
        f.AddBeing(newBeing);
        newBeing.SetScale(1/_size);
        _lastDirection = _lastDirection * -1f;
        newBeing.SetDirection(_lastDirection);
        if (startAngle == -999f) startAngle = Random.value * 360f;
        newBeing.SetAngle(startAngle);
        newBeing.SetPlanet(this);
        UpdateFaction();
        UpdatePopulation();
    }

    public void RemoveBeing(Being b)
    {
        b.Faction.RemoveBeing(b);
        _beings[b.Faction].Remove(b);
        b.Erase();
        UpdateFaction();
        UpdatePopulation();
    }

    bool _isSelected = false;
    float _amountSelected = 0;
    public bool IsSelected { get { return _isSelected; } }
    void OnMouseDown()
    {
        if (!IsUnderAttack && IsPlayer() && !GameManager.Instance.HasSelectedPlanet())
        {
            GameManager.Instance.SelectPlanet(this);
            SpawnShip();
            ShowSelection();
        }
    }
    public void Unselect()
    {
        RemoveShip();
        _isSelected = false;
        _amountSelected = 0f;
    }
    public void Select(float amount)
    {
        _isSelected = true;
        _amountSelected = amount;
        ShowSelection(false);

    }

    [SerializeField] Ship _shipPrefab;
    Ship _currentShip;
    public Ship SpawnShip()
    {
        Ship newShip = Instantiate(_shipPrefab, GameManager.Instance.transform);
        _currentShip = newShip;
        newShip.SetFaction(Faction);
        RotateShip(Mathf.PI/2);
        return newShip;
    }
    public void RotateShip(float angle = 0f)
    {
        _currentShip.transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg - 90);
        _currentShip.transform.position = transform.position + 
            new Vector3(
            Mathf.Cos(angle) * _size / 2f, 
            Mathf.Sin(angle) * _size / 2f, 
            0f
        );
    }
    public void RemoveShip()
    {
        _currentShip.Erase();
    }
    public void LaunchShip()
    {
        _isSelected = false;
        GameManager.Instance.UnselectPlanet(this);
        _currentShip.Launch();
        _currentShip.SetCrewCount(_amountSelected);
        for (int i = 0; i < _amountSelected; i++) RemoveBeing(_beings[_faction][0]);
    }

    public void ShowSelection(bool state=true)
    {
        _selection.color =_faction != null ? _faction.Color : Color.white;
        _innerSelection.color = _faction != null ?_faction.Color : Color.white;
        _selection.enabled = state;
        _innerSelection.enabled = state;
    }
    public void SetInnerSelectionScale(float scale)
    {
        _innerSelection.transform.localScale = new Vector3(scale, scale, scale);
    }

    public List<Planet> GetNearestEmptyPlanets()
    {
        List<Planet> planets = GameManager.Instance.Planets.Where((Planet a) => a.Faction == null).ToList();
        planets.Sort((Planet a, Planet b) => Mathf.FloorToInt(a.transform.position.DistanceTo(transform.position) - b.transform.position.DistanceTo(transform.position)));
        return planets;
    }
    public List<Planet> GetNearestEnemyPlanets()
    {
        List<Planet> planets = GameManager.Instance.Planets.Where((Planet a) => a.Faction != Faction).ToList();
        planets.Sort((Planet a, Planet b) => Mathf.FloorToInt(a.transform.position.DistanceTo(transform.position) - b.transform.position.DistanceTo(transform.position)));
        return planets;
    }



}