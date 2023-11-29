using UnityEngine;

namespace Match3._Project.Scripts.Convertors
{
    /// <summary>
    /// Абстрактний клас для конвертації координат між сіткою і світом.
    /// </summary>
    public abstract class CoordinateConverter {
        /// <summary>
        /// Перетворює координати сітки в координати світу.
        /// </summary>
        public abstract Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin);

        /// <summary>
        /// Перетворює координати сітки в координати центру світу.
        /// </summary>
        public abstract Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin);

        /// <summary>
        /// Перетворює координати світу в координати сітки.
        /// </summary>
        public abstract Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin);

        /// <summary>
        /// Визначає вектор напрямку у світових координатах.
        /// </summary>
        public abstract Vector3 Forward { get; }
    }
}