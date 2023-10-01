using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dialog : MonoBehaviour, IPointerClickHandler
{
    void Start()
    {
        if(!_isTutorial) StartCoroutine(ScheduleDestroy());
    }

    IEnumerator ScheduleDestroy()
    {
        yield return new WaitForSeconds(3);
    }

    Faction _faction;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] TextMeshProUGUI _shadowText;
    [SerializeField] GameObject _tutorialTriangle;
    public void SetText(string text)
    {
        _text.text = "";
        _shadowText.text = "";
        StartCoroutine(Write(text));
    }

    IEnumerator Write(string text)
    {
        foreach (char c in text)
        {
            yield return new WaitForSeconds(Random.value * 0.1f / 2f);
            _text.text += c;
            _shadowText.text += c;
            if (c == ' ') AudioManager.Instance.Play(SoundType.Speak, 0.5f, (2f * Random.value - 1f) * 0.1f + _faction.DialogPitch);
        }

        yield return new WaitForSeconds(2f);
        if (!_isTutorial) Destroy(gameObject);
    }

    public void SetFont(TMP_FontAsset fontAsset)
    {
        _text.font = fontAsset;
        _shadowText.font = fontAsset;
    }

    public void SetFaction(Faction faction)
    {
        _faction = faction;
    }

    bool _isTutorial = false;
    public void SetIsTutorial(bool isTutorial)
    {
        _tutorialTriangle.SetActive(isTutorial);
        _isTutorial = isTutorial;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if(_isTutorial)
        {
            Destroy(gameObject);
        }
        _faction.DialogClick(_text.text, _isTutorial);
    }
}
