using System.Collections.Generic;
using DG.Tweening;
using Game_Management;
using UnityEngine;

namespace Grid_Mechanic
{
    public class GridVisualController : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Renderer gridRenderer;
        [SerializeField] private List<Material> materialOptions;

        [Header("Emission Settings")]
        [SerializeField] private float emissionIntensity = 2f;
        [SerializeField] private float emissionDuration = 0.5f;

        private Tween colorTween;

        // Tracks the current material index assigned to this grid
        public int AssignedMaterialIndex { get; private set; } = -1;
        
        public void AssignRandomMaterial()
        {
            if (materialOptions == null || materialOptions.Count == 0)
            {
                Debug.LogWarning("Material options list is empty!");
                return;
            }

            AssignedMaterialIndex = Random.Range(0, materialOptions.Count);
            SetMaterialByIndex(AssignedMaterialIndex);
        }
        
        public void SetMaterialByIndex(int index)
        {
            if (materialOptions == null || index < 0)
                return;

            AssignedMaterialIndex = index % materialOptions.Count;
            gridRenderer.material = materialOptions[AssignedMaterialIndex];
        }
        
        public void AnimateEmission(bool glow)
        {
            if (gridRenderer == null || !gridRenderer.material.HasProperty("_EmissionColor"))
                return;

            Color targetColor = glow ? Color.white * emissionIntensity : Color.black;

            // Kill any existing emission tween before starting a new one
            colorTween?.Kill();

            colorTween = DOTween.To(
                () => gridRenderer.material.GetColor("_EmissionColor"),
                c => gridRenderer.material.SetColor("_EmissionColor", c),
                targetColor,
                emissionDuration
            ).SetLoops(glow ? 2 : 1, LoopType.Yoyo); // Ping-pong the glow effect if glowing
        }

        private void OnEnable()
        {
            Actions.ResetAllGrids += ResetGrid;
        }

        private void OnDisable()
        {
            Actions.ResetAllGrids -= ResetGrid;
        }
        
        private void ResetGrid()
        {
            Pool.Instance.DeactivateObject(gameObject, PoolItemType.Grid);
        }
    }
}
