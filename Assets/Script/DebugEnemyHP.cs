using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugEnemyHP : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;
    public Image fillImage;

    float maxHP = 0;
    CharacterBase character;
    internal void Set(CharacterBase character)
    {
        this.character = character;
        maxHP = character.maxHP;

        image.sprite = character.GetImage().sprite;
    }
    public void UpdateView()
    {
        fillImage.fillAmount = character.hp / maxHP;
        text.text = character.hp.ToString();
    }

    void Update()
    {
        if (character != null)
        {
            UpdateView();

            if (character.isDead) Destroy(gameObject);
            if (character.gameObject.activeSelf == false) Destroy(gameObject);
        }
    }
}
