using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : NetworkBehaviour
{
    //public:
    [Header("Car settings")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20;
    public float velocityToScreeching = 4.0f;

    [Header("Sprites")]
    public SpriteRenderer carSpriteRenderer;
    public SpriteRenderer carShadowRenderer;

    [Header("Jumping")]
    public AnimationCurve jumpCurve;
    public ParticleSystem landingParticleSystem;
    public bool canJump = true;

    //private:
    float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;

    float velocityVsUp = 0;

    bool isJumping = false;

    //Components
    Rigidbody2D carRb;
    Collider2D carCollider;
    CarSoundEffectsHandler carSFXHandler;



    void Awake()
    {
        carRb = GetComponent<Rigidbody2D>();
        carCollider = GetComponentInChildren<Collider2D>();
        carSFXHandler = GetComponent<CarSoundEffectsHandler>();
    }


    void Start()
    {
    }


    void Update()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            steeringInput = data.direction.x;
            accelerationInput = data.direction.y;
        }

        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();

    }


    void ApplyEngineForce()
    {
        if (isJumping && accelerationInput < 0) 
        {
            accelerationInput = 0;
        }

        velocityVsUp = Vector2.Dot(transform.up,carRb.linearVelocity);

        if (velocityVsUp > maxSpeed && accelerationInput > 0) return;
        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0) return;
        if (carRb.linearVelocity.magnitude > maxSpeed * maxSpeed && accelerationInput > 0 && !isJumping) return;

        if (accelerationInput == 0)
        {
            carRb.linearDamping = Mathf.Lerp(carRb.linearDamping, 3.0f, Time.fixedDeltaTime * 3);
        }
        else
        {
            carRb.linearDamping = 0;
        }


        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
        carRb.AddForce(engineForceVector, ForceMode2D.Force);

    }

    void ApplySteering()
    {
        float minSpeedBeforeAllowTurningFactor = (carRb.linearVelocity.magnitude / 8);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);


        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;

        carRb.MoveRotation(rotationAngle);

    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRb.linearVelocity, transform.right);

        carRb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }



    
    float GetLateralVelocity()//How fast car moves sideways
    {
        return Vector2.Dot(transform.right, carRb.linearVelocity);
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBreaking)
    {
        lateralVelocity = GetLateralVelocity();
        isBreaking = false;

        if (isJumping)
        {
            return false;
        }

        if(accelerationInput < 0 && velocityVsUp > 0) //If moving forward and break - screech tires
        {
            isBreaking = true;
            return true;
        }


        if (Mathf.Abs(GetLateralVelocity()) > velocityToScreeching) //A lot side movement
            return true;

        return false;
    }

    public float GetVelocityMagnitude()
    {
        return carRb.linearVelocity.magnitude;
    }

    public void Jump(float jumpHeightScale, float jumpPushScale)
    {
        if (!isJumping && canJump)
        {
            StartCoroutine(JumpCo(jumpHeightScale, jumpPushScale));
        }
    }

    private IEnumerator JumpCo(float jumpHeightScale, float jumpPushScale)
    {
        isJumping = true;

        float jumpStartTime = Time.time;
        float jumpDuration = carRb.linearVelocity.magnitude * 0.05f;

        jumpHeightScale = jumpHeightScale * carRb.linearVelocity.magnitude * 0.05f;
        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        carCollider.enabled = false;

        carSFXHandler.PlayJumpingSFX();

        carSpriteRenderer.sortingLayerName = "Flying";
        carShadowRenderer.sortingLayerName = "Flying";

        carRb.AddForce(carRb.linearVelocity.normalized * jumpPushScale * 5, ForceMode2D.Impulse);

        while (isJumping)
        {
            //Percentage 0-1.0 where we are in the jumping progress
            float jumpCompletedPercentage = (Time.time - jumpStartTime) / jumpDuration;
            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            //Take the base scale of 1 and add how much we should increase the scale with
            carSpriteRenderer.transform.localScale = Vector3.one + Vector3.one * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //Change the shadow scale also but make it a bit smaller
            carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale * 0.75f;

            //Offset the shadow a bit
            carShadowRenderer.transform.localPosition = new Vector3(1,-1,0) * 1 * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;

            //When we reach 100% we are done
            if(jumpCompletedPercentage == 1.0f)
            {
                break;
            }

            yield return null;
        }
        /*
        if (Physics2D.OverlapCircle(transform.position, 1.5f))
        {
            isJumping = false;
            Jump(0.2f, 0.5f);
        }
        */
        carSpriteRenderer.transform.localScale = Vector3.one;

        carShadowRenderer.transform.localPosition = Vector3.zero;
        carShadowRenderer.transform.localScale = carSpriteRenderer.transform.localScale;

        carCollider.enabled = true;

        carSpriteRenderer.sortingLayerName = "Default";
        carShadowRenderer.sortingLayerName = "Default";

        if (jumpHeightScale > 0.2f)
        {
            landingParticleSystem.Play();

            carSFXHandler.PlayLandingSFX();
        }

        isJumping = false;
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Ramp"))
        {
            JumpData jumpData = collider2D.GetComponent<JumpData>();
            Jump(jumpData.jumpHeightScale, jumpData.jumpPushScale);
        }
        else if (collider2D.CompareTag("Booster"))
        {
            carRb.AddForce(carRb.linearVelocity.normalized * 1 * 10, ForceMode2D.Impulse);
        }
    }
}