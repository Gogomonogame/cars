using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUiInput = false;

    Vector2 inputVector = Vector2.zero;

    CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    private void Update()
    {
        if (isUiInput)
        {

        }
        else
        {
            inputVector = Vector2.zero;
            switch (playerNumber)
            {
                case 1:
                    inputVector.x = Input.GetAxis("Horizontal_P1");
                    inputVector.y = Input.GetAxis("Vertical_P1");
                    break;
                case 2:
                    inputVector.x = Input.GetAxis("Horizontal_P2");
                    inputVector.y = Input.GetAxis("Vertical_P2");
                    break;
                case 3:
                    inputVector.x = Input.GetAxis("Horizontal_P3");
                    inputVector.y = Input.GetAxis("Vertical_P3");
                    break;
                case 4:
                    inputVector.x = Input.GetAxis("Horizontal_P4");
                    inputVector.y = Input.GetAxis("Vertical_P4");
                    break;
            }
            if (Input.GetButtonDown("Jump"))
            {
                carController.Jump(1, 0);
            }
        }
        carController.SetInputVector(inputVector);

        
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();
        networkInputData.direction = inputVector;

        return networkInputData;
    }



}
