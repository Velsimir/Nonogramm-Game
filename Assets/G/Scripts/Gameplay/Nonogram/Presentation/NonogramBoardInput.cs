using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using G.Core.Common;
using G.Gameplay.Nonogram.Data;

namespace G.Gameplay.Nonogram.Presentation
{
    public class NonogramBoardInput : View, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _gridRect;

        private readonly Subject<CellAddress> _strokeStarted = new();
        private readonly Subject<CellAddress> _strokeMoved = new();
        private readonly Subject<Unit> _strokeFinished = new();

        private float[] _columnOffsets = System.Array.Empty<float>();
        private float[] _rowOffsets = System.Array.Empty<float>();
        private float _cellSize;

        private CellAddress _startAddress;
        private CellAddress _lastAddress;
        private LineAxis _strokeAxis;
        private bool _isStrokeActive;
        private bool _isAxisLocked;

        public Observable<CellAddress> StrokeStarted => _strokeStarted;
        public Observable<CellAddress> StrokeMoved => _strokeMoved;
        public Observable<Unit> StrokeFinished => _strokeFinished;

        public void SetLayout(float[] columnOffsets, float[] rowOffsets, float cellSize)
        {
            _columnOffsets = columnOffsets;
            _rowOffsets = rowOffsets;
            _cellSize = cellSize;

            CancelStroke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (TryResolve(eventData, out CellAddress address) == false)
                return;

            _isStrokeActive = true;
            _isAxisLocked = false;
            _startAddress = address;
            _lastAddress = address;

            _strokeStarted.OnNext(address);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isStrokeActive == false)
                return;

            if (TryResolve(eventData, out CellAddress address) == false)
                return;

            if (TryLockAxis(ref address) == false)
                return;

            if (address.Equals(_lastAddress))
                return;

            _lastAddress = address;
            _strokeMoved.OnNext(address);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isStrokeActive == false)
                return;

            _isStrokeActive = false;
            _strokeFinished.OnNext(Unit.Default);
        }

        protected override void OnDestroyed()
        {
            _strokeStarted.Dispose();
            _strokeMoved.Dispose();
            _strokeFinished.Dispose();
        }

        private void CancelStroke()
        {
            if (_isStrokeActive == false)
                return;

            _isStrokeActive = false;
            _strokeFinished.OnNext(Unit.Default);
        }

        private bool TryLockAxis(ref CellAddress address)
        {
            if (_isAxisLocked == false)
            {
                int deltaColumn = Mathf.Abs(address.Column - _startAddress.Column);
                int deltaRow = Mathf.Abs(address.Row - _startAddress.Row);

                if (deltaColumn == 0 && deltaRow == 0)
                    return false;

                _strokeAxis = deltaColumn >= deltaRow ? LineAxis.Row : LineAxis.Column;
                _isAxisLocked = true;
            }

            address = _strokeAxis == LineAxis.Row
                ? new CellAddress(address.Column, _startAddress.Row)
                : new CellAddress(_startAddress.Column, address.Row);

            return true;
        }

        private bool TryResolve(PointerEventData eventData, out CellAddress address)
        {
            address = default;

            if (_cellSize <= 0f)
                return false;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_gridRect, eventData.position,
                    eventData.pressEventCamera, out Vector2 local) == false)
                return false;

            int column = ResolveIndex(_columnOffsets, local.x);
            int row = ResolveIndex(_rowOffsets, -local.y);

            if (column < 0 || row < 0)
                return false;

            address = new CellAddress(column, row);
            return true;
        }

        private int ResolveIndex(float[] offsets, float position)
        {
            int best = -1;
            float bestDistance = _cellSize;

            for (int i = 0; i < offsets.Length; i++)
            {
                float distance = Mathf.Abs(position - (offsets[i] + _cellSize * 0.5f));

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = i;
            }

            return best;
        }
    }
}
