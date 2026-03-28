using UnityEngine;
using UnityEngine.UI;
public class CarUIHandler : MonoBehaviour
{
    [Header("Car details")]
    public Image carImage;

    Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        
    }

    public void SetupCar(CarData carData)
    {
        carImage.sprite = carData.CarUISprite;
    }

    public void StartCarEntranceAnimation(bool isAppearingOnRightSide)
    {
        if (isAppearingOnRightSide)
            animator.Play("CarUIAppearFromRight");
        else
            animator.Play("CarUIAppearFromLeft");
    }

    public void StartCarExitAnimation(bool isAppearingOnRightSide)
    {
        if (isAppearingOnRightSide)
            animator.Play("CarUIDisappearToRight");
        else
            animator.Play("CarUIDisappearToLeft");
    }

    //Events
    public void OnCarAnimationCompleted()
    {
        Destroy(gameObject);
    }
}
