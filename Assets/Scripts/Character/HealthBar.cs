using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private Image _staminaBarImage;

    [SerializeField] private Sprite _emptyHeart;
    [SerializeField] private Sprite _filledHeart;

    private List<GameObject> _heartList;

    public void UpdateHealthBar(int hp, int maxHp) {
        _healthBarImage.fillAmount = (float)hp / (float)maxHp;
    }

    public void UpdateStaminaBar(int stamina, int maxStamina) {
        _staminaBarImage.fillAmount = (float)stamina / (float)maxStamina;
    }

    public void PlaceHearts(bool rightDirection, int numberHearts)
    {
        _heartList = new List<GameObject>();

        for (int i = 0; i < numberHearts; i++)
        {
            // Heart empty
            GameObject heartEmpty = new GameObject("HeartEmpty" + i.ToString());
            SpriteRenderer heartEmptyRenderer = heartEmpty.AddComponent<SpriteRenderer>();
            heartEmptyRenderer.sprite = _emptyHeart;
            heartEmpty.transform.localScale = new Vector3(30, 30, 30);

            // Heart filled
            GameObject heart = new GameObject("Heart" + i.ToString());
            SpriteRenderer heartRenderer = heart.AddComponent<SpriteRenderer>();
            heartRenderer.sprite = _filledHeart;
            heart.transform.localScale = new Vector3(30, 30, 30);

            if (rightDirection)
            {
                // Heart empty
                heartEmpty.transform.SetParent(transform, false);
                Vector3 heartEmptyPos = new Vector3(25, 25, 1) + new Vector3(30*i, 0, 0);
                heartEmpty.transform.localPosition = heartEmptyPos;

                // Heart filled
                heart.transform.SetParent(transform, false);
                Vector3 heartPos = new Vector3(25, 25, 0) + new Vector3(30*i, 0, 0);
                heart.transform.localPosition = heartPos;
            }
            else 
            {
                // Heart empty
                heartEmpty.transform.SetParent(transform, false);
                Vector3 heartEmptyPos = new Vector3(-25, 25, 1) + new Vector3(-30*i, 0, 0);
                heartEmpty.transform.localPosition = heartEmptyPos;

                // Heart filled
                heart.transform.SetParent(transform, false);
                Vector3 heartPos = new Vector3(-25, 25, 0) + new Vector3(-30*i, 0, 0);
                heart.transform.localPosition = heartPos;
            }

            _heartList.Add(heart);
        }
    }

    public void LoseLive()
    {
        GameObject lastHeart = _heartList[_heartList.Count-1];
        _heartList.RemoveAt(_heartList.Count-1);

        Destroy(lastHeart);
    }
}