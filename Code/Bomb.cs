using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    // Activate this script when bomb explode. Shouldn't be destroyed here either
    [SerializeField] private GameObject rootObject;
    [SerializeField] private GameObject mesh;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fuse;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private GameObject explosionParticleEffect;

    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float despawnDelay = 2f;

    private List<NetworkObject> objectsToDespawn = new List<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        StartCoroutine(ExplosionRoutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        DestructibleObject otherObject = other.gameObject.GetComponent<DestructibleObject>();

        if (otherObject != null)
        {
            var networkObject = otherObject.GetRootNetworkObject();
            if (networkObject != null)
                objectsToDespawn.Add(networkObject);
        }
    }

    private IEnumerator ExplosionRoutine()
    {
        audioSource.PlayOneShot(fuse);
        yield return new WaitForSeconds(explosionDelay);
        HideMesh();
        explosionParticleEffect.SetActive(true);
        audioSource.PlayOneShot(explosion);

        if (IsHost)
        {
            DespawnObjectsInRange();

            yield return new WaitForSeconds(despawnDelay);
            DespawnBomb();
        }
    }

    private void DespawnObjectsInRange()
    {
        foreach (var obj in objectsToDespawn)
            obj.Despawn();
    }

    private void HideMesh()
    {
        mesh.SetActive(false);
    }

    private void DespawnBomb()
    {
        rootObject.GetComponent<NetworkObject>().Despawn();
    }
}
