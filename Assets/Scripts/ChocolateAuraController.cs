using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChocolateAuraController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private ChocolateBlock parentBlock;

    [SerializeField] private Sprite auraDamagedSprite01;
    [SerializeField] private Sprite auraDamagedSprite02;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        parentBlock = GetComponentInParent<ChocolateBlock>();
    }

    private void OnEnable()
    {
        if (parentBlock != null)
        {
            parentBlock.OnDamageTaken += UpdateAuraSprite;
            UpdateAuraSprite(parentBlock.HitPoint);
        }
    }

    private void OnDisable()
    {
        if (parentBlock != null)
            parentBlock.OnDamageTaken -= UpdateAuraSprite;
    }

    private void UpdateAuraSprite(int currentHP)
    {
        if (currentHP == 2)
            spriteRenderer.sprite = auraDamagedSprite01;
        else if (currentHP == 1)
            spriteRenderer.sprite = auraDamagedSprite02;
    }
}
