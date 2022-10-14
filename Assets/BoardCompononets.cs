using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BoardCompononets", menuName = "ScriptableObjects/BoardCompononets", order = 4)]

public class BoardCompononets : ScriptableObject
{
    public int Width = 5;
    public int Height = 5;
    public GameObject BoardPrefab;
    public GameObject RowPrefab;
    public GameObject FieldPrefab;
    public AudioBank AudioBank;
    public List<Item> Items;
}
