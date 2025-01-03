using UnityEngine;
using System.Collections.Generic;

public class ParticleFilter : MonoBehaviour 
{
    [Header("Particle Filter Parameters")]
    public int numParticles = 100;
    public float positionNoise = 0.02f;
    public float measurementNoise = 0.04f;

    private List<ParticleState> previousParticles;
    private List<ParticleState> temporaryParticles;
    private List<ParticleState> currentParticles;

    [System.Serializable]
    private class ParticleState
    {
        public Vector3 state;
        public float weight;

        public ParticleState(Vector3 state, float weight)
        {
            this.state = state;
            this.weight = weight;
        }
    }

    void Start()
    {
        InitializeParticles();
    }

    private void InitializeParticles()
    {
        previousParticles = new List<ParticleState>();
        temporaryParticles = new List<ParticleState>();
        currentParticles = new List<ParticleState>();

        Vector3 initialPosition = transform.position;
        
        for (int i = 0; i < numParticles; i++)
        {
            previousParticles.Add(new ParticleState(
                initialPosition + Random.insideUnitSphere * positionNoise,
                1f / numParticles
            ));
        }
    }

    public void UpdateParticles(Vector3 controlInput, Vector3 measurement)
    {
        temporaryParticles.Clear();
        currentParticles.Clear();

        float totalWeight = 0f;
        
        for (int i = 0; i < numParticles; i++)
        {
            Vector3 newState = SampleNewState(previousParticles[i].state, controlInput);
            float weight = CalculateWeight(newState, measurement);
            totalWeight += weight;
            temporaryParticles.Add(new ParticleState(newState, weight));
        }

        if (totalWeight <= 0f)
        {
            for (int i = 0; i < numParticles; i++)
            {
                temporaryParticles[i].weight = 1f / numParticles;
            }
        }
        else
        {
            // Normalizza i pesi
            for (int i = 0; i < numParticles; i++)
            {
                temporaryParticles[i].weight /= totalWeight;
            }
        }

        currentParticles = SystematicResampling(temporaryParticles, numParticles);
        previousParticles = new List<ParticleState>(currentParticles);
    }

    private Vector3 SampleNewState(Vector3 previousState, Vector3 controlInput)
    {
        Vector3 predictedState = Vector3.Lerp(previousState, controlInput, 0);
        Vector3 noise = Random.insideUnitSphere * positionNoise;
        return predictedState + noise;
    }

    private float CalculateWeight(Vector3 state, Vector3 measurement)
    {
        float distance = Vector3.Distance(state, measurement);
        return Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(measurementNoise, 2)));
    }

    private List<ParticleState> SystematicResampling(List<ParticleState> particles, int n)
    {
        List<ParticleState> resampledParticles = new List<ParticleState>();
        float[] cumulativeSums = new float[n];
        
        // Calcola le somme cumulative
        cumulativeSums[0] = particles[0].weight;
        for (int idx = 1; idx < n; idx++)
        {
            cumulativeSums[idx] = cumulativeSums[idx-1] + particles[idx].weight;
        }

        // Assicurati che l'ultima somma cumulativa sia 1
        cumulativeSums[n-1] = 1f;

        // Inizializza la soglia
        float u = Random.Range(0f, 1f/n);
        int currentIndex = 0;

        // Seleziona i campioni
        for (int j = 0; j < n; j++)
        {
            while (currentIndex < n-1 && u > cumulativeSums[currentIndex])
            {
                currentIndex++;
            }

            resampledParticles.Add(new ParticleState(
                particles[currentIndex].state,
                1f/n
            ));

            u += 1f/n;
        }

        return resampledParticles;
    }

    public Vector3 EstimatePosition()
    {
        Vector3 estimatedPosition = Vector3.zero;
        
        if (currentParticles == null || currentParticles.Count == 0)
        {
            return transform.position;
        }

        foreach (var particle in currentParticles)
        {
            estimatedPosition += particle.state;
        }
        return estimatedPosition / currentParticles.Count;
    }

    // Metodo di debug per visualizzare le particelle
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || currentParticles == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var particle in currentParticles)
        {
            Gizmos.DrawWireSphere(particle.state, 0.1f);
        }

        // Disegna la posizione stimata in un colore diverso
        Gizmos.color = Color.red;
        Vector3 estimatedPos = EstimatePosition();
        Gizmos.DrawWireSphere(estimatedPos, 0.2f);
    }
}