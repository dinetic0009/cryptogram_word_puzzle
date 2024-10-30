using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

public class ShopController : MonoBehaviour
{
    public List<ShopItem> shopItems;


}


[Serializable]
public class ShopItem
{
    public float price;
    public bool onAd;
}