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
    private (int?, int?) currentActiveFIeld; //current x and y
    private List<RowController> _rows = new List<RowController>();
    //private Field _currentField;
    // Start is called before the first frame update
    void Start()
    {
        _itemIdArray = new int[Width, Height];
        InitializeBoard();
       
    }

    // Update is called once per frame
    void Update()
    {
        string temp = "";
        for (int y = 0; y < _itemIdArray.GetLength(0); y++)
        {
            for (int x = 0; x < _itemIdArray.GetLength(1); x++)
            {
                temp += _itemIdArray[y, x] + "|";
            }
            temp += "\n";
        }
        Debug.Log(temp);
    }
    private void InitializeBoard()
    {
        for (int x = 0; x < Height; x++)
        {
            GameObject row = Instantiate(_rowPrefab);
            row.transform.SetParent(this.transform);
            row.GetComponent<RectTransform>().localScale = Vector3.one;
            RowController CurrRow = row.GetComponent<RowController>();
            for (int y = 0; y < Width; y++)
            {
                GameObject field = Instantiate(_fieldPrefab);
                field.name = "FIELD " + y;
                field.transform.SetParent(row.transform);
                FieldContainer fieldContainer = field.GetComponent<FieldContainer>();
                fieldContainer.RectTransform.localScale = Vector3.one;
                fieldContainer.BorderImage.gameObject.SetActive(false);
                fieldContainer.IconImage.sprite = GetRandomItem(out int Id);
                field.GetComponent<Button>().onClick.AddListener(delegate { SetActiveField(field); });
                field.GetComponent<Button>().onClick.AddListener(delegate { TrySwap(field); });
                CurrRow.Fields.Add(field);
                _itemIdArray[x, y] = Id;
            }
            CurrRow.name = "ROW " + x;
            _rows.Add(CurrRow);
        }
    }
    /*
        private void SetActiveField(GameObject field)
        {
            Debug.Log(field.GetInstanceID());
            throw new System.NotImplementedException();
        }
    */

    private void SetActiveField(GameObject field)
    {
        GetFieldPos(field, out int x, out int y);
        //is current null then set clicked field current
        if (currentActiveFIeld.Item1 == null)
        {
            currentActiveFIeld.Item1 = x;
            currentActiveFIeld.Item2 = y;
            FieldContainer tempFieldContainer = field.GetComponent<FieldContainer>();
            tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        }
        else if (currentActiveFIeld.Item1 == x && currentActiveFIeld.Item2 == y)//diselect field when clicked the same field
        {
            currentActiveFIeld.Item1 = null;
            currentActiveFIeld.Item2 = null;
            FieldContainer tempFieldContainer = field.GetComponent<FieldContainer>();
            tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        }
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
                if (field == tempRow.Fields[j])
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
        if (currentActiveFIeld.Item1 != null && !(currentActiveFIeld == (x, y)))
        {
            if (x == currentActiveFIeld.Item1)
            {
                if (y == currentActiveFIeld.Item2 - 1)
                {
                    Debug.Log("CanSwitch1");
                    Swap(field, x, y);
                }
                else if (y == currentActiveFIeld.Item2 + 1)
                {
                    Debug.Log("CanSwitch2");
                    Swap(field, x, y);

                }
            }
            if (y == currentActiveFIeld.Item2)
            {
                if (x == currentActiveFIeld.Item1 - 1)
                {
                    Debug.Log("CanSwitch3");
                    Swap(field, x, y);

                }
                else if (x == currentActiveFIeld.Item1 + 1)
                {
                    Debug.Log("CanSwitch4");
                    Swap(field, x, y);
                }
            }
        }

    }

    private void Swap(GameObject field, int x, int y)
    {
        Debug.Log($"x {x},  y {y}, _itemIdArray[x, y] {_itemIdArray[x, y]}, currentActiveFIeld.Item2.Value {currentActiveFIeld.Item1.Value}, currentActiveFIeld.Item2.Value {currentActiveFIeld.Item2.Value}, _itemIdArray[currentActiveFIeld.Item2.Value, currentActiveFIeld.Item1.Value] {_itemIdArray[currentActiveFIeld.Item2.Value, currentActiveFIeld.Item1.Value]}");
        int tempid = _itemIdArray[currentActiveFIeld.Item1.Value, currentActiveFIeld.Item2.Value];
        ChangeSprites(field, tempid, _itemIdArray[x, y]);
        _itemIdArray[currentActiveFIeld.Item2.Value, currentActiveFIeld.Item1.Value] = _itemIdArray[x, y];
        _itemIdArray[x, y] = tempid;
        DeactivateField();
    }

    private void ChangeSprites(GameObject field, int tempid, int v)
    {
/*        Debug.Log($"tempid {tempid} | v {v}");
*/        field.GetComponent<FieldContainer>().IconImage.sprite = GetItemById(tempid);
        _rows[currentActiveFIeld.Item2.Value].Fields[currentActiveFIeld.Item1.Value].GetComponent<FieldContainer>().IconImage.sprite = GetItemById(v);
    }

    private void DeactivateField()
    {
        FieldContainer tempFieldContainer = _rows[currentActiveFIeld.Item2.Value].Fields[currentActiveFIeld.Item1.Value].GetComponent<FieldContainer>();
        tempFieldContainer.BorderImage.gameObject.SetActive(!tempFieldContainer.BorderImage.gameObject.activeSelf);
        currentActiveFIeld.Item2 = null;
        currentActiveFIeld.Item1 = null;
    }

    private Sprite GetRandomItem(out int id)
    {
        id = 0;
        if (_items.Count > 0)
        {
            Item item = _items[Random.RandomRange(0, _items.Count)];
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
        Item item = _items.Find(x=>x.ID==id);
        return item.Icon;
    }

}
