using System.Collections;
using UnityEngine;
using _Main._Enums;
using DG.Tweening;  // DOTween'i dahil ettik

namespace _Main._Tank
{
    public enum TankState
    {
        Waiting,  // Tank is waiting at the stop point.
        Filling,  // Tank is being filled with stickmen.
        Moving    // Tank is moving towards its target.
    }

    public class Tank : MonoBehaviour
    {
        [Header("Tank Configuration")]
        public TankState currentState = TankState.Waiting;

        public int stickmanCount = 0;
        [SerializeField] private int maxStickmanCount = 3;

        private Vector3 targetPosition;

        [Header("Tank Color Configuration")]
        [SerializeField]
        private ColorType _colorType;

        public bool isCurrentTank = false;

        public ColorType UnitColorType
        {
            get => _colorType;
            set
            {
                _colorType = value;
                UpdateColor();
            }
        }

        public void Initialize(Vector3 target)
        {
            UpdateColor();
            targetPosition = target;
            if (isCurrentTank)
            {
                currentState = TankState.Filling;  // Start filling if it's the current tank
            }
            else
            {
                currentState = TankState.Waiting;  // Wait if it's not the current tank
            }
        }

        private void UpdateColor()
        {
            Color newColor = ColorManager.ColorTypeToColor(_colorType);
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }

        public void AddStickman(ColorType stickmanColor)
        {
            if (currentState != TankState.Waiting)
            {
                Debug.Log("Tank is not in waiting state. Cannot add stickman.");
                return;
            }

            if (stickmanColor != _colorType)
            {
                Debug.Log("Stickman color does not match the tank's color.");
                return;
            }

            stickmanCount++;
            Debug.Log($"Stickman added: {stickmanCount}");

            if (IsFull())
            {
                currentState = TankState.Filling;  // Switch to filling if not already in that state
                Debug.Log("Tank is full, moving...");
                StartMoving();
            }
        }

        public void StartMoving()
        {
            if (currentState != TankState.Filling)
            {
                Debug.Log("Tank is not in filling state. Cannot start moving.");
                return;
            }

            currentState = TankState.Moving;  // Switch to moving state
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            float distanceFactor = 15f;
            targetPosition = new Vector3(transform.position.x - distanceFactor, transform.position.y, transform.position.z);

            Debug.Log($"Moving from {transform.position} to {targetPosition}");

            transform.DOMove(targetPosition, 5f)
                .SetEase(Ease.Linear)
                .OnKill(() =>
                {
                    Debug.Log("Arrived at the target!");
                    Destroy(gameObject);
                });
        }

        public bool IsFull()
        {
            return stickmanCount >= maxStickmanCount;
        }
    }
}
