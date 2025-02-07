using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class BallCollisionSoundPlayer : MonoBehaviour
{
    public BallCollisionSounds sounds;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minMagnitude = 10f;
    [SerializeField] private float minTimeBetweenSounds = 0.1f;

    private Rigidbody rb;
    private float timer = 0f;
    private bool canPlaySound = true;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!canPlaySound)
        {
            timer += Time.deltaTime;
            if (timer > minTimeBetweenSounds)
            {
                timer = 0f;
                canPlaySound = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        SurfaceMaterial surfaceMaterial = collision.gameObject.GetComponent<SurfaceMaterial>();
        if (!surfaceMaterial)
            return;

        float magnitude = collision.relativeVelocity.magnitude;
        if (canPlaySound && magnitude >= minMagnitude)
            PlaySound(surfaceMaterial.GetMaterialType(), collision.relativeVelocity.magnitude);
    }

    private void PlaySound(SurfaceMaterial.SurfaceMaterialType type, float hitForce)
    {
        audioSource.PlayOneShot(sounds.GetCollisionSound(type));
        audioSource.gameObject.GetComponent<AudioVolumeChanger>().OverrideInitialVolume(Mathf.Min(1f, hitForce / 59f));
        canPlaySound = false;
    }
}
