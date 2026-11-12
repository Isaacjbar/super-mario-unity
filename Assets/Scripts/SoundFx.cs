using UnityEngine;

/// <summary>
/// Tiny procedural sound effects (square-wave blips), no audio files required.
/// </summary>
public static class SoundFx
{
    static AudioSource source;

    public static AudioClip Jump, Coin, Stomp, Die, Win;

    public static void Init(GameObject host)
    {
        source = host.AddComponent<AudioSource>();
        Jump = Tone(280f, 640f, 0.18f);
        Coin = Tone(988f, 1319f, 0.22f);
        Stomp = Tone(300f, 90f, 0.12f);
        Die = Tone(500f, 80f, 0.6f);
        Win = Tone(523f, 1568f, 0.8f);
    }

    public static void Play(AudioClip clip)
    {
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    static AudioClip Tone(float f0, float f1, float duration)
    {
        const int rate = 44100;
        int n = (int)(rate * duration);
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            phase += Mathf.Lerp(f0, f1, t) / rate;
            float square = (phase % 1f) < 0.5f ? 1f : -1f;
            data[i] = square * 0.12f * (1f - t);
        }
        var clip = AudioClip.Create("tone", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
