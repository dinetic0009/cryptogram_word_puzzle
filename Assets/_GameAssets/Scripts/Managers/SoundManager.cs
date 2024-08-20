using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using UnityEngine;
using MyBox;
using System.Linq;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    //[SerializeField] private List<AudioSource> sfxSources;
    [SerializeField] private Sound[] music, sfx;

    //------------------------------------------------------//
    //----------------------- Click ------------------------//
    //------------------------------------------------------//


    public void OnClickSound()
    {
        PlaySfx(SoundType.Click);
    }



    //------------------------------------------------------//
    //----------------------- MUSIC ------------------------//
    //------------------------------------------------------//


    public void PlayMusic()
    {
        if (music.Length <= 0)
        {
            Debug.LogError($"Muisc not found");
            return;
        }

        Sound sound = music[UnityEngine.Random.Range(0, music.Length)];
        musicSource.clip = sound.audioClip;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }


    //public void AddMusciSource(AudioSource audioSource)
    //{
    //    sfxSources.Add(audioSource);
    //}


    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(bool isMute)
    {
        musicSource.mute = isMute;

    }

    //------------------------------------------------------//
    //----------------------- SFX --------------------------//
    //------------------------------------------------------//

    public void PlaySfx(SoundType type)
    {
        Sound sound = Array.Find(sfx, x => x.effectType == type);

        if (sound == null)
        {
            Debug.LogError($"Sound \"{type}\" not foound");
            return;
        }

        sfxSource.PlayOneShot(sound.audioClip);
    }

    public void StopSfx(SoundType type)
    {
        Sound sound = Array.Find(sfx, x => x.effectType == type);
        if (sound == null)
        {
            Debug.LogError($"Sound \"{type}\" not foound");
            return;
        }

        if (sfxSource.clip == sound.audioClip)
            sfxSource.Stop();
    }


    public void SetSfxVolume(bool isMute)
    {
        sfxSource.mute = isMute;
        //sfxSources.ForEach(x => x.mute = isMute);
    }


    IEnumerator WaitForSoundToFinish(float originalMusicVolume, AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        musicSource.volume = originalMusicVolume;
    }


    public void Vibrate(HapticTypes hapticTypes = HapticTypes.MediumImpact)
    {
        if (PlayerPrefs.GetInt("vibration") == 1)
            MMVibrationManager.Haptic(hapticTypes);
    }


}//Class

[Serializable]
public class Sound
{
    public SoundType effectType;
    public AudioClip audioClip;
}

public enum SoundType
{
    Click,
    Select,
    Wrong,
    Correct,
    Win,
    Lose,
    Keyboard,
}