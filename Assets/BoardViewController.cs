using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BoardViewController : MonoBehaviour
{
    private List<RowController> _rows = new List<RowController>();
    private AudioSource _audioSource;

    public List<RowController> Rows { get => _rows; set => _rows = value; }
    private Sequence _sequenceHide;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init();
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void GetFieldPos(GameObject _field, out int _x, out int _y)
    {
        _x = 0;
        _y = 0;
        for (int i = 0; i < _rows.Count; i++)
        {
            RowController tempRow = _rows[i].GetComponent<RowController>();
            for (int j = 0; j < tempRow.Fields.Count; j++)
            {
                if (_field.GetInstanceID() == tempRow.Fields[j].GetInstanceID())
                {
                    _y = i;
                    _x = j;
                    break;
                }
            }
        }
    }

    internal void ActivateField(GameObject _field)
    {
        FieldContainer tempFieldContainer = _field.GetComponent<FieldContainer>();
        tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);

    }

    internal void ActivateField(GameObject _field, bool _value)
    {

        FieldContainer tempFieldContainer = _field.GetComponent<FieldContainer>();
        tempFieldContainer.BorderImage.gameObject.SetActive(_value);
    }

    internal void ActivateField(int y, int x, bool v)
    {
        FieldContainer tempFieldContainer = _rows[x].Fields[y].GetComponent<FieldContainer>();
        tempFieldContainer.BorderImage.gameObject.SetActive(false);
    }

    internal void SwapSprite(GameObject _field, Sprite _sprite)
    {
        _field.GetComponent<FieldContainer>().IconImage.sprite = _sprite;
    }

    internal void SwapSprite(int y, int x, Sprite _sprite)
    {
        _rows[y].Fields[x].GetComponent<FieldContainer>().IconImage.sprite = _sprite;
    }

    internal async Task RunDotweenAsync(List<(int, int)> _toRemove)
    {
        _sequenceHide = DOTween.Sequence();
        foreach ((int, int) cord in _toRemove)
        {
            _sequenceHide.Join(_rows[cord.Item2].Fields[cord.Item1].GetComponent<FieldContainer>().IconImage.DOFade(0, 0.25f));
        }
        await _sequenceHide.Play().AsyncWaitForCompletion();
    }

    internal void FadeField(int y, int x)
    {
        _rows[y].Fields[x].GetComponent<FieldContainer>().IconImage.DOFade(1, 0.25f);
    }

    internal void PlaySound(AudioClip sound)
    {
        _audioSource.PlayOneShot(sound);
    }
}
