using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BoardManager
{
    private int[,] _itemIdArray;
    private (int?, int?) _currentActiveFIeld = (null, null); //current x and y
    private ItemType _currentFieldType = ItemType.Normal;

    //ZENJECT
    private BoardCompononets boardCompononets;
    private GameObject boardGameObject = null;
    private BoardViewController boardViewController;

    [Inject]
    public BoardManager(BoardCompononets _boardCompononets,Transform _targetParent)
    {
        boardCompononets = _boardCompononets;
        Init(_targetParent);
    }

    private void Init(Transform _targetParent)
    {
        boardGameObject = GameObject.Instantiate(boardCompononets.BoardPrefab);
        boardGameObject.transform.SetParent(_targetParent);
        boardViewController = boardGameObject.GetComponent<BoardViewController>();
        _itemIdArray = InitializeBoard(boardCompononets.Width, boardCompononets.Height, boardCompononets.RowPrefab, boardCompononets.FieldPrefab, boardCompononets.Items);

        Debug.Log(boardViewController);
    }
    internal int[,] InitializeBoard(int width, int height, GameObject rowPrefab, GameObject fieldPrefab, List<Item> items)
    {
        Debug.Log($"width {width} height {height}");
        int[,] itemIdArray = new int[width, height];
        List<RowController> rows = new List<RowController>();
        for (int y = 0; y < height; y++)
        {
            GameObject row = GameObject.Instantiate(rowPrefab);
            row.transform.SetParent(boardGameObject.transform);
            row.GetComponent<RectTransform>().localScale = Vector3.one;
            RowController CurrRow = row.GetComponent<RowController>();
            for (int x = 0; x < width; x++)
            {
                GameObject field = GameObject.Instantiate(fieldPrefab);
                field.transform.SetParent(row.transform);
                FieldContainer fieldContainer = field.GetComponent<FieldContainer>();
                fieldContainer.RectTransform.localScale = Vector3.one;
                fieldContainer.BorderImage.gameObject.SetActive(false);
                int Id = 0;
                fieldContainer.IconImage.sprite = GetRandomItem(items, out Id);
                while ((x - 1 >= 0 && itemIdArray[x - 1, y] == Id) && (y - 1 >= 0 && itemIdArray[x, y - 1] == Id))
                {
                    fieldContainer.IconImage.sprite = GetRandomItem(items, out Id);
                }
                field.GetComponent<Button>().onClick.AddListener(delegate { SetActiveField(field); });
                field.GetComponent<Button>().onClick.AddListener(delegate { TrySwap(field); });
                CurrRow.Fields.Add(field);
                itemIdArray[x, y] = Id;
                field.name = "FIELD " + x + "|" + y + " ID " + Id;
            }
            CurrRow.name = "ROW " + y;

            rows.Add(CurrRow);
        }

        boardViewController.Rows=rows;
        return itemIdArray;
    }



    private void SetActiveField(GameObject _field)
    {
        boardViewController.PlaySound(boardCompononets.AudioBank.GetSound(AudioType.Select).Sound);
        boardViewController.GetFieldPos(_field, out int x, out int y);
        //is current null then set clicked field current
        if (_currentActiveFIeld.Item1 == null)
        {
            _currentActiveFIeld.Item1 = x;
            _currentActiveFIeld.Item2 = y;
            _currentFieldType = GetItemType(_itemIdArray[x, y]);
            boardViewController.ActivateField(_field);
        }
        else if (_currentActiveFIeld.Item1 == x && _currentActiveFIeld.Item2 == y)//diselect field when clicked the same field
        {
            _currentActiveFIeld.Item1 = null;
            _currentActiveFIeld.Item2 = null;
            _currentFieldType = ItemType.Normal;
            boardViewController.ActivateField(_field, false);
        }
    }

    private ItemType GetItemType(int _id)
    {
        return boardCompononets.Items.Find(x => x.ID == _id).itemType;
    }


    private void TrySwap(GameObject _field)
    {
        boardViewController.GetFieldPos(_field, out int x, out int y);
        //is any field selected && are we clicking the same field
        if (_currentActiveFIeld.Item1 != null && !(_currentActiveFIeld == (x, y)))
        {
            if (x == _currentActiveFIeld.Item1)
            {
                if (y == _currentActiveFIeld.Item2 - 1)
                {
                    Swap(_field, x, y);
                }
                else if (y == _currentActiveFIeld.Item2 + 1)
                {
                    Swap(_field, x, y);
                }
            }
            if (y == _currentActiveFIeld.Item2)
            {
                if (x == _currentActiveFIeld.Item1 - 1)
                {
                    Swap(_field, x, y);
                }
                else if (x == _currentActiveFIeld.Item1 + 1)
                {
                    Swap(_field, x, y);
                }
            }
        }

    }

    private void Swap(GameObject field, int x, int y)
    {
        if (GetItemType(_itemIdArray[x, y]) == ItemType.DestroyTheSame || _currentFieldType == ItemType.DestroyTheSame)
        {
            (_itemIdArray[x, y], _itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) = (_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value], _itemIdArray[x, y]);
            ChangeSprites(field, x, y);
            DestroySameType(x, y);
            DeactivateField();

            return;
        }
        if ((GetItemType(_itemIdArray[x, y]) == ItemType.Bomb || _currentFieldType == ItemType.Bomb) && (GetItemType(_itemIdArray[x, y]) == ItemType.Normal || _currentFieldType == ItemType.Normal))
        {
            (_itemIdArray[x, y], _itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) = (_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value], _itemIdArray[x, y]);
            ChangeSprites(field, x, y);
            BombEvent(x, y);
            DeactivateField();
            return;
        }
        (_itemIdArray[x, y], _itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) = (_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value], _itemIdArray[x, y]);
        ChangeSprites(field, x, y);
        DeactivateField();
        VerifyBoard();
    }

    private void DestroySameType(int _x, int _y)
    {
        int IdToFind = 99;
        if ((GetItemType(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) == ItemType.DestroyTheSame)) IdToFind = _itemIdArray[_x, _y];
        if ((GetItemType(_itemIdArray[_x, _y]) == ItemType.DestroyTheSame)) IdToFind = _itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value];
        List<(int, int)> toRemove = new List<(int, int)>();
        if (IdToFind != 99)
        {
            toRemove = GetFieldsOfType(IdToFind);
        }
        if (!toRemove.Contains((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value))) toRemove.Add((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value));
        if (!toRemove.Contains((_x, _y))) toRemove.Add((_x, _y));
        toRemove.Sort(new MyComparer());
        if (toRemove.Count > 0)
        {
            RemoveFieldsAsync(toRemove);
        }
    }

    private List<(int, int)> GetFieldsOfType(int _idToFind)
    {
        List<(int, int)> cords = new List<(int, int)>();
        for (int x = 0; x < _itemIdArray.GetLength(0); x++)
        {
            for (int y = 0; y < _itemIdArray.GetLength(1); y++)
            {
                if (_itemIdArray[x, y] == _idToFind) cords.Add((x, y));
            }
        }
        return cords;
    }

    private void BombEvent(int _x, int _y)
    {
        List<(int, int)> centers = new List<(int, int)>();
        if ((GetItemType(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) == ItemType.Bomb)) centers.Add((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value));
        if ((GetItemType(_itemIdArray[_x, _y]) == ItemType.Bomb)) centers.Add((_x, _y));
        List<(int, int)> toRemove = new List<(int, int)>();
        foreach ((int, int) cord in centers)
        {
            for (int i = cord.Item1 - 1; i <= cord.Item1 + 1; i++)
            {
                for (int j = cord.Item2 - 1; j <= cord.Item2 + 1; j++)
                {
                    if (!toRemove.Contains((i, j)) && i >= 0 && i < boardCompononets.Width && j >= 0 && j < boardCompononets.Height) toRemove.Add((i, j));
                }
            }
        }
        toRemove.Sort(new MyComparer());
        if (toRemove.Count > 0)
        {
            RemoveFieldsAsync(toRemove);
        }
    }

    private void ChangeSprites(GameObject field, int x, int y)
    {
        boardViewController.SwapSprite(field, GetItemById(_itemIdArray[x, y]));
        boardViewController.SwapSprite(_currentActiveFIeld.Item2.Value, _currentActiveFIeld.Item1.Value, GetItemById(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]));
    }

    private void DeactivateField()
    {
        boardViewController.ActivateField(_currentActiveFIeld.Item2.Value, _currentActiveFIeld.Item1.Value);
        _currentActiveFIeld.Item2 = null;
        _currentActiveFIeld.Item1 = null;
    }

    private Sprite GetRandomItem(List<Item> _items, out int _id)
    {
        _id = 0;
        if (_items.Count > 0)
        {
            Item item = _items[UnityEngine.Random.RandomRange(0, _items.Count)];
            _id = item.ID;
            return item.Icon;
        }
        return null;
    }
    private Sprite GetItemById(int _id, out int _fieldid)
    {
        Item item = boardCompononets.Items[_id];
        _fieldid = item.ID;
        return item.Icon;
    }
    private Sprite GetItemById(int _id)
    {
        Item item = boardCompononets.Items.Find(x => x.ID == _id);
        return item.Icon;
    }

    private void VerifyBoard()
    {
        List<(int, int)> toRemove = new List<(int, int)>();

        for (int x = 0; x < _itemIdArray.GetLength(0); x++)
        {
            for (int y = 0; y < _itemIdArray.GetLength(1); y++)
            {
                //Check horizontal
                if (x + 2 < _itemIdArray.GetLength(0))
                {
                    if (_itemIdArray[x, y] == _itemIdArray[x + 1, y] && (_itemIdArray[x + 1, y] == _itemIdArray[x + 2, y]))
                    {
                        if (!toRemove.Contains((x, y))) toRemove.Add((x, y));
                        if (!toRemove.Contains((x + 1, y))) toRemove.Add((x + 1, y));
                        if (!toRemove.Contains((x + 2, y))) toRemove.Add((x + 2, y));
                    }
                }
                //Check vertical
                if (y + 2 < _itemIdArray.GetLength(1))
                {
                    if (_itemIdArray[x, y] == _itemIdArray[x, y + 1] && (_itemIdArray[x, y + 1] == _itemIdArray[x, y + 2]))
                    {
                        if (!toRemove.Contains((x, y))) toRemove.Add((x, y));
                        if (!toRemove.Contains((x, y + 1))) toRemove.Add((x, y + 1));
                        if (!toRemove.Contains((x, y + 2))) toRemove.Add((x, y + 2));
                    }
                }
            }
        }
        toRemove.Sort(new MyComparer());
        if (toRemove.Count > 0)
        {
            RemoveFieldsAsync(toRemove);
        }

    }

    private async Task RemoveFieldsAsync(List<(int, int)> _toRemove)
    {
        boardViewController.RunDotweenAsync(_toRemove); 
        foreach ((int, int) cord in _toRemove)
        {
            boardViewController.SwapSprite(cord.Item2, cord.Item1, GetRandomItem(boardCompononets.Items, out int id));
            boardViewController.FadeField(cord.Item2, cord.Item1);

        }
        boardViewController.AudioSource.PlayOneShot(boardCompononets.AudioBank.GetSound(AudioType.Destroy).Sound);
        VerifyBoard();
    }
}
public class MyComparer : IComparer<(int, int)>
{
    public int Compare((int, int) x, (int, int) y)
    {
        if (x.Item2.CompareTo(y.Item2) != 0)
            return x.Item2.CompareTo(y.Item2);
        else
            return 0;
    }
}
