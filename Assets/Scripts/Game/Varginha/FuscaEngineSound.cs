using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Creates a short retro engine rev without an external audio asset.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class FuscaEngineSound : MonoBehaviour
    {
        [SerializeField] private float duration = 2.4f;
        [SerializeField] private float volume = 0.42f;
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        public void PlayRev()
        {
            if (_source == null) _source = GetComponent<AudioSource>();
            _source.Stop();
            _source.clip = CreateRevClip();
            _source.volume = volume;
            _source.Play();
        }

        private AudioClip CreateRevClip()
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(duration * sampleRate);
            var data = new float[samples];
            float phaseA = 0f;
            float phaseB = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float progress = t / duration;
                float frequency = Mathf.Lerp(48f, 138f, Mathf.SmoothStep(0f, 1f, progress));
                phaseA += frequency / sampleRate;
                phaseB += (frequency * 2.02f) / sampleRate;
                float pulse = Mathf.Sin(phaseA * Mathf.PI * 2f) * 0.52f + Mathf.Sin(phaseB * Mathf.PI * 2f) * 0.18f;
                float rumble = Mathf.PerlinNoise(i * 0.015f, 0.37f) * 2f - 1f;
                float envelope = Mathf.Clamp01(t / 0.12f) * Mathf.Clamp01((duration - t) / 0.28f);
                data[i] = (pulse + rumble * 0.12f) * envelope;
            }

            var clip = AudioClip.Create("Fusca_Vruuum", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
