namespace Match3._Project.Scripts.GridSystem
{
    public class GridObject<T>
    {
        private GridSystem2D<GridObject<T>> _grid;
        private int _x;
        private int _y;
        private T _gem;

        public GridObject(GridSystem2D<GridObject<T>> grid, int x, int y)
        {
            _grid = grid;
            _x = x;
            _y = y;
        }

        public void SetValue(T gem)
        {
            _gem = gem;
        }

        public T GetValue() => _gem;
    }
}