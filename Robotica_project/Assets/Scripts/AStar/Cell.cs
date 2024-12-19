using UnityEngine;

public class Cell
    {
        private Vector3 worldPosition;
        private int x, z;
        private bool isWalkable;

        public Cell(Vector3 position, int x, int z, bool isWalkable)
        {
            this.worldPosition = position;
            this.isWalkable = isWalkable;
            this.x = x;
            this.z = z;
        }
        public override string ToString()
        {
            return $"Cell [x: {this.x}, y: {this.z}, pos: {this.worldPosition}, walkable: {this.isWalkable}]";
        }

        // Metodo per ottenere la posizione nel mondo della cella
        public Vector3 GetWorldPosition()
        {
            return this.worldPosition;
        }

        // Metodo per ottenere la posizione della cella nella griglia
        public Vector3 GetGridPosition()
        {
            return new Vector3(x, -0.01f, z);
        }

        // Metodo per determinare settare la cella come percorribile o meno
        public void SetWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }

        // Metodo per ottenere se la cella è percorribile
        public bool IsWalkable()
        {
            return this.isWalkable;
        }

    }