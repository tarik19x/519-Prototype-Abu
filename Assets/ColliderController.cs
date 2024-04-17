using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ColliderController : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "SoundIcon_FourStatus")
        {
            SoundManager.Instance.PlaySound(clip);
            Debug.Log("Object Hitted");
        }
    }
}
