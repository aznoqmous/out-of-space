using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Being : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Transform _root;
    [SerializeField] Transform _container;
    [SerializeField] CollisionCheck _collisionCheck;

    float _breedCooldown = 1f;
    float _lastBreed = 0f;

    Faction _faction;
    public Faction Faction { get { return _faction; } }

    float _direction = 1f;
    public float Direction { get { return _direction; } }

    public void CollideWith(Being being)
    {
        if (being.Faction == Faction) Breed(being);
        else Erase();
    }

    public void Breed(Being being)
    {
        if (_planet.IsUnderAttack) return;

        if (being.Direction == Direction) return;
        if (Time.time - _lastBreed < _breedCooldown) return;
        if (_planet.Population < _planet.MaxCapacity)
        {
            being.SetLastBreed();
            SetLastBreed();
            _planet.AddBeing(Faction);
        }
    }

    Planet _planet;
    public void SetPlanet(Planet planet)
    {
        _planet = planet;
    }

    public void SetFaction(Faction faction)
    {
        _faction = faction;
        SetSprite(_faction.BeingSprite);
        _spriteRenderer.color = faction.Color;
    }

    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    public void SetScale(float scale)
    {
        _container.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetDirection(float direction)
    {
        _direction = direction;
    }

    public void SetAngle(float angle)
    {
        _root.transform.localEulerAngles = new Vector3(0, 0, angle);
    }
    public void SetLastBreed()
    {
        _lastBreed = Time.time;
    }

    bool _erased = false;
    public void Erase()
    {
        if (_erased) return;
        _erased = true;
        _planet.RemoveBeing(this);
        Destroy(gameObject);
    }

    private void Start()
    {
        _spriteRenderer.transform.localScale = Vector3.zero;
        _collisionCheck.Disable();
    }

    void Update()
    {
        _spriteRenderer.transform.localScale = Vector3.Lerp(_spriteRenderer.transform.localScale, new Vector3(0.5f, 0.5f, 1f), Time.deltaTime);
        _root.transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + Time.deltaTime * _direction * 20f);
        if (_spriteRenderer.transform.localScale.x > 0.49f) _collisionCheck.Enable();
    }
}
