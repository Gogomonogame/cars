using System;
using UnityEngine;

public class CarSoundEffectsHandler : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource tireScreechAudioSource;
    public AudioSource engineAudioSource;
    public AudioSource carHitAudioSource;
    public AudioSource carJumpingAudioSource;
    public AudioSource carLandingAudioSource;

    float desiredEnginePitch = 0.5f;
    float tireScreechPitch = 0.5f;

    CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    private void Update()
    {
        UpdateEngineSFX();
        UpdateTiresScreechSFX();
    }

    private void UpdateTiresScreechSFX()
    {
        //Handle tire screeching
        if (carController.IsTireScreeching(out float lateralVelocity, out bool isBreaking))
        {
            //If breaking we want louder and change the pitch
            if(isBreaking)
            {
                tireScreechAudioSource.volume = Mathf.Lerp(tireScreechAudioSource.volume, 1.0f, Time.deltaTime * 10);
                tireScreechPitch = Mathf.Lerp(tireScreechPitch, 0.5f, Time.deltaTime * 10);
            }
            else
            {
                //If not breaking play when drifting
                tireScreechAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                tireScreechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        }
        else
        {
            tireScreechAudioSource.volume = Mathf.Lerp(tireScreechAudioSource.volume, 0, Time.deltaTime * 10);
        }
    }

    private void UpdateEngineSFX()
    {
        //Handle engine SFX
        float velocityMagnitude = carController.GetVelocityMagnitude();
        //Increase volume as car goes faster
        float desiredEngineVolume = Mathf.Abs(velocityMagnitude * 0.05f);
        //Keep minimum level of sound
        desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, 0.2f, 1.0f);
        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolume, Time.deltaTime * 10);
        //To add more variation to the engine sound change the pitch
        desiredEnginePitch = velocityMagnitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relativeVelocity = collision.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        carHitAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        carHitAudioSource.volume = volume;

        if(!carHitAudioSource.isPlaying)
        {
            carHitAudioSource.Play();
        }
    }

    public void PlayJumpingSFX()
    {
        carJumpingAudioSource.Play();
    }

    public void PlayLandingSFX()
    {
        carLandingAudioSource.Play();
    }

}
