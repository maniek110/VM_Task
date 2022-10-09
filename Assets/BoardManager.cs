using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public int Width;
    public int Height;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private GameObject _fieldPrefab;
    [SerializeField] private List<Item> _items;

    private int[,] _itemIdArray;
    private (int?, int?) _currentActiveFIeld = (null, null); //current x and y
    private ItemType _currentFieldType = ItemType.Normal;
    private List<RowController> _rows = new List<RowController>();
    void Start()
    {
        _itemIdArray = new int[Width, Height];
        InitializeBoard();

    }

    // Update is called once per frame
    void Update()
    {
        /*string temp = "";
        for (int y = 0; y < _itemIdArray.GetLength(0); y++)
        {
            for (int x = 0; x < _itemIdArray.GetLength(1); x++)
            {
                temp += _itemIdArray[x, y] + "|";
            }
            temp += "\n";
        }
        Debug.Log(temp);
*/
    }
    private void InitializeBoard()
    {
        for (int y = 0; y < Height; y++)
        {
            GameObject row = Instantiate(_rowPrefab);
            row.transform.SetParent(this.transform);
            row.GetComponent<RectTransform>().localScale = Vector3.one;
            RowController CurrRow = row.GetComponent<RowController>();
            for (int x = 0; x < Width; x++)
            {
                GameObject field = Instantiate(_fieldPrefab);
                field.transform.SetParent(row.transform);
                FieldContainer fieldContainer = field.GetComponent<FieldContainer>();
                fieldContainer.RectTransform.localScale = Vector3.one;
                fieldContainer.BorderImage.gameObject.SetActive(false);
                int Id = 0;
                fieldContainer.IconImage.sprite = GetRandomItem(out Id);
                while ((x - 1 >= 0 && _itemIdArray[x - 1, y] == Id) && (y - 1 >= 0 && _itemIdArray[x, y - 1] == Id))
                {
                    fieldContainer.IconImage.sprite = GetRandomItem(out Id);
                }
                field.GetComponent<Button>().onClick.AddListener(delegate { SetActiveField(field); });
                field.GetComponent<Button>().onClick.AddListener(delegate { TrySwap(field); });
                CurrRow.Fields.Add(field);
                _itemIdArray[x, y] = Id;
                field.name = "FIELD " + x + "|" + y + " ID " + Id;
            }
            CurrRow.name = "ROW " + y;
            _rows.Add(CurrRow);
        }
    }

    private void SetActiveField(GameObject field)
    {
        GetFieldPos(field, out int x, out int y);
        //is current null then set clicked field current
        if (_currentActiveFIeld.Item1 == null)
        {
            _currentActiveFIeld.Item1 = x;
            _currentActiveFIeld.Item2 = y;
            _currentFieldType = GetItemType(_itemIdArray[x, y]);
            FieldContainer tempFieldContainer = field.GetComponent<FieldContainer>();
            tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        }
        else if (_currentActiveFIeld.Item1 == x && _currentActiveFIeld.Item2 == y)//diselect field when clicked the same field
        {
            _currentActiveFIeld.Item1 = null;
            _currentActiveFIeld.Item2 = null;
            _currentFieldType = ItemType.Normal;
            FieldContainer tempFieldContainer = field.GetComponent<FieldContainer>();
            tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        }
    }

    private ItemType GetItemType(int id)
    {
        return _items.Find(x => x.ID == id).itemType;
    }

    private void GetFieldPos(GameObject field, out int x, out int y)
    {
        x = 0;
        y = 0;
        for (int i = 0; i < _rows.Count; i++)
        {
            RowController tempRow = _rows[i].GetComponent<RowController>();
            for (int j = 0; j < tempRow.Fields.Count; j++)
            {
                if (field.GetInstanceID() == tempRow.Fields[j].GetInstanceID())
                {
                    y = i;
                    x = j;
                    break;
                }
            }
        }
    }

    private void TrySwap(GameObject field)
    {
        GetFieldPos(field, out int x, out int y);
        //is any field selected && are we clicking the same field
        if (_currentActiveFIeld.Item1 != null && !(_currentActiveFIeld == (x, y)))
        {
            if (x == _currentActiveFIeld.Item1)
            {
                if (y == _currentActiveFIeld.Item2 - 1)
                {
                    Debug.Log("CanSwitch1");
                    Swap(field, x, y);
                }
                else if (y == _currentActiveFIeld.Item2 + 1)
                {
                    Debug.Log("CanSwitch2");
                    Swap(field, x, y);

                }
            }
            if (y == _currentActiveFIeld.Item2)
            {
                if (x == _currentActiveFIeld.Item1 - 1)
                {
                    Debug.Log("CanSwitch3");
                    Swap(field, x, y);

                }
                else if (x == _currentActiveFIeld.Item1 + 1)
                {
                    Debug.Log("CanSwitch4");
                    Swap(field, x, y);
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

    private void DestroySameType(int x, int y)
    {
        int IdToFind = 99;
        if ((GetItemType(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) == ItemType.DestroyTheSame)) IdToFind = _itemIdArray[x, y]; 
        if ((GetItemType(_itemIdArray[x, y]) == ItemType.DestroyTheSame)) IdToFind = _itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value];
        List<(int, int)> toRemove = new List<(int, int)>();
        if (IdToFind != 99)
        {
            toRemove = GetFieldsOfType(IdToFind);
        }
        if (!toRemove.Contains((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value))) toRemove.Add((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value));
        if (!toRemove.Contains((x, y))) toRemove.Add((x, y));
        toRemove.Sort(new MyComparer());
        if (toRemove.Count > 0)
        {
            RemoveFields(toRemove);
        }
    }

    private List<(int, int)> GetFieldsOfType(int idToFind)
    {
        List<(int, int)> cords = new List<(int, int)>();
        for (int x = 0; x < _itemIdArray.GetLength(0); x++)
        {
            for (int y = 0; y < _itemIdArray.GetLength(1); y++)
            {
                if (_itemIdArray[x, y] == idToFind) cords.Add((x, y));
            }
        }
        return cords;
    }

    private void BombEvent(int x, int y)
    {
        List<(int, int)> centers = new List<(int, int)>();
        if ((GetItemType(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]) == ItemType.Bomb)) centers.Add((_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value));
        if ((GetItemType(_itemIdArray[x, y]) == ItemType.Bomb)) centers.Add((x, y));
        List<(int, int)> toRemove = new List<(int, int)>();

        foreach ((int, int) cord in centers)
        {
            for (int i = cord.Item1 - 1; i <= cord.Item1 + 1; i++)
            {
                for (int j = cord.Item2 - 1; j <= cord.Item2 + 1; j++)
                {
                    if (!toRemove.Contains((i, j))) toRemove.Add((i, j));
                }
            }
        }
        toRemove.Sort(new MyComparer());
        if (toRemove.Count > 0)
        {
            RemoveFields(toRemove);
        }
    }

    private void ChangeSprites(GameObject field, int x, int y)
    {
        field.GetComponent<FieldContainer>().IconImage.sprite = GetItemById(_itemIdArray[x, y]);
        _rows[_currentActiveFIeld.Item2.Value].Fields[_currentActiveFIeld.Item1.Value].GetComponent<FieldContainer>().IconImage.sprite = GetItemById(_itemIdArray[_currentActiveFIeld.Item1.Value, _currentActiveFIeld.Item2.Value]);
    }

    private void DeactivateField()
    {
        FieldContainer tempFieldContainer = _rows[_currentActiveFIeld.Item2.Value].Fields[_currentActiveFIeld.Item1.Value].GetComponent<FieldContainer>();
        tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        _currentActiveFIeld.Item2 = null;
        _currentActiveFIeld.Item1 = null;
    }

    private Sprite GetRandomItem(out int id)
    {
        id = 0;
        if (_items.Count > 0)
        {
            Item item = _items[UnityEngine.Random.RandomRange(0, _items.Count)];
            id = item.ID;
            return item.Icon;
        }
        return null;
    }
    private Sprite GetItemById(int id, out int fieldid)
    {
        Item item = _items[id];
        fieldid = item.ID;
        return item.Icon;
    }
    private Sprite GetItemById(int id)
    {
        Item item = _items.Find(x => x.ID == id);
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
            RemoveFields(toRemove);
        }

    }

    private void RemoveFields(List<(int, int)> toRemove)
    {
        foreach ((int, int) cord in toRemove)
        {
            //temporary 99 is empty field
            _itemIdArray[cord.Item1, cord.Item2] = 99;
            ShiftTiles(cord.Item1, cord.Item2);
        }
        //AddPoints(toRemove);
        VerifyBoard();
    }

    private void ShiftTiles(int x, int yStart, float shiftDelay = .03f)
    {
        Debug.Log($"START {x}|{yStart}");

        List<(int, int)> ToShift = new List<(int, int)>();
        int toMove = 0;
        for (int y = yStart; y >= 0; y--)
        {
            if (_itemIdArray[x, y] == 99)
            {
                _rows[y].Fields[x].GetComponent<FieldContainer>().IconImage.sprite = null;
                toMove++;
            }
            Debug.Log($"{x}|{y}");
            ToShift.Add((x, y));
            Debug.Log("Kutas");
        }
        /*        for (int i = 0; i < toMove; i++)
                {
                    for
                }
        */







        List<(int, int)> ToRefil = new List<(int, int)>();
        int nullCount = 0;
        for (int y = yStart; y >= 0; y--)
        {  // 1
            if (_itemIdArray[x, y] == 99)
            { // 2
                _rows[y].Fields[x].GetComponent<FieldContainer>().IconImage.sprite = null;
                nullCount++;
            }
            ToRefil.Add((x, y));
        }

        for (int i = 0; i < nullCount; i++)
        {
            //yield return new WaitForSeconds(shiftDelay);// 4
            for (int k = 0; k < ToRefil.Count; k++)
            { // 5
                int id;
                if (ToRefil[k].Item2 == 0)
                {
                    _rows[ToRefil[k].Item2].Fields[ToRefil[k].Item1].GetComponent<FieldContainer>().IconImage.sprite = GetRandomItem(out id);
                    _itemIdArray[ToRefil[k].Item1, ToRefil[k].Item2] = id;
                }
                else
                {

                    _rows[ToRefil[k].Item2].Fields[ToRefil[k].Item1].GetComponent<FieldContainer>().IconImage.sprite = _rows[ToRefil[k].Item2 - 1].Fields[ToRefil[k].Item1].GetComponent<FieldContainer>().IconImage.sprite;
                    _rows[ToRefil[k].Item2 - 1].Fields[ToRefil[k].Item1].GetComponent<FieldContainer>().IconImage.sprite = GetRandomItem(out id);
                    _itemIdArray[ToRefil[k].Item1, ToRefil[k].Item2] = _itemIdArray[ToRefil[k].Item1, ToRefil[k].Item2 - 1];
                    _itemIdArray[ToRefil[k].Item1, ToRefil[k].Item2 - 1] = id; // 6
                }
            }
        }
        //throw new NotImplementedException();
    }


    private void Slide(List<(int, int)> temp)
    {
        throw new NotImplementedException();
    }

    private void AddPoints(List<(int, int)> toRemove)
    {
        throw new NotImplementedException();
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
