using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public enum InfoMode
{
    NONE = 0,
    CUSTOM_TEXT,
    AMMO
}
public class InfoPopup : MonoBehaviour
{
    InfoMode mode = InfoMode.NONE;
    [SerializeField] Sprite[] sprites;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshProUGUI textMesh;

    private void Start()
    {
        if (mode != InfoMode.NONE)
            Initialize(mode);
    }

    public void Initialize(InfoMode mode, string customText = "TEXT NOT INITIALIZED")
    {
        switch (mode)
        {
            case InfoMode.NONE:
                Debug.LogError("Attempted to initialize InfoPopup with mode NONE");
                return;
            case InfoMode.CUSTOM_TEXT:
                textMesh.text = customText;
                spriteRenderer.enabled = false;
                textMesh.DOFade(0f, 3f);
                break;
            case InfoMode.AMMO:
                spriteRenderer.sprite = sprites[0];
                textMesh.enabled = false;
                spriteRenderer.DOFade(0f, 3f);
                break;

        }
        transform.DOMoveY(transform.position.y + 3, 3f).OnComplete(End);
    }

    private void End()
    {
        Destroy(gameObject);
    }
}
