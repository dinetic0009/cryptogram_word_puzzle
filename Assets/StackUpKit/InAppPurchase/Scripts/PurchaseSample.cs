using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseSample : MonoBehaviour
{
	[SerializeField]
	public Text PurchaseStatus;
	public void UnlockAll()
	{
		Purchaser.instance.BuyNonConsumableProduct("UnlockAll");
		Purchaser.instance.NonConsumableAction += OnNonConsumableSuccess;
	}
	public void GetHints()
	{
		Purchaser.instance.BuyConsumableProduct("DiamondHints");
		Purchaser.instance.consumableAction += OnConsumableSuccess;

	}
	private void OnConsumableSuccess(Consumable product_info, bool success)
	{

		if (success)
		{
			Debug.Log(product_info.ProductID + "Success");
			PurchaseStatus.text = product_info.ProductID + "Success";
		}
		else
		{
			Debug.Log(product_info.ProductID + "Failed");
			PurchaseStatus.text = product_info.ProductID + "Failed";
		}
		Purchaser.instance.consumableAction -= OnConsumableSuccess;
	}
	private void OnNonConsumableSuccess(NonConsumable product_info, bool success)
	{
		if (success)
		{
			Debug.Log(product_info.ProductID + "Success");
			PurchaseStatus.text = product_info.ProductID + "Success";
		}
		else
		{
			Debug.Log(product_info.ProductID + "Failed");
			PurchaseStatus.text = product_info.ProductID + "Failed";

		}
		Purchaser.instance.NonConsumableAction -= OnNonConsumableSuccess;

	}
}
