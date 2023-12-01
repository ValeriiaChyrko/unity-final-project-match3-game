using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] private bool debug;

        [SerializeField] private Gem gemPrefab;
        [SerializeField] private GemType[] gemTypes;
        [SerializeField] private Ease ease = Ease.InQuad;
        [SerializeField] private GameObject explosion;

        private GridSystem2D<GridObject<Gem>> _grid;
        private AudioManager _audioManager;

        private InputReader _inputReader;
        private Vector2Int _selectedGem = Vector2Int.one * -1;
        private bool _userInitiated;

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _audioManager = GetComponent<AudioManager>();
        }

        private void Start()
        {
            InitializeGrid();
            _inputReader.Fire += OnSelectGem;
        }

        private void OnDestroy()
        {
            _inputReader.Fire -= OnSelectGem;
        }

        private void InitializeGrid()
        {
            _grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, debug);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    CreateGem(x, y);
                }
            }
        }

        private void CreateGem(int x, int y)
        {
            var gem = Instantiate(gemPrefab, _grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
            var gridObject = new GridObject<Gem>(_grid, x, y);
            gridObject.SetValue(gem);
            _grid.SetValue(x, y, gridObject);
        }

        private void OnSelectGem()
        {
            var gridPos = _grid.GetXY(Camera.main!.ScreenToWorldPoint(_inputReader.Selected));

            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

            if (_selectedGem == gridPos)
            {
                DeselectGem();
                _audioManager.PlayDeselect();
            }
            else if (_selectedGem == Vector2Int.one * -1)
            {
                SelectGem(gridPos);
                _audioManager.PlayClick();
            }
            else
            {
                StartCoroutine(RunGameLoop(_selectedGem, gridPos));
            }
        }

        private IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB) {
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));

            List<Vector2Int> matches;

            while ((matches = FindMatches()).Count != 0) {
                // Matches?
                // TODO: Calculate score
                // Make Gems explode
                yield return StartCoroutine(ExplodeGems(matches));
                // Make gems fall
                yield return StartCoroutine(MakeGemsFall());
                // Fill empty spots
                yield return StartCoroutine(FillEmptySpots());
                
                _userInitiated = false;
            }

            // TODO: Check if game is over

            DeselectGem();
        }

        private List<Vector2Int> FindMatches()
        {
            HashSet<Vector2Int> matches = new();

            // Horizontal
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width - 2; x++)
                {
                    var gemA = _grid.GetValue(x, y);
                    var gemB = _grid.GetValue(x + 1, y);
                    var gemC = _grid.GetValue(x + 2, y);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetValue().GetType() != gemB.GetValue().GetType()
                        || gemB.GetValue().GetType() != gemC.GetValue().GetType()) continue;
                    
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x + 1, y));
                    matches.Add(new Vector2Int(x + 2, y));
                }
            }

            // Vertical
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height - 2; y++)
                {
                    var gemA = _grid.GetValue(x, y);
                    var gemB = _grid.GetValue(x, y + 1);
                    var gemC = _grid.GetValue(x, y + 2);

                    if (gemA == null || gemB == null || gemC == null) continue;

                    if (gemA.GetValue().GetType() != gemB.GetValue().GetType()
                        || gemB.GetValue().GetType() != gemC.GetValue().GetType()) continue;
                    
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x, y + 1));
                    matches.Add(new Vector2Int(x, y + 2));
                }
                
                if (matches.Count == 0 && _userInitiated) 
                    _audioManager.PlayNoMatch();
                else if (matches.Count != 0)
                    _audioManager.PlayMatch();
                
            }

            return new List<Vector2Int>(matches);
        }
        
        private IEnumerator ExplodeGems(List<Vector2Int> matches) {
            _audioManager.PlayPop();
            
            foreach (var match in matches) {
                var gem = _grid.GetValue(match.x, match.y).GetValue();
                _grid.SetValue(match.x, match.y, null);
                
                ExplodeVFX(match);
                
                gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);
                
                yield return new WaitForSeconds(0.1f);
                
                Destroy(gem.gameObject, 0.1f);
            }
        }
        
        private IEnumerator MakeGemsFall() {
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++)
                {
                    if (_grid.GetValue(x, y) != null) continue;
                    
                    for (var i = y + 1; i < height; i++)
                    {
                        if (_grid.GetValue(x, i) == null) continue;
                            
                        var gem = _grid.GetValue(x, i).GetValue();
                        _grid.SetValue(x, y, _grid.GetValue(x, i));
                        _grid.SetValue(x, i, null);
                        
                        gem.transform
                            .DOLocalMove(_grid.GetWorldPositionCenter(x, y), 0.5f)
                            .SetEase(ease);
                        _audioManager.PlayWoosh();
                        
                        yield return new WaitForSeconds(0.2f);
                        break;
                    }
                }
            }
        }
        
        private IEnumerator FillEmptySpots() {
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++)
                {
                    if (_grid.GetValue(x, y) != null) continue;
                    
                    CreateGem(x, y);
                    _audioManager.PlayPop();
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        private IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB)
        {
            _userInitiated = true;
            var gridObjectA = _grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = _grid.GetValue(gridPosB.x, gridPosB.y);

            gridObjectA.GetValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(_grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);

            _grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            _grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

            yield return new WaitForSeconds(0.5f);
        }
        
        private void ExplodeVFX(Vector2Int match) {
            // TODO: Pool
            var fx = Instantiate(explosion, transform);
            fx.transform.position = _grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 5f);
        }

        private void DeselectGem() => _selectedGem = new Vector2Int(-1, -1);
        private void SelectGem(Vector2Int gridPos) => _selectedGem = gridPos;
        private bool IsEmptyPosition(Vector2Int gridPosition) => _grid.GetValue(gridPosition.x, gridPosition.y) == null;

        private bool IsValidPosition(Vector2 gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}