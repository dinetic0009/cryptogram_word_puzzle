using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class Purchaser : MonoBehaviour, IStoreListener
{
    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    public static Purchaser instance;
    public List<NonConsumable> Non_Consumable_Products = new List<NonConsumable>();
    public List<Consumable> Consumable_Products = new List<Consumable>();
    public Action<Consumable, bool> consumableAction;
    public Action<NonConsumable, bool> NonConsumableAction;
    [SerializeField]
    public GameObject Spinner;
    void Start()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        instance = this;
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }

        Debug.Log("");
    }
    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (NonConsumable product in Non_Consumable_Products)
        {
            builder.AddProduct(product.ProductID, ProductType.NonConsumable, new IDs(){
            { product.AppleStoreID, AppleAppStore.Name},
            { product.GoogleStoreID, GooglePlay.Name},
            { product.GoogleStoreID, AmazonApps.Name},
            });
        }
        foreach (Consumable product in Consumable_Products)
        {
            builder.AddProduct(product.ProductID, ProductType.Consumable, new IDs(){
            { product.AppleStoreID, AppleAppStore.Name},
            { product.GoogleStoreID, GooglePlay.Name},
            { product.GoogleStoreID, AmazonApps.Name},
            });
        }
        UnityPurchasing.Initialize(this, builder);
    }
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }
    public void BuyConsumableProduct(string productId)
    {
        Consumable product = Consumable_Products.Find(x => x.ProductID == productId);
        if (product != null)
        {
            BuyProductID(product.ProductID);
        }
        else
        {
            Debug.LogError("No item match with this product id");
        }
    }
    public void BuyNonConsumableProduct(string productId)
    {
        NonConsumable product = Non_Consumable_Products.Find(x => x.ProductID == productId);
        if (product != null)
        {
            BuyProductID(product.ProductID);
        }
        else
        {
            Debug.LogError("No item match with this product id");
        }
    }

    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
#if UNITY_IOS
				Spinner.SetActive(true);
#endif
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
#if UNITY_IOS
				Spinner.SetActive(false);
#endif
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
#if UNITY_IOS
				Spinner.SetActive(false);
#endif

        }
    }
    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {

        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {

        foreach (NonConsumable product in Non_Consumable_Products)
        {
            if (String.Equals(args.purchasedProduct.definition.id, product.ProductID, StringComparison.Ordinal))
            {
                NonConsumableAction?.Invoke(product, true);
                break;
            }
        }
        foreach (Consumable product in Consumable_Products)
        {
            if (String.Equals(args.purchasedProduct.definition.id, product.ProductID, StringComparison.Ordinal))
            {
                consumableAction?.Invoke(product, true);
                break;
            }
        }
#if UNITY_IOS
				Spinner.SetActive(false);
#endif
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
#if UNITY_IOS
				Spinner.SetActive(false);
#endif
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        consumableAction?.Invoke(null, false);
        NonConsumableAction?.Invoke(null, false);

    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }
}
[Serializable]
public class NonConsumable
{
    public string ProductID;
    public string GoogleStoreID;
    public string AppleStoreID;
}
[Serializable]
public class Consumable
{
    public string ProductID;
    public string GoogleStoreID;
    public string AppleStoreID;
    public int RewardAmount;
}
