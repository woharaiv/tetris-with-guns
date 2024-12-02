using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoMeters : MonoBehaviour
{
    [SerializeField] public GameObject[] ammoBars;
    public Slider[] ammoFills { get; private set; }
    private void Start()
    {
        for(int i = 0; i < ammoBars.Length; i++)
        {
            Gun.instance.ammoMeters.Add(ammoBars[i].GetComponent<Slider>());
            ammoBars[i].GetComponentInChildren<TextMeshProUGUI>().text = Gun.instance.weapons[i].name;
        }
    }
}
