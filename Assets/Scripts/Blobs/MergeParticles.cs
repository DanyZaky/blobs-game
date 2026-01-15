using UnityEngine;

namespace Blobs.Blobs
{
    /// <summary>
    /// Attach to a particle system prefab to handle merge effect.
    /// Use for BlobParticles or create a new MergeParticles prefab.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class MergeParticles : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoDestroy = true;
        [SerializeField] private float destroyDelay = 2f;

        private ParticleSystem ps;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            if (autoDestroy)
            {
                var main = ps.main;
                float totalLifetime = main.duration + main.startLifetime.constantMax;
                Destroy(gameObject, Mathf.Max(totalLifetime, destroyDelay));
            }
        }

        /// <summary>
        /// Set the particle color to match the blob color
        /// </summary>
        public void SetColor(Color color)
        {
            if (ps == null) ps = GetComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = color;
        }

        /// <summary>
        /// Play the particle effect
        /// </summary>
        public void Play()
        {
            if (ps == null) ps = GetComponent<ParticleSystem>();
            ps.Play();
        }
    }
}
