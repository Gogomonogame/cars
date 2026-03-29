using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1; // Залиште для сумісності з Ghost/UI
    public bool isUiInput = false;

    private Vector2 inputVector = Vector2.zero;
    private bool jumpRequested = false;

    /*private void Update()
    {
        // Збираємо ввід ТІЛЬКИ якщо це локальний гравець
        // (Для безпеки можна додати перевірку на HasInputAuthority, якщо скрипт на NetworkObject)
        if (!isUiInput)
        {
            inputVector.x = Input.GetAxis("Horizontal"); // Використовуйте стандартні осі для мережі
            inputVector.y = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Jump")) jumpRequested = true;
        }
    }*/

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();
        Vector2 inputVector = Vector2.zero;

        // Зчитуємо ввід ТУТ. Fusion сам знає, чий це ввід, 
        // тому playerNumber та розділення на P1/P2 більше не потрібні.
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        networkInputData.direction = inputVector;
        return networkInputData;
    }

    public void SetInput(Vector2 newInput) // Для UI кнопок
    {
        inputVector = newInput;
    }
}