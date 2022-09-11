using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthBarImage;

    public void UpdateHealthBar(int hp, int maxHp) {
        _healthBarImage.fillAmount = (float)hp / (float)maxHp;
    }
}