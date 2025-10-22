// SoundUtils.cs
using System.Collections;
using UnityEngine;

public static class SoundUtils
{
    public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        GameObject go = new GameObject("OneShotAudio");
        go.transform.position = position;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.spatialBlend = 0f; // 2D sound; set >0 for 3D
        src.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }
}
