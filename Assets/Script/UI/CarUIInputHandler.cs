using UnityEngine;

public class CarUIInputHandler : MonoBehaviour
{
    CarInputHandler playerCarInputHandler;
    Vector2 inputVector = Vector2.zero;
    private void Awake()
    {
        CarInputHandler[] carInputHandlers = FindObjectsOfType<CarInputHandler>();
        foreach (CarInputHandler handler in carInputHandlers)
        {
            if (handler.isUiInput)
            {
                playerCarInputHandler = handler;
                break;
            }
        }
    }

    public void OnAcceleratePress()
    {
        inputVector.y = 1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnBreakPress()
    {
        inputVector.y = -1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnAccelerateBreakRelease()
    {
        inputVector.y = 0.0f;
        playerCarInputHandler.SetInput(inputVector);
    }
    public void OnSteerLeftPress()
    {
        inputVector.x = -1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnSteerRightPress()
    {
        inputVector.x = 1.0f;
        playerCarInputHandler.SetInput(inputVector);
    }

    public void OnSteerRelease()
    {
        inputVector.x = 0.0f;
        playerCarInputHandler.SetInput(inputVector);
    }


}
