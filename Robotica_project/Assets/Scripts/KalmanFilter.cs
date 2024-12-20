using UnityEngine;

public class KalmanFilter
{
    private Vector3 estimatedPosition;
    private Vector3 estimatedVelocity;

    private Matrix4x4 errorCovariance = Matrix4x4.identity;
    private float processNoise = 1.0f;
    private float measurementNoise = 1.0f;

    public KalmanFilter(Vector3 initialPosition)
    {
        estimatedPosition = initialPosition;
        estimatedVelocity = Vector3.zero;
    }

    public Vector3 Predict(float deltaTime)
    {
        estimatedPosition += estimatedVelocity * deltaTime;
        return estimatedPosition;
    }

    public Vector3 Update(Vector3 measuredPosition, float deltaTime)
    {
        // Calcolo del guadagno di Kalman
        float kalmanGain = errorCovariance.m00 / (errorCovariance.m00 + measurementNoise);

        // Aggiornamento della stima
        estimatedPosition += kalmanGain * (measuredPosition - estimatedPosition);
        estimatedVelocity = (measuredPosition - estimatedPosition) / deltaTime;

        // Aggiornamento della covarianza dell'errore
        errorCovariance.m00 = (1 - kalmanGain) * errorCovariance.m00 + processNoise;

        return estimatedPosition;
    }
}
