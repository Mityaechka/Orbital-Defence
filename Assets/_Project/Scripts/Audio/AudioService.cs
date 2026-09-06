using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip cannonShotClip;
        [SerializeField] private AudioClip coreShotClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip enemyDestroyedClip;
        [SerializeField] private AudioClip coreDamageClip;
        [SerializeField] private AudioClip shieldBlockClip;
        [SerializeField] private AudioClip resourceCollectedClip;
        [SerializeField] private AudioClip buildClip;
        [SerializeField] private AudioClip upgradeClip;
        [SerializeField] private AudioClip sellClip;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.7f;
        [SerializeField, Range(0f, 0.5f)] private float pitchJitter = 0.06f;
        [SerializeField] private float hitCooldown = 0.045f;

        private static AudioService instance;
        private float nextHitTime;

        private void Awake()
        {
            source ??= GetComponent<AudioSource>();
            source.playOnAwake = false;
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void PlayCannonShot()
        {
            Play(instance != null ? instance.cannonShotClip : null, 0.75f);
        }

        public static void PlayCoreShot()
        {
            Play(instance != null ? instance.coreShotClip : null, 0.55f);
        }

        public static void PlayHit()
        {
            if (instance == null || Time.unscaledTime < instance.nextHitTime)
            {
                return;
            }

            instance.nextHitTime = Time.unscaledTime + instance.hitCooldown;
            Play(instance.hitClip, 0.38f);
        }

        public static void PlayEnemyDestroyed()
        {
            Play(instance != null ? instance.enemyDestroyedClip : null, 0.65f);
        }

        public static void PlayCoreDamage()
        {
            Play(instance != null ? instance.coreDamageClip : null, 0.75f);
        }

        public static void PlayShieldBlock()
        {
            Play(instance != null ? instance.shieldBlockClip : null, 0.75f);
        }

        public static void PlayBuild()
        {
            Play(instance != null ? instance.buildClip : null, 0.58f);
        }

        public static void PlayResourceCollected()
        {
            Play(instance != null ? instance.resourceCollectedClip : null, 0.30f);
        }

        public static void PlayUpgrade()
        {
            Play(instance != null ? instance.upgradeClip : null, 0.62f);
        }

        public static void PlaySell()
        {
            Play(instance != null ? instance.sellClip : null, 0.50f);
        }

        private static void Play(AudioClip clip, float volumeScale)
        {
            if (instance == null || instance.source == null || clip == null)
            {
                return;
            }

            instance.source.pitch = Random.Range(1f - instance.pitchJitter, 1f + instance.pitchJitter);
            instance.source.PlayOneShot(clip, instance.masterVolume * volumeScale);
        }
    }
}
