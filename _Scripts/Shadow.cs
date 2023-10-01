using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public Vector3 Offset = new Vector3(0.1f, 0.1f);
    public Material Material;
    GameObject _shadow;

    void Start()
    {
        _shadow = new GameObject("Shadow");
        _shadow.transform.parent = transform;
        _shadow.transform.localPosition = Offset;
        _shadow.transform.localRotation = Quaternion.identity;
        _shadow.transform.localScale = Vector3.one;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        SpriteRenderer sr = _shadow.AddComponent<SpriteRenderer>();
        sr.sprite = renderer.sprite;
        sr.material= Material;
        sr.sortingLayerName = renderer.sortingLayerName;
        sr.sortingOrder = -1;
    }

    void LateUpdate()
    {
        Vector2 v2 = (Vector2) Offset / transform.localScale;
        _shadow.transform.localPosition = v2.Rotate(-transform.eulerAngles.z);
        //_shadow.transform.eulerAngles = transform.eulerAngles;
    }
}
