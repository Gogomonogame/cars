using UnityEngine;

public class CarWheelTrailRendererHandler : MonoBehaviour
{
    CarController carController;
    TrailRenderer trailRenderer;

    private void Awake()
    {
        carController = GetComponentInParent<CarController>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;

    }

    private void Update()
    {
        if(carController.IsTireScreeching(out float lateralVelocity, out bool isBreaking))
        {
            trailRenderer.emitting = true;
        }
        else trailRenderer.emitting = false;
    }
}
