using System;
using Match3._Project.Scripts.GridSystem;
using UnityEngine;

namespace Match3._Project.Scripts
{
    public class Match3 : MonoBehaviour
    {
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 originPosition = Vector3.zero;
        [SerializeField] private bool debug = true;

       private GridSystem2D<GridObject<Gem>> _grid;
        
        private void Start()
        {
            _grid = GridSystem2D<GridObject<Gem>>.HorizontalGrid(width, height, cellSize, originPosition, debug);
        }
    }
}