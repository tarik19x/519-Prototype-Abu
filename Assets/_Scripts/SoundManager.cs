using UnityEngine;

public class SoundManager : SingletonMonoBehavior<SoundManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    public void ChangeMasterVolume(float val)
    {
        AudioListener.volume = val;
    }
    public void PlaySound(AudioClip clip)
    {
        if (effectsSource.isPlaying)
        {
            effectsSource.Stop();
        }
        else
        {
            effectsSource.PlayOneShot(clip);
        }
       
    }
    
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    
    public void ToggleEffects()
    {
        effectsSource.mute = !effectsSource.mute;
    }
}