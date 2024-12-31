using UnityEngine;

public class KalmanFilter
{
    public Vector3 x; // Stato (posizione)
    public Matrix4x4 P; // Matrice di covarianza (4x4)

    private Matrix4x4 F; // Matrice di transizione
    private Matrix4x4 H; // Matrice di osservazione
    private Matrix4x4 Q; // Rumore di processo
    private Matrix4x4 R; // Rumore di misurazione

    public KalmanFilter()
    {
        x = new Vector3(0, 0, 0); // Stato iniziale
        P = Matrix4x4.identity; // Incertezza iniziale

        // Inizializzazione delle matrici
        F = Matrix4x4.identity; // Matrice di transizione
        H = Matrix4x4.identity; // Matrice di osservazione
        Q = MultiplyMatrixByScalar(Matrix4x4.identity, 0.01f); // Rumore di processo
        R = MultiplyMatrixByScalar(Matrix4x4.identity, 1f); // Rumore di misurazione
    }

    // Funzione per moltiplicare una matrice 4x4 per uno scalare
    private Matrix4x4 MultiplyMatrixByScalar(Matrix4x4 matrix, float scalar)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 16; i++)
        {
            result[i] = matrix[i] * scalar;
        }

        return result;
    }

    // Funzione per sommare due matrici 4x4
    private Matrix4x4 AddMatrices(Matrix4x4 a, Matrix4x4 b)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 16; i++)
        {
            result[i] = a[i] + b[i];
        }

        return result;
    }

    // Funzione per sottrarre due matrici 4x4
    private Matrix4x4 SubtractMatrices(Matrix4x4 a, Matrix4x4 b)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 16; i++)
        {
            result[i] = a[i] - b[i];
        }

        return result;
    }

    // Funzione per moltiplicare due matrici 4x4
    private Matrix4x4 MultiplyMatrices(Matrix4x4 a, Matrix4x4 b)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                result[i * 4 + j] = 0;

                for (int k = 0; k < 4; k++)
                {
                    result[i * 4 + j] += a[i * 4 + k] * b[k * 4 + j];
                }
            }
        }

        return result;
    }

    // Predizione dello stato
    public void Predict()
    {
        // Predizione dello stato successivo
        x = F.MultiplyVector(x);

        // Predizione della covarianza
        P = AddMatrices(MultiplyMatrices(F, MultiplyMatrices(P, F.transpose)), Q);
    }

    // Aggiornamento dello stato con la misura
    public void Update(Vector3 measurement)
    {
        // Calcolo dell'innovazione
        Vector3 y = measurement - H.MultiplyVector(x);

        // Calcolo della matrice di guadagno di Kalman
        Matrix4x4 S = AddMatrices(MultiplyMatrices(H, MultiplyMatrices(P, H.transpose)), R);
        Matrix4x4 K = MultiplyMatrices(P, MultiplyMatrices(H.transpose, S.inverse));

        // Aggiornamento dello stato
        x = x + K.MultiplyVector(y);

        // Aggiornamento della covarianza
        P = MultiplyMatrices(SubtractMatrices(Matrix4x4.identity, MultiplyMatrices(K, H)), P);
    }
}
