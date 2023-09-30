using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Rigidbody2D _rigidBody;
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] ParticleSystem _particleSystem;
    Faction _faction;
    float _crewCount = 1;

    private void Start()
    {
        _particleSystem.Pause();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        
        Planet planet = collision.collider.GetComponent<Planet>();
        if (planet)
        {
            _particleSystem.Stop();
            Vector3 distance = planet.transform.position - transform.position;
            //float angle = Mathf.Atan2(distance.y, distance.x);
            for (int i = 0; i < _crewCount; i++)
            {
                planet.AddBeing(_faction);
            }
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
        ActivateThruster(100f);
        _collider.enabled = true;
        _particleSystem.Play();
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
        Vector2 bounds = GameManager.Instance.Bounds;
        Vector3 position = transform.position;
        if (position.x > bounds.x / 2f) position.x = -bounds.x/2f;
        if (position.x < -bounds.x / 2f) position.x = bounds.x/2f;
        if (position.y > bounds.y / 2f) position.y = -bounds.y/2f;
        if (position.y < -bounds.y / 2f) position.y = bounds.y/2f;
        transform.position = position;

        if(_rigidBody.velocity != Vector2.zero)
        {
            Planet planet = GameManager.Instance.GetNearestPlanet(transform.position);
            Vector3 distance = planet.transform.position - transform.position;
            if (distance.sqrMagnitude > 10f) return;
            float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(transform.localEulerAngles.z, angle) - 90;
            if (Mathf.Abs(delta) > 30f) return;

            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 10f);

            _rigidBody.velocity = Vector2.Lerp(_rigidBody.velocity, distance.normalized, Time.deltaTime * 10f);
        }
        
    }
}
