using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public class Item : ScriptableObject
{
    public int ID;
    public Sprite Icon;
    public string Name;
    public ItemType itemType = ItemType.Normal;
}
public enum ItemType
{
    Normal,
    Bomb,
    DestroyTheSame
}