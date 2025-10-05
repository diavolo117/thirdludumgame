using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    private int currentUpgrade = 0;

    // вызывается при подборе награды
    public void ApplyUpgrade()
    {
        currentUpgrade++;

        switch (currentUpgrade)
        {
            case 1:
                // Пример: увеличить скорость
                
                Debug.Log("Апгрейд 1: скорость увеличена");
                break;
            case 2:
                // Пример: увеличить урон
                
                Debug.Log("Апгрейд 2: урон увеличен");
                break;
            case 3:
                // Пример: больше здоровья
                
                Debug.Log("Апгрейд 3: здоровье увеличено");
                break;
            case 4:
                // Пример: быстрее перезарядка
                
                Debug.Log("Апгрейд 4: быстрее атаки");
                break;
            case 5:
                // Пример: пассивная регенерация
                
                Debug.Log("Апгрейд 5: регенерация");
                break;
            case 6:
                // Пример: финальная форма
                Debug.Log("Апгрейд 6: финальный апгрейд!");
                break;
            default:
                Debug.Log("Апгрейды закончились!");
                break;
        }
    }
}
