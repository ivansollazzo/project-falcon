using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;  // Per la parallelizzazione

public class ParticleFilter : MonoBehaviour
{
    [Header("Particle Filter Parameters")]
    public int numParticles = 1000; // Ridotto il numero di particelle per migliorare la velocità
    public float positionNoise = 0.1f;
    public float measurementNoise = 0.1f;

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
                initialPosition + Random.insideUnitSphere * positionNoise,  // Qui la generazione è ancora nel thread principale
                1f / numParticles
            ));
        }
    }

    public void UpdateParticles(Vector3 controlInput, Vector3 measurement)
    {
        temporaryParticles.Clear();
        currentParticles.Clear();

        float totalWeight = 0f;

        // Genera un array di rumori per ogni particella prima di avviare il thread parallelo
        Vector3[] noiseArray = new Vector3[numParticles];
        for (int i = 0; i < numParticles; i++)
        {
            noiseArray[i] = Random.insideUnitSphere * positionNoise;
        }

        // Parallelizzazione della predizione e calcolo dei pesi
        Parallel.For(0, numParticles, i =>
        {
            Vector3 newState = SampleNewState(previousParticles[i].state, controlInput, noiseArray[i]);
            float weight = CalculateWeight(newState, measurement);

            lock (temporaryParticles)  // Protezione per l'accesso alle risorse condivise
            {
                temporaryParticles.Add(new ParticleState(newState, weight));
            }
        });

        // Calcolo totale del peso e normalizzazione
        for (int i = 0; i < numParticles; i++)
        {
            totalWeight += temporaryParticles[i].weight;
        }

        if (totalWeight > 0f)
        {
            for (int i = 0; i < numParticles; i++)
            {
                temporaryParticles[i].weight /= totalWeight;
            }
        }
        else
        {
            // Normalizzazione dei pesi nel caso di un peso totale di zero
            for (int i = 0; i < numParticles; i++)
            {
                temporaryParticles[i].weight = 1f / numParticles;
            }
        }

        // Resampling sistematico
        currentParticles = SystematicResampling(temporaryParticles, numParticles);

        // Stabilizzazione delle particelle (per evitare oscillazioni)
        StabilizeParticles();

        // Aggiorna la lista di particelle precedenti
        previousParticles = new List<ParticleState>(currentParticles);
    }

    private Vector3 SampleNewState(Vector3 previousState, Vector3 controlInput, Vector3 noise)
    {
        // Predici la nuova posizione in base al controllo (ad esempio velocità e direzione)
        Vector3 predictedState = previousState + controlInput;

        // Aggiungi rumore per simulare l'incertezza
        return predictedState + noise;
    }

    private float CalculateWeight(Vector3 state, Vector3 measurement)
    {
        float distance = Vector3.Distance(state, measurement);
        
        // Ottimizzazione: se la distanza è troppo grande, assegna direttamente un peso basso
        if (distance > 10f)
            return 0.01f;

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
            cumulativeSums[idx] = cumulativeSums[idx - 1] + particles[idx].weight;
        }

        // Assicurati che l'ultima somma cumulativa sia 1
        cumulativeSums[n - 1] = 1f;

        // Inizializza la soglia
        float u = Random.Range(0f, 1f / n);
        int currentIndex = 0;

        // Seleziona i campioni
        for (int j = 0; j < n; j++)
        {
            while (currentIndex < n - 1 && u > cumulativeSums[currentIndex])
            {
                currentIndex++;
            }

            resampledParticles.Add(new ParticleState(
                particles[currentIndex].state,
                1f / n
            ));

            u += 1f / n;
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

    // Metodo di stabilizzazione per evitare oscillazioni improvvise
    private void StabilizeParticles()
    {
        // Stabilizza la posizione media delle particelle (ad esempio limitando il movimento troppo rapido)
        Vector3 estimatedPosition = EstimatePosition();
        foreach (var particle in currentParticles)
        {
            particle.state = Vector3.Lerp(particle.state, estimatedPosition, 0.1f);
        }
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