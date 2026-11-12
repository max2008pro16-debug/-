using System;
using UnityEngine;

namespace VirusInside
{
    /// <summary>Крошечный процедурный звуковой движок: все эффекты синтезируются в рантайме.</summary>
    public class VirusAudio : MonoBehaviour
    {
        AudioSource source;
        AudioClip pickup, note, hurt, heal, click, growl, success, fail, ending, door, step;
        float nextStep;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.55f;

            pickup = Make("pickup", 0.22f, t =>
                Mathf.Sin(Phase(t, 880f, 1480f, 0.22f)) * Env(t, 0.22f, 0.005f, 0.9f) * 0.35f);
            note = Make("note", 0.16f, t =>
                (Mathf.Sin(Phase(t, 520f, 760f, 0.16f)) * 0.25f + Noise(t * 900f) * 0.18f) *
                Env(t, 0.16f, 0.005f, 0.8f));
            hurt = Make("hurt", 0.5f, t =>
                (Saw(Phase(t, 130f, 55f, 0.5f)) * 0.4f + Noise(t * 400f) * 0.25f) *
                Env(t, 0.5f, 0.005f, 0.7f));
            heal = Make("heal", 0.55f, t =>
                (Mathf.Sin(t * 2f * Mathf.PI * 523f) + Mathf.Sin(t * 2f * Mathf.PI * 784f) * 0.7f) *
                0.18f * Env(t, 0.55f, 0.02f, 0.9f));
            click = Make("click", 0.06f, t =>
                Mathf.Sign(Mathf.Sin(t * 2f * Mathf.PI * 1500f)) * 0.15f * Env(t, 0.06f, 0.001f, 0.9f));
            growl = Make("growl", 0.7f, t =>
                (Mathf.Sin(t * 2f * Mathf.PI * (62f + Mathf.Sin(t * 30f) * 12f)) * 0.32f +
                 Noise(t * 160f) * 0.2f) * Env(t, 0.7f, 0.05f, 0.8f));
            success = Make("success", 0.34f, t =>
                Mathf.Sin(Phase(t, 440f, 1100f, 0.34f)) * 0.3f * Env(t, 0.34f, 0.005f, 0.85f));
            fail = Make("fail", 0.45f, t =>
                Mathf.Sin(Phase(t, 320f, 80f, 0.45f)) * 0.32f * Env(t, 0.45f, 0.005f, 0.8f));
            ending = Make("ending", 1.8f, t =>
                (Mathf.Sin(t * 2f * Mathf.PI * 110f) + Mathf.Sin(t * 2f * Mathf.PI * 164.8f) * 0.6f +
                 Mathf.Sin(t * 2f * Mathf.PI * 220f) * 0.3f) * 0.14f * Env(t, 1.8f, 0.2f, 0.9f));
            door = Make("door", 0.35f, t =>
                (Noise(t * 260f) * 0.22f + Mathf.Sin(t * 2f * Mathf.PI * 90f) * 0.25f) *
                Env(t, 0.35f, 0.01f, 0.85f));
            step = Make("step", 0.09f, t => Noise(t * 600f) * 0.13f * Env(t, 0.09f, 0.002f, 0.9f));
        }

        void Play(AudioClip clip, float volume = 1f)
        {
            if (clip != null && source != null) source.PlayOneShot(clip, volume);
        }

        public void Pickup() => Play(pickup, 0.9f);
        public void Note() => Play(note, 0.9f);
        public void Hurt() => Play(hurt, 1f);
        public void Heal() => Play(heal, 1f);
        public void Click() => Play(click, 0.6f);
        public void Growl() => Play(growl, 0.8f);
        public void Success() => Play(success, 1f);
        public void Fail() => Play(fail, 1f);
        public void Ending() => Play(ending, 1f);
        public void Door() => Play(door, 0.8f);

        public void Step()
        {
            if (Time.time < nextStep) return;
            nextStep = Time.time + 0.33f;
            Play(step, 0.45f);
        }

        // ------------------------------------------------------------ synthesis
        static AudioClip Make(string name, float duration, Func<float, float> wave)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(duration * rate);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                data[i] = Mathf.Clamp(wave(t), -1f, 1f);
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Phase(float t, float f0, float f1, float dur)
        {
            float k = (f1 - f0) / Mathf.Max(0.0001f, dur);
            return 2f * Mathf.PI * (f0 * t + 0.5f * k * t * t);
        }

        static float Env(float t, float dur, float attack, float curve)
        {
            if (t < attack) return t / Mathf.Max(0.0001f, attack);
            float x = 1f - (t - attack) / Mathf.Max(0.0001f, dur - attack);
            return Mathf.Pow(Mathf.Clamp01(x), curve);
        }

        static float Noise(float seed)
        {
            return Mathf.Repeat(Mathf.Sin(seed * 12.9898f) * 43758.5453f, 1f) * 2f - 1f;
        }

        static float Saw(float phase)
        {
            return Mathf.Repeat(phase / (2f * Mathf.PI), 1f) * 2f - 1f;
        }
    }
}
