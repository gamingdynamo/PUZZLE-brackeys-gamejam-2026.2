using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class MobileDisplay : MonoBehaviour
{
    [SerializeField] GameObject mobileDisplay;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mobileDisplay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        MobileToggle();
    }

    public void MobileToggle()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame && !mobileDisplay.activeInHierarchy)
        {
            mobileDisplay.SetActive(true);
        }
        else if (Keyboard.current.mKey.wasPressedThisFrame && mobileDisplay.activeInHierarchy)
        {
            mobileDisplay.SetActive(false);
        }
    }
}
