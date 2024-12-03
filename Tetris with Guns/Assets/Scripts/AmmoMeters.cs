using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoMeters : MonoBehaviour
{
    public static AmmoMeters Instance;
    [SerializeField] public GameObject[] GameObjects;
    [SerializeField] public GameObject[] Tints;
    public Slider[] Sliders;
    public Image[] FillImages;
    public Slider[] ammoFills { get; private set; }
    private void Awake()
    {
        Instance = this;
        Sliders = new Slider[GameObjects.Length];
        FillImages = new Image[GameObjects.Length];
        for(int i = 0; i < GameObjects.Length; i++)
        {
            GameObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = Gun.instance.weapons[i].name;
            Sliders[i] = GameObjects[i].GetComponent<Slider>();
            foreach(Transform child in GameObjects[i].transform)
            {
                if(child.name.Equals("Fill"))
                    FillImages[i] = child.GetComponent<Image>();
            }
        }
    }
}
