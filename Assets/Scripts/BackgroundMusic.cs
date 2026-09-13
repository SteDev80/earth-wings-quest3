using UnityEngine;

namespace EarthWings
{
    public sealed class BackgroundMusic : MonoBehaviour
    {
        const string ResourceName = "BackgroundMusic";
        const float DefaultVolume = 0.22f;

        void Awake()
        {
            var clip = Resources.Load<AudioClip>(ResourceName);
            if (!clip)
            {
                Debug.Log("Background music not found. Add an authorized audio file as Assets/Resources/BackgroundMusic.mp3, .ogg or .wav.");
                return;
            }

            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = DefaultVolume;
            source.priority = 128;
            source.Play();
        }
    }
}
