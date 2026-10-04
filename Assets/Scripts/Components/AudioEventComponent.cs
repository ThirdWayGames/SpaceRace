using UnityEngine;

namespace Assets.Scripts.Components
{
    public class AudioEventComponent : MutableEventComponent
    {
        public AudioSource AudioSource;

        public AudioClip AudioClip;

        protected override void TriggerEvent()
        {
            if (AudioSource != null && AudioClip != null)
            {
                if (!AudioSource.isPlaying)
                {
                    AudioSource.PlayOneShot(AudioClip);
                }
            }
        }
    }
}