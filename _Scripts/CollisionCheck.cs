using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] CircleCollider2D _collider;
    [SerializeField] Being _being;
    public Being Being { get { return _being; } }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionCheck col = collision.GetComponent<CollisionCheck>();
        if(col!= null)
        {
            Being.CollideWith(col.Being);
        }
    }

    public void Disable()
    {
        _collider.enabled= false;
    }
    public void Enable()
    {
        _collider.enabled= true;
    }
}
