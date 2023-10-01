using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else Destroy(gameObject);
    }

    [SerializeField] SpriteRenderer _boundsRenderer;
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

    float _gameSize = 10f;
    public float GameSize { get { return _gameSize + 3f;  } }

    public void Clean()
    {
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);

        foreach (Faction faction in _factions) faction.Erase();
        _factions.Clear();

        _planets.Clear();

        _playerFaction = null;

        /*Camera.main.orthographicSize = maxZoom;
        Camera.main.orthographicSize = minZoom;
        _targetZoom = minZoom;
        _dragTargetPosition = Camera.main.transform.position;*/
    }

    void Start()
    {
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        Camera.main.orthographicSize = maxZoom;
        Camera.main.orthographicSize = minZoom;
        _targetZoom = minZoom;
        _dragTargetPosition = Camera.main.transform.position;
    }

    [SerializeField] GameObject _winPanel;
    [SerializeField] GameObject _losePanel;
    


    Vector3 _lastDrag = Vector3.zero;
    Vector3 _startDragPosition;
    Vector3 _dragTargetPosition;
    void Update()
    {
        if (_playerFaction == null) return;
        
        
        if(_planets.Count > 0 && _playerFaction.Planets.Count >= _planets.Count && IsWin())
        {
            Win();
        }
        else
        {
            _winPanel.SetActive(false);
        }
        if (_playerFaction.Beings.Count <= 1 && _playerFaction.Ships.Count <= 0)
        {
            Lose();
        }

        Time.timeScale = _timeScale;

        if(Input.GetKeyUp(KeyCode.Space)) {
            LevelManager.Instance.LoadCurrentLevel();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            LevelManager.Instance.LoadNextLevel();
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
            if(_dragTargetPosition.magnitude > GameSize)
            {
                _dragTargetPosition = _dragTargetPosition.normalized * GameSize;
                _dragTargetPosition.z = -10f;
            }
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

        foreach (Faction faction in _factions) faction.Erase();
        _factions.Clear();
        foreach (Planet planet in _planets) planet.Erase();
        _planets.Clear();

        factionCount = Mathf.Min(_scriptableFactions.Count, factionCount);
        List<ScriptableFaction> sfactions = new List<ScriptableFaction>(_scriptableFactions);
        sfactions.Shuffle();
        for (int i = 0; i < factionCount; i++)
        {
            Faction faction = Instantiate(_factionPrefab, _factionContainer);
            faction.Load(sfactions[i]);
            _factions.Add(faction);
            if (i > 0) faction.SetAI(true);
        }
        _playerFaction = _factions[0];

        Camera.main.transform.position = new Vector3(0, 0, -10);
        
        _startingPlanet = GeneratePlanet(Vector3.right);
        _startingPlanet.SetStatic();
        _startingPlanet.AddBeing(_playerFaction);
        _startingPlanet.AddBeing(_playerFaction);
        _startingPlanet.AddBeing(_playerFaction);

        for (int i = 0; i < planetCount; i++)
        {
            Planet p = GenerateRandomPlanet();
            if (i < _factions.Count && _factions[i] != _playerFaction)
            {
                p.AddBeing(_factions[i]);
                p.AddBeing(_factions[i]);
                p.AddBeing(_factions[i]);
            }   
        }

        _targetZoom = Camera.main.orthographicSize;
        StartCoroutine(ScheduledCalculateBounds());

    }

    public Planet GenerateRandomPlanet()
    {
        Vector3 position = new Vector3(Random.value * _gameSize, Random.value * _gameSize, 0);
        return GeneratePlanet(position, Random.value * 2f + 1f);
    }

    public Planet GeneratePlanet(Vector3 position, float size = 1f)
    {
        Planet newPlanet = Instantiate(_planetPrefab);
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
    public Planet GetFarthestPlanet(Vector2 position)
    {
        _planets.Sort((Planet a, Planet b) => Mathf.FloorToInt(b.transform.position.DistanceTo(position) - a.transform.position.DistanceTo(position)));
        return _planets[0];
    }

    public void StartLevel(Level level)
    {
        if(level.Planets.Count == 0)
        {
            StartGame();
            return;
        }
        foreach (ScriptableFaction sfaction in level.Factions)
        {
            Faction faction = Instantiate(_factionPrefab, _factionContainer);
            faction.Load(sfaction);
            _factions.Add(faction);
            if (_playerFaction == null) _playerFaction = faction;
            else faction.SetAI(true);
        }

        _startingPlanet = level.Planets[0];
        _planets = level.Planets;

        foreach(Planet p in _planets)
        {
            p.SetSize(p.transform.localScale.x);
            p.Init();
        }


        for(int i = 0; i < _factions.Count; i++)
        {
            level.Planets[i].AddBeing(_factions[i]);
            level.Planets[i].AddBeing(_factions[i]);
            level.Planets[i].AddBeing(_factions[i]);
        }
        CalculateBounds();
    }

    public IEnumerator ScheduledCalculateBounds()
    {
        yield return new WaitForSeconds(2f);
        CalculateBounds();
    }

    public void CalculateBounds()
    {
        Vector3 center = Vector3.zero;
        foreach(Planet planet in _planets)
        {
            center += planet.transform.position;
        }
        center /= _planets.Count;
        
        foreach(Planet planet in _planets)
        {
            planet.transform.position -= center;
        }

        Planet farthest = GetFarthestPlanet(Vector2.zero);
        float distance = farthest.transform.position.magnitude;

        _gameSize = distance;
        _boundsRenderer.transform.localScale = Vector3.one * GameSize * 2f;

        Camera.main.transform.position = new Vector3(_startingPlanet.transform.position.x, _startingPlanet.transform.position.y, -10);
        _dragTargetPosition = Camera.main.transform.position;
        _targetZoom = Camera.main.orthographicSize;
    }

    public void Win() {
        _winPanel.SetActive(true);
    }
    public void Lose()
    {
        _losePanel.SetActive(true);
    }

    bool IsWin()
    {
        foreach (Faction faction in _factions)
        {
            if (faction == _playerFaction) continue;
            if (!faction.IsDead()) return false;
        }
        return true;
    }
}
