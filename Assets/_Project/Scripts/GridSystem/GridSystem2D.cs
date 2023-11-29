using System;
using Match3._Project.Scripts.Convertors;
using TMPro;
using UnityEngine;

namespace Match3._Project.Scripts.GridSystem
{
    /// <summary>
    /// Клас для представлення двовимірного сіткового массиву та його обробки.
    /// </summary>
    public class GridSystem2D<T>
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _origin;
        private readonly T[,] _gridArray;

        private readonly CoordinateConverter _coordinateConverter;
        public event Action<int, int, T> OnValueChangeEvent;

        /// <summary>
        /// Конструктор для ініціалізації сітки.
        /// </summary>
        public GridSystem2D(int width, int height, float cellSize, Vector3 origin, CoordinateConverter coordinateConverter, bool debug) {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;
            _coordinateConverter = coordinateConverter ?? new VerticalConverter();

            _gridArray = new T[width, height];

            if (debug) {
                DrawDebugLines();
            }
        }

        /// <summary>
        /// Встановлює значення елемента сітки за його світовими координатами.
        /// </summary>
        public void SetValue(Vector3 worldPosition, T value) {
            var pos = _coordinateConverter.WorldToGrid(worldPosition, _cellSize, _origin);
            SetValue(pos.x, pos.y, value);
        }

        /// <summary>
        /// Встановлює значення елемента сітки за його індексами.
        /// </summary>
        public void SetValue(int x, int y, T value) {
            if (!IsValid(x, y)) return;

            _gridArray[x, y] = value;
            OnValueChangeEvent?.Invoke(x, y, value);
        }

        /// <summary>
        /// Отримує значення елемента сітки за його світовими координатами.
        /// </summary>
        public T GetValue(Vector3 worldPosition) {
            var pos = GetXY(worldPosition);
            return GetValue(pos.x, pos.y);
        }

        /// <summary>
        /// Отримує значення елемента сітки за його індексами.
        /// </summary>
        public T GetValue(int x, int y) {
            return IsValid(x, y) ? _gridArray[x, y] : default;
        }

        /// <summary>
        /// Перевіряє, чи вказані індекси є дійсними для сітки.
        /// </summary>
        bool IsValid(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

        /// <summary>
        /// Отримує індекси сітки за його світовими координатами.
        /// </summary>
        public Vector2Int GetXY(Vector3 worldPosition) => _coordinateConverter.WorldToGrid(worldPosition, _cellSize, _origin);

        /// <summary>
        /// Отримує світові координати центру сіткового елемента за його індексами.
        /// </summary>
        public Vector3 GetWorldPositionCenter(int x, int y) => _coordinateConverter.GridToWorldCenter(x, y, _cellSize, _origin);

        /// <summary>
        /// Отримує світові координати сіткового елемента за його індексами.
        /// </summary>
        Vector3 GetWorldPosition(int x, int y) => _coordinateConverter.GridToWorld(x, y, _cellSize, _origin);

        /// <summary>
        /// Створює сітку для вертикальної орієнтації.
        /// </summary>
        public static GridSystem2D<T> VerticalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false) {
            return new GridSystem2D<T>(width, height, cellSize, origin, new VerticalConverter(), debug);
        }

        /// <summary>
        /// Створює сітку для горизонтальної орієнтації.
        /// </summary>
        public static GridSystem2D<T> HorizontalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false) {
            return new GridSystem2D<T>(width, height, cellSize, origin, new HorizontalConverter(), debug);
        }

        /// <summary>
        /// Малює лінії для дебагу.
        /// </summary>
        private void DrawDebugLines() {
            const float duration = 100f;
            var parent = new GameObject("Debugging");

            for (var x = 0; x < _width; x++) {
                for (var y = 0; y < _height; y++) {
                    CreateWorldText(parent, x + "," + y, GetWorldPositionCenter(x, y), _coordinateConverter.Forward);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, duration);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, duration);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.white, duration);
            Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.white, duration);
        }

        /// <summary>
        /// Створює текст у світі для дебагу.
        /// </summary>
        TextMeshPro CreateWorldText(GameObject parent, string text, Vector3 position, Vector3 dir,
            int fontSize = 2, Color color = default, TextAlignmentOptions textAnchor = TextAlignmentOptions.Center, int sortingOrder = 0) 
        {
            var gameObject = new GameObject("DebugText_" + text, typeof(TextMeshPro));
            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.position = position;
            gameObject.transform.forward = dir;

            var textMeshPro = gameObject.GetComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color == default ? Color.white : color;
            textMeshPro.alignment = textAnchor;
            textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMeshPro;
        }
    }
}
