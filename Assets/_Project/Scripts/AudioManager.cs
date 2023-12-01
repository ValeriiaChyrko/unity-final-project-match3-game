using UnityEngine;

namespace Match3._Project.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour {
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip deselect;
        [SerializeField] private AudioClip match;
        [SerializeField] private AudioClip noMatch;
        [SerializeField] private AudioClip woosh;
        [SerializeField] private AudioClip pop;
        
        private AudioSource _audioSource;
        
        private void OnValidate() {
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        }

        public void PlayClick() => _audioSource.PlayOneShot(click);
        public void PlayDeselect() => _audioSource.PlayOneShot(deselect);
        public void PlayMatch() => _audioSource.PlayOneShot(match);
        public void PlayNoMatch() => _audioSource.PlayOneShot(noMatch);
        public void PlayWoosh() => PlayRandomPitch(woosh);
        public void PlayPop() => PlayRandomPitch(pop);

        private void PlayRandomPitch(AudioClip audioClip) {
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(audioClip);
            _audioSource.pitch = 1f;
        }
    }
}