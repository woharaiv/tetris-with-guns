using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ParticleGroup
{
    public string name;
    public List<Particle> particles;
}

public enum SpawnShape
{
    CIRCLE,
    BOX
}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    [SerializeField] float minLaunchAngle;
    [SerializeField] float maxLaunchAngle;
    [Space]
    [SerializeField] float minLaunchSpeed;
    [SerializeField] float maxLaunchSpeed;
    [Space]
    [SerializeField] float minSpinSpeed;
    [SerializeField] float maxSpinSpeed;


    public List<ParticleGroup> _particleGroups;
    Dictionary<string, ParticleGroup> particleGroups = new Dictionary<string, ParticleGroup>();

    private void Awake()
    {
        instance = this;
        foreach (var particleGroup in _particleGroups)
        {
            particleGroups.Add(particleGroup.name, particleGroup);
        }
    }

    public bool SpawnParticles(string group, int count, Vector2 origin, out List<Particle> spawnedParticles, SpawnShape spawnShape = SpawnShape.CIRCLE, float spawnRadius = 0.0f)
    {
        if (particleGroups.TryGetValue(group, out ParticleGroup particleGroup))
        {
            spawnedParticles = new List<Particle>();
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPoint = origin;
                if (spawnRadius > 0.0f)
                {
                    switch (spawnShape)
                    {
                        case SpawnShape.CIRCLE:
                            spawnPoint += Random.insideUnitCircle * spawnRadius;
                            break;
                        case SpawnShape.BOX:
                            spawnPoint += new Vector2(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius));
                            break;
                    }
                }

                GameObject spawnedParticle = Instantiate(particleGroup.particles[Random.Range(0, particleGroup.particles.Count)].gameObject, spawnPoint, Quaternion.Euler(0, 0, Random.Range(0, 360.0f)), null);
                spawnedParticles.Add(spawnedParticle.GetComponent<Particle>());
            }
            return true;
        }
        else
        {
            Debug.LogWarning("Tried to spawn particles from group \"" +  group + "\" but no particle group of that name was found");
            spawnedParticles = null;
            return false;
        }
    }

    public bool SpawnAndLaunchParticles(string group, int count, Vector2 origin, out List<Particle> spawnedParticles, SpawnShape spawnShape = SpawnShape.CIRCLE, float spawnRadius = 0.0f)
    {
        if (SpawnParticles(group, count, origin, out List<Particle> spawned, spawnShape, spawnRadius))
        {
            spawnedParticles = spawned;
            foreach (var particle in spawnedParticles)
            {
                float launchAngle = Random.Range(minLaunchAngle, maxLaunchAngle);
                float launchSpeed = Random.Range(minLaunchSpeed, maxLaunchSpeed);
                Vector2 launchDir = new Vector2(Mathf.Cos(launchAngle), Mathf.Sin(launchAngle));
                particle.gameObject.GetComponent<Rigidbody2D>().AddForce(launchDir * launchSpeed);

                float spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
                particle.gameObject.GetComponent<Rigidbody2D>().AddTorque(spinSpeed);
            }
            return true;
        }
        else
        {
            spawnedParticles = null;
            return false;
        }
    }


}
