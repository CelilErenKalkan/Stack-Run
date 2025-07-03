using System.Collections.Generic;
using DG.Tweening;
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

        public int AssignedMaterialIndex { get; private set; } = -1;

        public void AssignRandomMaterial()
        {
            if (materialOptions == null || materialOptions.Count == 0)
            {
                Debug.LogWarning("Material options list is empty!");
                return;
            }

            AssignedMaterialIndex = Random.Range(0, materialOptions.Count);
            gridRenderer.material = materialOptions[AssignedMaterialIndex];
        }

        public void SetMaterialByIndex(int index)
        {
            if (materialOptions == null || index < 0 || index >= materialOptions.Count)
                return;

            AssignedMaterialIndex = index;
            Material matInstance = Instantiate(materialOptions[AssignedMaterialIndex]);
            gridRenderer.material = matInstance;
        }

        public void AnimateEmission(bool glow)
        {
            if (gridRenderer == null || !gridRenderer.material.HasProperty("_EmissionColor"))
                return;

            Color targetColor = glow ? Color.white * emissionIntensity : Color.black;

            colorTween?.Kill();
            colorTween = DOTween.To(
                () => gridRenderer.material.GetColor("_EmissionColor"),
                c => gridRenderer.material.SetColor("_EmissionColor", c),
                targetColor,
                emissionDuration
            ).SetLoops(glow ? 2 : 1, LoopType.Yoyo);
        }
    }
}