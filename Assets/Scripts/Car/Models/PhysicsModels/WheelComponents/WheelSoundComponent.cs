using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

namespace Car.Models.PhysicsModels.WheelComponents
{
    public class WheelSoundComponent
    {
        private AudioSource m_audioSource;

        public WheelSoundComponent(Transform wheel, AudioClip skidSoundClip, AudioMixerGroup mixer)
        {
            m_audioSource = wheel.AddComponent<AudioSource>();
            m_audioSource.clip = skidSoundClip;
            m_audioSource.loop = true;
            m_audioSource.outputAudioMixerGroup = mixer;
            m_audioSource.Play();
        }

        public void ApplyAudio(float slipForce)
        {
            m_audioSource.volume = Mathf.Clamp(Mathf.Abs(slipForce), 0, 1);
        }
    }
}
