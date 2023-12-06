using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Pancake.MobileInput
{
    [RequireComponent(typeof(TouchCamera))]
    public class MobilePickingController : MonoBehaviour
    {
        public enum SelectionAction
        {
            Select,
            Deselect,
        }

        #region inspector

        [SerializeField] [Tooltip("When set to true, the position of dragged items snaps to discrete units.")]
        private bool snapToGrid = true;

        [SerializeField] [Tooltip("Size of the snap units when snapToGrid is enabled.")]
        private float snapUnitSize = 1;

        [SerializeField]
        [Tooltip(
            "When snapping is enabled, this value defines a position offset that is added to the center of the object when dragging. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 snapOffset = Vector2.zero;

        [SerializeField]
        [Tooltip(
            "When set to Straight, picked items will be snapped to a perfectly horizontal and vertical grid in world space. Diagonal snaps the items on a 45 degree grid.")]
        private SnapAngle snapAngle = SnapAngle.Straight0Degrees;

        [Header("Advanced")] [SerializeField] [Tooltip("When this flag is enabled, more than one item can be selected and moved at the same time.")]
        private bool isMultiSelectionEnabled = false;

        [SerializeField] [Tooltip("When setting this variable to true, pickables can only be moved by long tapping on them first.")]
        private bool requireLongTapForMove = false;


        [Header("Event Callbacks")]
#pragma warning disable 649
        [SerializeField]
        [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is selected.")]
        private TransformUnityEvent onPickableTransformSelected;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is selected through a long tap.")]
        private PickableSelectedUnityEvent onPickableTransformSelectedExtended;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is deselected.")]
        private TransformUnityEvent onPickableTransformDeselected;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when the moving of a pickable transform is started.")]
        private TransformUnityEvent onPickableTransformMoveStarted;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is moved to a new position.")]
        private TransformUnityEvent onPickableTransformMoved;
        
        [SerializeField]
        [Tooltip(
            "Here you can set up callbacks to be invoked when the moving of a pickable transform is ended. The event requires 2 parameters. The first is the start position of the drag. The second is the dragged transform. The start position can be used to reset the transform in case the drag has ended on an invalid position.")]
        private Vector3TransformUnityEvent onPickableTransformMoveEnded;
#pragma warning restore 649

        #endregion

        #region expert mode tweakables

        [Header("Expert Mode")] [SerializeField] private bool expertModeEnabled;

        [SerializeField]
        [Tooltip(
            "When setting this to false, pickables will not become deselected when the user clicks somewhere on the screen, except when he clicks on another pickable.")]
        private bool deselectPreviousColliderOnClick = true;

        [SerializeField]
        [Tooltip("When setting this to false, the OnPickableTransformSelect event will only be sent once when clicking on the same pickable repeatedly.")]
        private bool repeatEventSelectedOnClick = true;

        [SerializeField] [Tooltip("Previous versions of this asset may have fired the OnPickableTransformMoveStarted too early, when it hasn't actually been moved.")]
        private bool useLegacyTransformMovedEventOrder = false;

        #endregion

        private TouchInput _touchInput;

        private TouchCamera _touchCam;

        private Component SelectedCollider
        {
            get
            {
                if (SelectedColliders.Count == 0)
                {
                    return null;
                }

                return SelectedColliders[SelectedColliders.Count - 1];
            }
        }

        public List<Component> SelectedColliders { get; private set; }

        private bool _isSelectedViaLongTap = false;

        public TouchPickable CurrentlyDraggedPickable { get; private set; }

        private Transform CurrentlyDraggedTransform
        {
            get
            {
                if (CurrentlyDraggedPickable != null)
                {
                    return CurrentlyDraggedPickable.PickableTransform;
                }
                else
                {
                    return null;
                }
            }
        }

        private Vector3 _draggedTransformOffset = Vector3.zero;

        private Vector3 _draggedTransformHeightOffset = Vector3.zero;

        private Vector3 _draggedItemCustomOffset = Vector3.zero;

        public bool SnapToGrid { get { return snapToGrid; } set { snapToGrid = value; } }

        public SnapAngle SnapAngle { get { return snapAngle; } set { snapAngle = value; } }

        public float SnapUnitSize { get { return snapUnitSize; } set { snapUnitSize = value; } }

        public Vector2 SnapOffset { get { return snapOffset; } set { snapOffset = value; } }

        public const float SNAP_ANGLE_DIAGONAL = 45 * Mathf.Deg2Rad;

        private Vector3 _currentlyDraggedTransformPosition = Vector3.zero;

        private const float TRANSFORM_MOVED_DISTANCE_THRESHOLD = 0.001f;

        private Vector3 _currentDragStartPos = Vector3.zero;

        private bool _invokeMoveStartedOnDrag = false;
        private bool _invokeMoveEndedOnDrag = false;

        private Vector3 _itemInitialDragOffsetWorld;
        private bool _isManualSelectionRequest;

        public bool IsMultiSelectionEnabled
        {
            get { return isMultiSelectionEnabled; }
            set
            {
                isMultiSelectionEnabled = value;
                if (value == false)
                {
                    DeselectAll();
                }
            }
        }

        private Dictionary<Component, Vector3> _selectionPositionOffsets = new Dictionary<Component, Vector3>();

        public void Awake()
        {
            SelectedColliders = new List<Component>();
            _touchCam = FindObjectOfType<TouchCamera>();
            if (_touchCam == null)
            {
                Debug.LogError("No MobileTouchCamera found in scene. This script will not work without this.");
            }

            _touchInput = _touchCam.GetComponent<TouchInput>();
            if (_touchInput == null)
            {
                Debug.LogError("No TouchInputController found in scene. Make sure this component exists and is attached to the MobileTouchCamera gameObject.");
            }
        }

        public void Start()
        {
            _touchInput.onClick.OnRaised += OnClick;
            _touchInput.onFingerDown.OnRaised += InputOnFingerDown;
            _touchInput.onFingerUp.OnRaised += InputOnFingerUp;
            _touchInput.onStartDrag.OnRaised += InputOnDragStart;
            _touchInput.onUpdateDrag.OnRaised += InputOnDragUpdate;
            _touchInput.onStopDrag.OnRaised += InputOnDragStop;
        }

        public void OnDestroy()
        {
            _touchInput.onClick.OnRaised -= OnClick;
            _touchInput.onFingerDown.OnRaised -= InputOnFingerDown;
            _touchInput.onFingerUp.OnRaised -= InputOnFingerUp;
            _touchInput.onStartDrag.OnRaised -= InputOnDragStart;
            _touchInput.onUpdateDrag.OnRaised -= InputOnDragUpdate;
            _touchInput.onStopDrag.OnRaised -= InputOnDragStop;
        }

        public void LateUpdate()
        {
            if (_isManualSelectionRequest && TouchWrapper.TouchCount == 0)
            {
                _isManualSelectionRequest = false;
            }
        }

        #region public interface

        /// <summary>
        /// Method that allows to set the currently selected collider for the picking controller by code.
        /// Useful for example for auto-selecting newly spawned items or for selecting items via a menu button.
        /// Use this method when you want to select just one item.
        /// </summary>
        public void SelectCollider(Component collider)
        {
            if (IsMultiSelectionEnabled)
            {
                Select(collider, false, false);
            }
            else
            {
                SelectColliderInternal(collider, false, false);
                _isManualSelectionRequest = true;
            }
        }

        /// <summary>
        /// Method to deselect the last selected collider.
        /// </summary>
        public void DeselectSelectedCollider() { Deselect(SelectedCollider); }

        /// <summary>
        /// In case multi-selection is enabled, this method allows to deselect
        /// all colliders at once.
        /// </summary>
        public void DeselectAllSelectedColliders()
        {
            var collidersToRemove = new List<Component>(SelectedColliders);
            foreach (var colliderToRemove in collidersToRemove)
            {
                Deselect(colliderToRemove);
            }
        }

        /// <summary>
        /// Method to deselect the given collider.
        /// In case the collider hasn't been selected before, the method returns false.
        /// </summary>
        private bool Deselect(Component colliderComponent)
        {
            bool wasRemoved = SelectedColliders.Remove(colliderComponent);
            if (wasRemoved)
            {
                OnSelectedColliderChanged(SelectionAction.Deselect, colliderComponent.GetComponent<TouchPickable>());
            }

            return wasRemoved;
        }

        /// <summary>
        /// Method to deselect all currently selected colliders.
        /// </summary>
        /// <returns></returns>
        public int DeselectAll()
        {
            SelectedColliders.RemoveAll(item => item == null);
            int colliderCount = SelectedColliders.Count;
            foreach (Component colliderComponent in SelectedColliders)
            {
                OnSelectedColliderChanged(SelectionAction.Deselect, colliderComponent.GetComponent<TouchPickable>());
            }

            SelectedColliders.Clear();
            return colliderCount;
        }

        public Component GetClosestColliderAtScreenPoint(Vector3 screenPoint, out Vector3 intersectionPoint)
        {
            Component hitCollider = null;
            float hitDistance = float.MaxValue;
            Ray camRay = _touchCam.Cam.ScreenPointToRay(screenPoint);
            RaycastHit hitInfo;
            intersectionPoint = Vector3.zero;
            if (Physics.Raycast(camRay, out hitInfo))
            {
                hitDistance = hitInfo.distance;
                hitCollider = hitInfo.collider;
                intersectionPoint = hitInfo.point;
            }

            RaycastHit2D hitInfo2D = Physics2D.Raycast(camRay.origin, camRay.direction);
            if (hitInfo2D == true)
            {
                if (hitInfo2D.distance < hitDistance)
                {
                    hitCollider = hitInfo2D.collider;
                    intersectionPoint = hitInfo2D.point;
                }
            }

            return hitCollider;
        }

        public void RequestDragPickable(Component colliderComponent)
        {
            if (TouchWrapper.TouchCount == 1)
            {
                SelectColliderInternal(colliderComponent, false, false);
                _isManualSelectionRequest = true;
                Vector3 fingerDownPos = TouchWrapper.Touch0.Position;
                Vector3 intersectionPoint;
                Ray dragRay = _touchCam.Cam.ScreenPointToRay(fingerDownPos);
                bool hitSuccess = _touchCam.RaycastGround(dragRay, out intersectionPoint);
                if (hitSuccess == false)
                {
                    intersectionPoint = colliderComponent.transform.position;
                }

                if (requireLongTapForMove)
                {
                    _isSelectedViaLongTap = true; //This line ensures that dragging via scrip also works when 'requireLongTapForDrag' is set to true.
                }

                RequestDragPickable(colliderComponent, fingerDownPos, intersectionPoint);
                _invokeMoveEndedOnDrag = true;
            }
            else
            {
                Debug.LogError("A drag request can only be invoked when the user has placed exactly 1 finger on the screen.");
            }
        }

        public Vector3 GetFinger0PosWorld() { return _touchCam.GetFinger0PosWorld(); }

        #endregion

        private void SelectColliderInternal(Component colliderComponent, bool isDoubleClick, bool isLongTap)
        {
            if (deselectPreviousColliderOnClick == false)
            {
                if (colliderComponent == null || colliderComponent.GetComponent<TouchPickable>() == null)
                {
                    return; //Skip selection change in case the user requested to deselect only in case another pickable is clicked.
                }
            }

            if (_isManualSelectionRequest)
            {
                return; //Skip selection when the user has already requested a manual selection with the same click.
            }

            Component previouslySelectedCollider = SelectedCollider;
            bool skipSelect = false;

            if (isMultiSelectionEnabled == false)
            {
                if (previouslySelectedCollider != null && previouslySelectedCollider != colliderComponent)
                {
                    Deselect(previouslySelectedCollider);
                }
            }
            else
            {
                skipSelect = Deselect(colliderComponent);
            }

            if (skipSelect == false)
            {
                if (colliderComponent != null)
                {
                    if (colliderComponent != previouslySelectedCollider || repeatEventSelectedOnClick)
                    {
                        Select(colliderComponent, isDoubleClick, isLongTap);
                        _isSelectedViaLongTap = isLongTap;
                    }
                }
            }
        }

        private void OnClick(Vector3 clickPosition, bool isDoubleClick, bool isLongTap)
        {
            Vector3 intersectionPoint;
            var newCollider = GetClosestColliderAtScreenPoint(clickPosition, out intersectionPoint);
            SelectColliderInternal(newCollider, isDoubleClick, isLongTap);
        }

        private void RequestDragPickable(Vector3 fingerDownPos)
        {
            Vector3 intersectionPoint = Vector3.zero;
            Component pickedCollider = GetClosestColliderAtScreenPoint(fingerDownPos, out intersectionPoint);
            if (pickedCollider != null && SelectedColliders.Contains(pickedCollider))
            {
                RequestDragPickable(pickedCollider, fingerDownPos, intersectionPoint);
            }
        }

        private void RequestDragPickable(Component colliderComponent, Vector2 fingerDownPos, Vector3 intersectionPoint)
        {
            if (requireLongTapForMove && _isSelectedViaLongTap == false)
            {
                return;
            }

            CurrentlyDraggedPickable = null;
            bool isDragStartedOnSelection = colliderComponent != null && SelectedColliders.Contains(colliderComponent);
            if (isDragStartedOnSelection)
            {
                TouchPickable touchPickable = colliderComponent.GetComponent<TouchPickable>();
                if (touchPickable != null)
                {
                    _touchCam.OnDragSceneObject(); //Lock camera movement.
                    CurrentlyDraggedPickable = touchPickable;
                    _currentlyDraggedTransformPosition = CurrentlyDraggedTransform.position;

                    _invokeMoveStartedOnDrag = true;
                    _currentDragStartPos = CurrentlyDraggedTransform.position;
                    _selectionPositionOffsets.Clear();
                    foreach (Component selectionComponent in SelectedColliders)
                    {
                        _selectionPositionOffsets.Add(selectionComponent, _currentDragStartPos - selectionComponent.transform.position);
                    }

                    _draggedTransformOffset = Vector3.zero;
                    _draggedTransformHeightOffset = Vector3.zero;
                    _draggedItemCustomOffset = Vector3.zero;

                    //Find offset of item transform relative to ground.
                    Vector3 groundPosCenter = Vector3.zero;
                    Ray groundScanRayCenter = new Ray(CurrentlyDraggedTransform.position, -_touchCam.RefPlane.normal);
                    bool rayHitSuccess = _touchCam.RaycastGround(groundScanRayCenter, out groundPosCenter);
                    if (rayHitSuccess)
                    {
                        _draggedTransformHeightOffset = CurrentlyDraggedTransform.position - groundPosCenter;
                    }
                    else
                    {
                        groundPosCenter = CurrentlyDraggedTransform.position;
                    }

                    _draggedTransformOffset = groundPosCenter - intersectionPoint;
                    _itemInitialDragOffsetWorld = ComputeDragPosition(fingerDownPos, SnapToGrid) - CurrentlyDraggedTransform.position;
                }
            }
        }

        private void InputOnFingerDown(Vector3 fingerDownPos)
        {
            if (requireLongTapForMove == false || _isSelectedViaLongTap)
            {
                RequestDragPickable(fingerDownPos);
            }
        }

        private void InputOnFingerUp() { EndPickableTransformMove(); }

        private Vector3 ComputeDragPosition(Vector3 dragPosCurrent, bool clampToGrid)
        {
            Vector3 dragPosWorld = Vector3.zero;
            Ray dragRay = _touchCam.Cam.ScreenPointToRay(dragPosCurrent);

            dragRay.origin += _draggedTransformOffset;
            bool hitSuccess = _touchCam.RaycastGround(dragRay, out dragPosWorld);
            if (hitSuccess == false)
            {
                //This case really should never be met. But in case it is for some unknown reason, return the current item position. That way at least it will remain static and not move somewhere into nirvana.
                return CurrentlyDraggedTransform.position;
            }

            dragPosWorld += _draggedTransformHeightOffset;
            dragPosWorld += _draggedItemCustomOffset;

            if (clampToGrid)
            {
                dragPosWorld = ClampDragPosition(CurrentlyDraggedPickable, dragPosWorld);
            }

            return dragPosWorld;
        }

        private void InputOnDragStart(Vector3 clickPosition, bool isLongTap)
        {
            if (isLongTap && _touchInput.LongTapStartsDrag)
            {
                Vector3 intersectionPoint;
                Component newCollider = GetClosestColliderAtScreenPoint(clickPosition, out intersectionPoint);
                if (newCollider != null)
                {
                    TouchPickable newPickable = newCollider.GetComponent<TouchPickable>();
                    if (newPickable != null)
                    {
                        if (SelectedColliders.Contains(newCollider) == false)
                        {
                            SelectColliderInternal(newCollider, false, isLongTap);
                        }
                        else
                        {
                            _isSelectedViaLongTap = isLongTap;
                        }

                        RequestDragPickable(clickPosition);
                    }
                }
            }
        }

        private void InputOnDragUpdate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset)
        {
            if (CurrentlyDraggedTransform != null)
            {
                if (_invokeMoveStartedOnDrag && useLegacyTransformMovedEventOrder)
                {
                    InvokePickableMoveStart();
                }

                _draggedItemCustomOffset +=
                    CurrentlyDraggedTransform.position -
                    _currentlyDraggedTransformPosition; //Accomodate for custom movements by user code that happen while an item is being dragged. E.g. this allows users to lift items slightly during a drag.

                Vector3 dragPosWorld = ComputeDragPosition(dragPosCurrent, SnapToGrid);
                CurrentlyDraggedTransform.position = dragPosWorld - _itemInitialDragOffsetWorld;

                if (SelectedColliders.Count > 1)
                {
                    foreach (KeyValuePair<Component, Vector3> colliderOffsetPair in _selectionPositionOffsets)
                    {
                        if (colliderOffsetPair.Key != null && colliderOffsetPair.Key.transform != CurrentlyDraggedTransform)
                        {
                            colliderOffsetPair.Key.transform.position = CurrentlyDraggedTransform.position - colliderOffsetPair.Value;
                        }
                    }
                }

                bool hasMoved = false;
                if (_touchCam.CameraAxes == CameraPlaneAxes.XY2DSideScroll)
                {
                    hasMoved = ComputeDistance2d(CurrentlyDraggedTransform.position.x,
                        CurrentlyDraggedTransform.position.y,
                        _currentlyDraggedTransformPosition.x,
                        _currentlyDraggedTransformPosition.y) > TRANSFORM_MOVED_DISTANCE_THRESHOLD;
                }
                else
                {
                    hasMoved = ComputeDistance2d(CurrentlyDraggedTransform.position.x,
                        CurrentlyDraggedTransform.position.z,
                        _currentlyDraggedTransformPosition.x,
                        _currentlyDraggedTransformPosition.z) > TRANSFORM_MOVED_DISTANCE_THRESHOLD;
                }

                if (hasMoved)
                {
                    if (_invokeMoveStartedOnDrag && useLegacyTransformMovedEventOrder == false)
                    {
                        InvokePickableMoveStart();
                    }

                    InvokeTransformActionSafe(onPickableTransformMoved, CurrentlyDraggedTransform);
                }

                _currentlyDraggedTransformPosition = CurrentlyDraggedTransform.position;
            }
        }

        private void InvokePickableMoveStart()
        {
            InvokeTransformActionSafe(onPickableTransformMoveStarted, CurrentlyDraggedTransform);
            _invokeMoveStartedOnDrag = false;
            _invokeMoveEndedOnDrag = true;
        }

        private float ComputeDistance2d(float x0, float y0, float x1, float y1) { return Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)); }

        private void InputOnDragStop(Vector3 dragStopPos, Vector3 dragFinalMomentum) { EndPickableTransformMove(); }

        private void EndPickableTransformMove()
        {
            if (CurrentlyDraggedTransform != null)
            {
                if (onPickableTransformMoveEnded != null)
                {
                    if (_invokeMoveEndedOnDrag)
                    {
                        onPickableTransformMoveEnded.Invoke(_currentDragStartPos, CurrentlyDraggedTransform);
                    }
                }
            }

            CurrentlyDraggedPickable = null;
            _invokeMoveStartedOnDrag = false;
            _invokeMoveEndedOnDrag = false;
        }

        private Vector3 ClampDragPosition(TouchPickable draggedPickable, Vector3 position)
        {
            if (_touchCam.CameraAxes == CameraPlaneAxes.XY2DSideScroll)
            {
                if (snapAngle == SnapAngle.Diagonal45Degrees)
                {
                    RotateVector2(ref position.x, ref position.y, -SNAP_ANGLE_DIAGONAL);
                }

                position.x = GetPositionSnapped(position.x, draggedPickable.LocalSnapOffset.x + snapOffset.x);
                position.y = GetPositionSnapped(position.y, draggedPickable.LocalSnapOffset.y + snapOffset.y);
                if (snapAngle == SnapAngle.Diagonal45Degrees)
                {
                    RotateVector2(ref position.x, ref position.y, SNAP_ANGLE_DIAGONAL);
                }
            }
            else
            {
                if (snapAngle == SnapAngle.Diagonal45Degrees)
                {
                    RotateVector2(ref position.x, ref position.z, -SNAP_ANGLE_DIAGONAL);
                }

                position.x = GetPositionSnapped(position.x, draggedPickable.LocalSnapOffset.x + snapOffset.x);
                position.z = GetPositionSnapped(position.z, draggedPickable.LocalSnapOffset.y + snapOffset.y);
                if (snapAngle == SnapAngle.Diagonal45Degrees)
                {
                    RotateVector2(ref position.x, ref position.z, SNAP_ANGLE_DIAGONAL);
                }
            }

            return position;
        }

        private void RotateVector2(ref float x, ref float y, float degrees)
        {
            if (Mathf.Approximately(degrees, 0))
            {
                return;
            }

            float newX = x * Mathf.Cos(degrees) - y * Mathf.Sin(degrees);
            float newY = x * Mathf.Sin(degrees) + y * Mathf.Cos(degrees);
            x = newX;
            y = newY;
        }

        private float GetPositionSnapped(float position, float snapOffset)
        {
            if (snapToGrid)
            {
                return Mathf.RoundToInt(position / snapUnitSize) * snapUnitSize + snapOffset;
            }
            else
            {
                return position;
            }
        }

        private void OnSelectedColliderChanged(SelectionAction selectionAction, TouchPickable touchPickable)
        {
            if (touchPickable != null)
            {
                if (selectionAction == SelectionAction.Select)
                {
                    InvokeTransformActionSafe(onPickableTransformSelected, touchPickable.PickableTransform);
                }
                else if (selectionAction == SelectionAction.Deselect)
                {
                    InvokeTransformActionSafe(onPickableTransformDeselected, touchPickable.PickableTransform);
                }
            }
        }

        private void OnSelectedColliderChangedExtended(SelectionAction selectionAction, TouchPickable touchPickable, bool isDoubleClick, bool isLongTap)
        {
            if (touchPickable != null)
            {
                if (selectionAction == SelectionAction.Select)
                {
                    PickableSelected pickableSelected = new PickableSelected()
                    {
                        Selected = touchPickable.PickableTransform, IsDoubleClick = isDoubleClick, IsLongTap = isLongTap
                    };
                    InvokeGenericActionSafe(onPickableTransformSelectedExtended, pickableSelected);
                }
            }
        }

        private void InvokeTransformActionSafe(TransformUnityEvent eventAction, Transform selectionTransform)
        {
            if (eventAction != null)
            {
                eventAction.Invoke(selectionTransform);
            }
        }

        private void InvokeGenericActionSafe<T1, T2>(T1 eventAction, T2 eventArgs) where T1 : UnityEvent<T2>
        {
            if (eventAction != null)
            {
                eventAction.Invoke(eventArgs);
            }
        }

        private void Select(Component colliderComponent, bool isDoubleClick, bool isLongTap)
        {
            TouchPickable touchPickable = colliderComponent.GetComponent<TouchPickable>();
            if (touchPickable != null)
            {
                if (SelectedColliders.Contains(colliderComponent) == false)
                {
                    SelectedColliders.Add(colliderComponent);
                }
            }

            OnSelectedColliderChanged(SelectionAction.Select, touchPickable);
            OnSelectedColliderChangedExtended(SelectionAction.Select, touchPickable, isDoubleClick, isLongTap);
        }
    }
}