using UnityEngine;

public class SfxHandler : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public void Play()
    {
        Debug.Log("Playing");
        SoundManager.Instance.PlaySound(clip);
    }
}