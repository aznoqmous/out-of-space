using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] float _gameHeight = 50f;
    [SerializeField] float _gameWidth = 50f;
    [SerializeField] float _timeScale = 2f;
    float minZoom = 3f;
    float maxZoom = 15f;

    [SerializeField] Planet _planetPrefab;
    [SerializeField] Planet _startingPlanet;
    List<Planet> _planets = new List<Planet>();
    public List<Planet> Planets { get { return _planets; } }

    [SerializeField] Faction _factionPrefab;
    [SerializeField] Transform _factionContainer;
    [SerializeField] List<ScriptableFaction> _scriptableFactions = new List<ScriptableFaction>();
    List<Faction> _factions = new List<Faction>();
    public List<Faction> Factions { get { return _factions; } }
    Faction _playerFaction;
    public Faction PlayerFaction { get { return _playerFaction; } }

    float _targetZoom = 5f;
    float _currentZoom = 5f;
    public float CurrentZoom { get { return _currentZoom;  } }

    [SerializeField] TextMeshProUGUI _cursorBeingCountText;
    [SerializeField] RectTransform _cursorBeingCount;

    public Vector2 Bounds { get { return new Vector2(_gameWidth * 2.5f, _gameHeight * 2.5f); } }
    void Start()
    {
        Camera.main.orthographicSize = maxZoom;
        StartGame();
        Camera.main.orthographicSize = minZoom;
        _targetZoom = minZoom;
        _dragTargetPosition = Camera.main.transform.position;
    }

    Vector3 _lastDrag = Vector3.zero;
    Vector3 _startDragPosition;
    Vector3 _dragTargetPosition;
    void Update()
    {
        Time.timeScale = _timeScale;

        if(Input.GetKeyUp(KeyCode.Space)) {
            GenerateRandomPlanet();
        }
        if (Input.mouseScrollDelta.y > 0) _targetZoom /= 1.1f;
        if (Input.mouseScrollDelta.y < 0) _targetZoom *= 1.1f;
        _targetZoom = Mathf.Min(maxZoom, Mathf.Max(minZoom, _targetZoom));
        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * 2f);
        //transform.localScale = new Vector3(_currentZoom, _currentZoom, 1f);
        Camera.main.orthographicSize = _currentZoom;

        
        /* Right Click + Drag */
        if (Input.GetMouseButtonDown(1))
        {
            if (_selectedPlanet != null)
            {
                _selectedPlanet.Unselect();
                _cursorBeingCountText.text = "";
                UnselectPlanet(_selectedPlanet);
            }
            else
            {
                _lastDrag = Input.mousePosition;
                _startDragPosition = Camera.main.transform.position;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            _lastDrag = Vector3.zero;
        }
        if (_lastDrag != Vector3.zero)
        {
            Vector3 moved = _lastDrag - Input.mousePosition;
            _dragTargetPosition = _startDragPosition + moved / 200f * CurrentZoom; 
        }
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, _dragTargetPosition, Time.deltaTime * 2f);

        /* Planet actions */
        if (HasSelectedPlanet())
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 distance = mousePosition - _selectedPlanet.transform.position;
            float amount;

            if (_selectedPlanet.IsSelected)
            {
                _selectedPlanet.RotateShip(Mathf.Atan2(distance.y, distance.x));
            }
            else
            {
                amount = 
                    Mathf.FloorToInt(
                        Mathf.Min(
                        Mathf.Min(distance.sqrMagnitude - 100f, 100f) / 10f * _selectedPlanet.MaxCapacity + 1f,
                        _selectedPlanet.Population
                    ));
                _cursorBeingCountText.text = $"{amount}";
                _cursorBeingCount.position = Input.mousePosition;

                _selectedPlanet.SetInnerSelectionScale((amount / _selectedPlanet.Population * 0.8f) + 0.2f);
            }
            if (Input.GetMouseButtonUp(0))
            {
                _cursorBeingCountText.text = "";

                if (_selectedPlanet.IsSelected)
                {
                    _selectedPlanet.LaunchShip();
                }
                else
                {
                    amount =
                     Mathf.FloorToInt(
                         Mathf.Min(
                         Mathf.Min(distance.sqrMagnitude - 100f, 100f) / 10f * _selectedPlanet.MaxCapacity + 1f,
                         _selectedPlanet.Population
                     ));
                    //_selectedPlanet.SpawnShip();
                    //_selectedPlanet.RotateShip(Mathf.Atan2(distance.y, distance.x));
                    _selectedPlanet.Select(amount);
                    
                }
            }
        }
    }
    
    public void StartGame()
    {
        int planetCount = 10;
        int factionCount = 3;

        _factions.Clear();
        factionCount = Mathf.Min(_scriptableFactions.Count, factionCount);
        List<ScriptableFaction> sfactions = new List<ScriptableFaction>(_scriptableFactions);
        sfactions.Shuffle();
        for (int i = 0; i < factionCount; i++)
        {
            Faction faction = Instantiate(_factionPrefab, _factionContainer);
            faction.Load(_scriptableFactions[i]);
            _factions.Add(faction);
            if (i > 0) faction.SetAI(true);
        }
        _playerFaction = _factions[0];

        foreach (Planet planet in _planets) planet.Erase();
        _planets.Clear();

        Camera.main.transform.position = new Vector3(0, 0, -10);
        
        _startingPlanet = GeneratePlanet(Vector3.right);
        _startingPlanet.SetStatic();
        _startingPlanet.AddBeing(_playerFaction);
        _startingPlanet.AddBeing(_playerFaction);

        for (int i = 0; i < planetCount; i++)
        {
            Planet p = GenerateRandomPlanet();
            if (i < _factions.Count && _factions[i] != _playerFaction)
            {
                p.AddBeing(_factions[i]);
                p.AddBeing(_factions[i]);
            }   
        }

        _targetZoom = Camera.main.orthographicSize;

    }

    public Planet GenerateRandomPlanet()
    {
        Vector3 position = new Vector3(Random.value * _gameWidth - _gameWidth / 2f, Random.value * _gameHeight - _gameHeight / 2f, 0);
        return GeneratePlanet(position, Random.value * 2f + 1f);
    }

    public Planet GeneratePlanet(Vector3 position, float size = 1f)
    {
        Planet newPlanet = Instantiate(_planetPrefab, transform);
        newPlanet.SetSize(size);
        newPlanet.transform.position = position;
        newPlanet.Init();
        _planets.Add(newPlanet);
        return newPlanet;
    }

    Planet _selectedPlanet;
    public Planet SelectedPlanet { get { return _selectedPlanet; } }
    public void SelectPlanet(Planet planet)
    {
        _selectedPlanet = planet;
    }
    public bool HasSelectedPlanet()
    {
        return _selectedPlanet != null;
    }
    public void UnselectPlanet(Planet planet)
    {
        planet.ShowSelection(false);
        if (_selectedPlanet != planet) return;
        _selectedPlanet = null;
    }

    public Planet GetNearestPlanet(Vector2 position)
    {
        _planets.Sort((Planet a, Planet b) => Mathf.FloorToInt(a.transform.position.DistanceTo(position) - b.transform.position.DistanceTo(position)));
        return _planets[0];
    }
}
