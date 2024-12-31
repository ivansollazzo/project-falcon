﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaypointsFree
{
    public class WaypointsTraveler : MonoBehaviour
    {
        [Tooltip("WaypointsGroup gameobject containing the waypoints to travel.")]
        public WaypointsGroup Waypoints = null;

        [Tooltip("Movement and look-at constraints.")]
        public PositionConstraint XYZConstraint = PositionConstraint.XYZ;

        [Tooltip("Auto-start movement if true.")]
        public bool AutoStart = false;

        public float MoveSpeed = 5.0f;
        public float LookAtSpeed = 3.0f;

        [Tooltip("Starts movement from the position vector at this index. Dependent upon StartTravelDirection!")]
        public int StartIndex = 0;

        [Tooltip("Immediately move starting position to postion at StartIndex.")]
        public bool AutoPositionAtStart = true;

        [Tooltip("Initial direction of travel through the positions list.")]
        public TravelDirection StartTravelDirection = TravelDirection.FORWARD;

        [Tooltip("Movement behavior to apply when last postion reached.")]
        public EndpointBehavior EndReachedBehavior = EndpointBehavior.LOOP;

        [Tooltip("Movement function type")]
        public MoveType StartingMovementType = MoveType.LERP;

        public float DecelerationRate = 5.0f;
        private float currentSpeed;

        public bool IsMoving
        {
            get { return isMoving; }
        }

        delegate bool MovementFunction();
        MovementFunction moveFunc = null;

        int positionIndex = -1;
        List<Waypoint> waypointsList;

        Vector3 nextPosition;
        Vector3 startPosition;
        Vector3 destinationPosition;

        float distanceToNextWaypoint;
        float distanceTraveled = 0;
        float timeTraveled = 0;

        int travelIndexCounter = 1;

        bool isMoving = false;

        Vector3 positionOriginal;
        Quaternion rotationOriginal;
        float moveSpeedOriginal = 0;
        float lookAtSpeedOriginal = 0;

        public void ResetTraveler()
        {
            transform.position = positionOriginal;
            transform.rotation = rotationOriginal;

            MoveSpeed = moveSpeedOriginal;
            LookAtSpeed = lookAtSpeedOriginal;

            StartAtIndex(StartIndex, AutoPositionAtStart);
            SetNextPosition();
            travelIndexCounter = StartTravelDirection == TravelDirection.REVERSE ? -1 : 1;

            if (StartingMovementType == MoveType.LERP)
                moveFunc = MoveLerpSimple;
            else if (StartingMovementType == MoveType.FORWARD_TRANSLATE)
                moveFunc = MoveForwardToNext;
        }

        void Start()
        {
            moveSpeedOriginal = MoveSpeed;
            lookAtSpeedOriginal = LookAtSpeed;

            positionOriginal = transform.position;
            rotationOriginal = transform.rotation;

            currentSpeed = moveSpeedOriginal;

            ResetTraveler();

            Move(AutoStart);
        }

        public void Move(bool tf)
        {
            isMoving = tf;
        }

        private void Awake()
        {
            if (Waypoints != null)
            {
                waypointsList = Waypoints.waypoints;
            }
        }

        void Update()
        {
            if (CheckForPedestrians() || CheckForCars())
            {
                currentSpeed = Mathf.Max(0, currentSpeed - DecelerationRate * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.Min(moveSpeedOriginal, currentSpeed + DecelerationRate * Time.deltaTime);
            }

            if (isMoving && moveFunc != null)
            {
                bool arrivedAtDestination = moveFunc();
                if (arrivedAtDestination)
                {
                    SetNextPosition();
                }
            }
        }

        private bool CheckForPedestrians()
        {
            float detectionDistance = 3.5f; // Distanza di rilevamento pedoni
            float coneAngle = 30f; // Angolo del cono per pedoni
            int numRays = 10; // Numero di raggi nel cono
            Vector3 rayOrigin = transform.position; 
            Vector3 rayDirection = transform.forward; 

            bool pedestrianDetected = false;

            float halfAngle = coneAngle / 2f;

            for (int i = 0; i < numRays; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / (numRays - 1));
                Vector3 dir = Quaternion.Euler(0, angle, 0) * rayDirection;

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, dir, out hit, detectionDistance))
                {
                    if (hit.collider.CompareTag("Pedestrian"))
                    {
                        //Debug.Log("Pedone rilevato: " + hit.collider.name);
                        Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.red);
                        pedestrianDetected = true;
                    }
                    else
                    {
                        Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.green);
                    }
                }
            }

            return pedestrianDetected;
        }

        private bool CheckForCars()
        {
            float detectionDistance = 5f; // Distanza di rilevamento per le auto
            float coneAngle = 10f; // Angolo del cono per le auto
            int numRays = 10; // Numero di raggi nel cono
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = transform.forward;

            bool carDetected = false;

            float halfAngle = coneAngle / 2f;

            for (int i = 0; i < numRays; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / (numRays - 1));
                Vector3 dir = Quaternion.Euler(0, angle, 0) * rayDirection;

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, dir, out hit, detectionDistance))
                {
                    if (hit.collider.CompareTag("Car"))
                    {
                        //Debug.Log("Auto rilevata: " + hit.collider.name);
                        Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.blue);
                        carDetected = true;
                    }
                    else
                    {
                        Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.green);
                    }
                }
            }

            return carDetected;
        }

        public void SetWaypointsGroup(WaypointsGroup newGroup)
        {
            Waypoints = newGroup;
            waypointsList = null;
            if (newGroup != null)
            {
                waypointsList = newGroup.waypoints;
            }
        }

        void StartAtIndex(int ndx, bool autoUpdatePosition = true)
        {
            if (StartTravelDirection == TravelDirection.REVERSE)
                ndx = waypointsList.Count - ndx - 1;

            ndx = Mathf.Clamp(ndx, 0, waypointsList.Count - 1);
            positionIndex = ndx - 1;
            if (autoUpdatePosition)
            {
                transform.position = waypointsList[ndx].GetPosition();
            }
        }

        void SetNextPosition()
        {
            int posCount = waypointsList.Count;
            if (posCount > 0)
            {
                if ((positionIndex == 0 && travelIndexCounter < 0) || (positionIndex == posCount - 1 && travelIndexCounter > 0))
                {
                    if (EndReachedBehavior == EndpointBehavior.STOP)
                    {
                        isMoving = false;
                        return;
                    }
                    else if (EndReachedBehavior == EndpointBehavior.PINGPONG)
                    {
                        travelIndexCounter = -travelIndexCounter;
                    }
                }

                positionIndex += travelIndexCounter;

                if (positionIndex >= posCount)
                    positionIndex = 0;
                else if (positionIndex < 0)
                    positionIndex = posCount - 1;

                nextPosition = waypointsList[positionIndex].GetPosition();
                if (XYZConstraint == PositionConstraint.XY)
                {
                    nextPosition.z = transform.position.z;
                }
                else if (XYZConstraint == PositionConstraint.XZ)
                {
                    nextPosition.y = transform.position.y;
                }

                nextPosition.y = positionOriginal.y;

                ResetMovementValues();
            }
        }

        void ResetMovementValues()
        {
            startPosition = transform.position;
            destinationPosition = nextPosition;
            distanceToNextWaypoint = Vector3.Distance(startPosition, destinationPosition);
            distanceTraveled = 0;
            timeTraveled = 0;
        }

        bool MoveLerpSimple()
        {
            if (currentSpeed < 0)
                currentSpeed = 0;

            timeTraveled += Time.deltaTime;
            distanceTraveled += Time.deltaTime * currentSpeed;
            float fracAmount = distanceTraveled / distanceToNextWaypoint;
            transform.position = Vector3.Lerp(startPosition, destinationPosition, fracAmount);
            UpdateLookAtRotation();
            return fracAmount >= 1;
        }

        bool MoveForwardToNext()
        {
            if (currentSpeed < 0)
                currentSpeed = 0;

            float rate = Time.deltaTime * currentSpeed;
            float distance = Vector3.Distance(transform.position, destinationPosition);
            if (distance < rate)
            {
                transform.position = destinationPosition;
                return true;
            }

            if (LookAtSpeed <= 0) LookAtSpeed = float.MaxValue;

            UpdateLookAtRotation();

            Vector3 moveDir = Vector3.forward;
            if (XYZConstraint == PositionConstraint.XY)
            {
                moveDir = Vector3.up;
            }

            transform.Translate(moveDir * rate);

            return false;
        }

        void UpdateLookAtRotation()
        {
            if (LookAtSpeed <= 0) return;

            float step = LookAtSpeed * Time.deltaTime;
            Vector3 targetDir = nextPosition - transform.position;

            if (XYZConstraint == PositionConstraint.XY)
            {
                float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg - 90;
                Quaternion qt = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qt, step);
            }
            else if (XYZConstraint == PositionConstraint.XZ)
            {
                float angle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
                Quaternion qt = Quaternion.AngleAxis(angle, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qt, step);
            }
            else
            {
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
    }
}