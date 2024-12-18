using System.Collections;
using System.Collections.Generic;
using _Main._Enums;
using UnityEngine;

public enum TankState
{
    Waiting,  // Durak noktas?nda bekliyor
    Filling,  // Stickman doluyor
    Moving    // ?leri hareket ediyor
}

namespace _Main._Tank
{
    public class Tank : MonoBehaviour
    {
        public TankState currentState = TankState.Waiting; // Ba?lang?ç durumu
        public int stickmanCount = 0; // Tankta bulunan stickman say?s?
        private Vector3 targetPosition; // Tank?n hareket edece?i hedef pozisyon

        [SerializeField]
        private ColorType _colorType;
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
            currentState = TankState.Waiting; // Ba?lang?ç durumu Waiting
        }

        // Tüm child objelerin renklerini de?i?tirme i?lemi
        private void UpdateColor()
        {
            // ColorType'a göre yeni renk belirle
            Color newColor = ColorManager.ColorTypeToColor(_colorType);

            // Bu objenin ve tüm child objelerin Renderer bile?enlerine eri?
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = newColor;
            }
        }


        public void AddStickman()
        {
            if (currentState != TankState.Waiting) return;

            stickmanCount++;
            Debug.Log($"Stickman eklendi: {stickmanCount}");
            if (stickmanCount >= 3) // 3 stickman tamamlan?nca
            {
                currentState = TankState.Filling;
                Debug.Log("Tank doldu, hareket ediliyor...");
                StartMoving();
            }
        }

        public void StartMoving()
        {
            if (currentState != TankState.Filling) return;

            currentState = TankState.Moving;
            StartCoroutine(MoveToTarget());
        }

        private IEnumerator MoveToTarget()
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                yield return null;
            }

            Debug.Log("Hedefe ula??ld?!");
            Destroy(gameObject); // Tank hedefe ula??nca yok ediliyor (örnek için)
        }
    }
}

