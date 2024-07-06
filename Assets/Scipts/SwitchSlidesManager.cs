using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchSlidesManager : MonoBehaviour
{
    public Sprite[] slides;  
    public int idx = 0;
    public Image imageComponent;

    void Start()
    {
        imageComponent = GetComponent<Image>();
        Debug.Log("SwitchSlidesManager: loaded " + slides.Length + " slides");
        if (slides.Length > 0)
        {
            Debug.Log("SwitchSlidesManager: slide.length > 0");
            imageComponent.sprite = slides[idx];
        }
    }

    public void SwitchPhoto()
    {
        if (slides.Length > 0)
        {
            idx = (idx + 1) % slides.Length;
            imageComponent.sprite = slides[idx];
            Debug.Log("SwitchSlidesManager: Switched to slide " + idx);
        }
    }
}
