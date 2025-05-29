using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [FormerlySerializedAs("ballPrefabs")]
    [Header("Ball Settings")]
    [SerializeField] private GameObject[] emojiPrefabs;
    [FormerlySerializedAs("ballPrices")] [SerializeField] private int[] emojiPrices;
    [Header("UI References")]
    [SerializeField] private Text coinText;
    [FormerlySerializedAs("ballButtons")] [SerializeField] private Button[] emojiButtons;
    [SerializeField] private Text[] buttonTexts;
    [Header("Toaster Settings")]
    [SerializeField] private GameObject toasterPrefab;
    [SerializeField] private Transform toasterParent;
    [SerializeField] private float toasterDuration = 2f;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float bounceSpeed = 0.1f;

    private int selectedEmojiIndex;
    private GameObject currentToaster;

    void Start()
    {
        // Ensure the first emoji is bought and selected by default
        if (PlayerPrefs.GetInt("emoji_0_bought", 0) == 0)
        {
            PlayerPrefs.SetInt("emoji_0_bought", 1);
            PlayerPrefs.SetInt("selectedEmoji", 0);
            PlayerPrefs.Save();
        }

        selectedEmojiIndex = PlayerPrefs.GetInt("selectedEmoji", 0);
        selectedEmojiIndex = Mathf.Clamp(selectedEmojiIndex, 0, emojiPrefabs.Length - 1);

        UpdateShopUI();
        UpdateCoinText();
    }


    public void BuyAndSelectEmoji(int index)
    {
        if (index < 0 || index >= emojiPrefabs.Length || index >= emojiPrices.Length) return;

        string emojiKey = $"emoji_{index}_bought";
        bool isBought = PlayerPrefs.GetInt(emojiKey, 0) == 1;
        int coins = PlayerPrefs.GetInt("coins", 0);
        int price = emojiPrices[index];

        if (!isBought)
        {
            if (coins >= price)
            {
                coins -= price;
                PlayerPrefs.SetInt("coins", coins);
                PlayerPrefs.SetInt(emojiKey, 1);
                PlayerPrefs.SetInt("selectedEmoji", index);
                PlayerPrefs.Save();
                Debug.Log($"Emoji {index} bought for {price} coins and selected.");
                ShowToaster("Emoji Bought!");
            }
            else
            {
                Debug.Log("Not enough coins to buy this emoji.");
                ShowToaster("Not Enough Coins");
            }
        }
        else
        {
            PlayerPrefs.SetInt("selectedEmoji", index);
            PlayerPrefs.Save();
            Debug.Log($"Emoji {index} selected.");
            ShowToaster("Emoji Selected");
        }

        selectedEmojiIndex = index;
        UpdateShopUI();
        UpdateCoinText();
    }
    private void ShowToaster(string message)
    {
        if (currentToaster != null)
        {
            Destroy(currentToaster);
        }
        if (toasterPrefab != null && toasterParent != null)
        {
            GameObject toasterInstance = Instantiate(toasterPrefab, toasterParent);
            Text toasterText = toasterInstance.GetComponentInChildren<Text>();
            if (toasterText != null)
            {
                toasterText.text = message;
                StartCoroutine(AnimateToaster(toasterText.transform, toasterInstance));
            }
            currentToaster = toasterInstance;
            Destroy(toasterInstance, toasterDuration + (bounceSpeed * 4));
        }
        else
        {
            Debug.LogWarning("Toaster Prefab or Parent not assigned in the Inspector.");
        }
    }

    private IEnumerator AnimateToaster(Transform toasterTransform, GameObject toaster)
    {
        // Bounce In
        Vector3 originalScale = toasterTransform.localScale;
        float timer = 0f;
        while (timer < bounceSpeed * 2)
        {
            timer += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(0f, bounceScale, timer / bounceSpeed);
            toasterTransform.localScale = originalScale * scaleFactor;
            yield return null;
        }
        toasterTransform.localScale = originalScale * bounceScale;

        timer = 0f;
        while (timer < bounceSpeed * 2)
        {
            timer += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(bounceScale, 1f, timer / bounceSpeed);
            toasterTransform.localScale = originalScale * scaleFactor;
            yield return null;
        }
        toasterTransform.localScale = originalScale;

        // Wait for the duration
        yield return new WaitForSeconds(toasterDuration);

        // Bounce Out
        timer = 0f;
        while (timer < bounceSpeed * 2)
        {
            timer += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(1f, bounceScale, timer / bounceSpeed);
            toasterTransform.localScale = originalScale * scaleFactor;
            yield return null;
        }
        toasterTransform.localScale = originalScale * bounceScale;

        timer = 0f;
        while (timer < bounceSpeed * 2)
        {
            timer += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(bounceScale, 0f, timer / bounceSpeed);
            toasterTransform.localScale = originalScale * scaleFactor;
            yield return null;
        }
        toasterTransform.localScale = Vector3.zero; // Fully scale down before destroy
        if (toaster != null)
        {
            Destroy(toaster);
            currentToaster = null;
        }
    }

    private void UpdateShopUI()
    {
        for (int i = 0; i < emojiPrefabs.Length; i++)
        {
            string emojiKey = $"emoji_{i}_bought";
            bool isBought = PlayerPrefs.GetInt(emojiKey, 0) == 1;

            if (!isBought)
            {
                buttonTexts[i].text = $"${emojiPrices[i]}";
                buttonTexts[i].color = Color.red;
            }
            else if (i == selectedEmojiIndex)
            {
                buttonTexts[i].text = "Selected";
                buttonTexts[i].color = Color.green;
            }
            else
            {
                buttonTexts[i].text = "Select";
                buttonTexts[i].color = Color.yellow;
            }
        }
    }
    private void UpdateCoinText()
    {
        int coins = PlayerPrefs.GetInt("coins", 0);
        //coinText.text = $"{coins}";
        coinText.text = coins.ToString();
    }
}