using System.Collections.Generic;
using Car.Models.PhysicsModels.WheelComponents;
using UnityEngine;
using UnityEngine.Audio;

namespace Car.Models.PhysicsModels.WheelModels
{
    public class WheelSoundSystemModel
    {
        private List<WheelSoundComponent> m_wheelSoundComponents = new List<WheelSoundComponent>();
        private AudioClip m_skidSoundClip;
        private AudioMixerGroup m_wheelAudioMixer;

        public WheelSoundSystemModel(List<Transform> wheelTransforms)
        {
            m_skidSoundClip = Resources.Load<AudioClip>("Skid");
            m_wheelAudioMixer = Resources.Load<AudioMixerGroup>("Wheel");
            
            for (int i = 0; i < wheelTransforms.Count; i++)
            {
                m_wheelSoundComponents.Add(new WheelSoundComponent(wheelTransforms[i], m_skidSoundClip, m_wheelAudioMixer));
            }
        }

        public void UpdateWheelSounds(List<float> slipForces)
        {
            for (int i = 0; i < m_wheelSoundComponents.Count; i++)
            {
                m_wheelSoundComponents[i].ApplyAudio(slipForces[i]);
            }
        }
    }
}
