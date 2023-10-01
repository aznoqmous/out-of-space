using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;

public class Ship : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Rigidbody2D _rigidBody;
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] ParticleSystem _particleSystem;
    Faction _faction;
    float _crewCount = 1;
    float _speed = 6f;
    private void Awake()
    {
        ShowParticles(false);
    }

    public void ShowParticles(bool state=true)
    {
        _particleSystem.gameObject.SetActive(state);
    }

    bool _landed = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        
        Planet planet = collision.collider.GetComponent<Planet>();
        if (planet)
        {
            ShowParticles(false);
            //Vector3 distance = planet.transform.position - transform.position;
            //float angle = Mathf.Atan2(distance.y, distance.x);
            for (int i = 0; i < _crewCount; i++)
            {
                planet.AddBeing(_faction);
            }
            _faction.RemoveShip(this);
            _landed = true;
        }
    }


    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    public void SetFaction(Faction faction)
    {
        _faction = faction;
        SetSprite(faction.ShipSprite);
        _spriteRenderer.color = faction.Color;
    }

    public void Erase()
    {
        Destroy(gameObject);
    }

    public void Launch()
    {
        ActivateThruster(_speed*30f);
        _collider.enabled = true;
        ShowParticles();
    }

    void ActivateThruster(float force=1f, float angle=-999f)
    {
        if (angle == -999f) angle = transform.localEulerAngles.z * Mathf.Deg2Rad;
        angle += Mathf.PI / 2;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        _rigidBody.AddForce(direction * force);
    }

    public void SetCrewCount(float crewCount)
    {
        _crewCount = crewCount;
    }

    private void Update()
    {
        if(_landed)
        {
            Color color = _spriteRenderer.color;
            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime);
            _spriteRenderer.color = color;
            _spriteRenderer.transform.localScale = Vector3.Lerp(_spriteRenderer.transform.localScale, Vector3.zero, Time.deltaTime);
            if (color.a <= 0.01f) Destroy(gameObject);
            return;
        }

        Vector3 position = transform.position;
        if (position.magnitude > GameManager.Instance.GameSize)
        {
            RotateTowardNearestPlanet(Time.deltaTime);
            return;
        }
       
        transform.position = position;

        if(_rigidBody.velocity != Vector2.zero)
        {
            Planet planet = GameManager.Instance.GetNearestPlanet(transform.position);
            Vector3 distance = planet.transform.position - transform.position;
            if (distance.sqrMagnitude > 10f) return;
            float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(transform.localEulerAngles.z, angle) - 90;
            if (Mathf.Abs(delta) > 30f) return;

            RotateTowardNearestPlanet(Time.deltaTime * 10f);
            /*Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 10f);

            _rigidBody.velocity = Vector2.Lerp(_rigidBody.velocity, distance.normalized*_speed, Time.deltaTime * 10f);*/
        }
        
    }

    public void RotateTowardNearestPlanet(float force)
    {
        Planet planet = GameManager.Instance.GetNearestPlanet(transform.position);
        RotateToward(planet.transform.position, force);
    }
    public void RotateToward(Vector3 position, float force)
    {
        Vector3 dir = position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, force);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle-90f, Vector3.forward), Time.deltaTime);

        float delta = Mathf.DeltaAngle(transform.localEulerAngles.z, angle) - 90;
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(0, 0, transform.localEulerAngles.z + delta), force);
        _rigidBody.velocity = Vector2.Lerp(_rigidBody.velocity, dir.normalized*_speed, force / 4f);
            
        //_rigidBody.velocity = Vector2.zero;


    }
}
