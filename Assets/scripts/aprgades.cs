using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    private int currentUpgrade = 0;

    // ���������� ��� ������� �������
    public void ApplyUpgrade()
    {
        currentUpgrade++;

        switch (currentUpgrade)
        {
            case 1:
                // ������: ��������� ��������
                
                Debug.Log("������� 1: �������� ���������");
                break;
            case 2:
                // ������: ��������� ����
                
                Debug.Log("������� 2: ���� ��������");
                break;
            case 3:
                // ������: ������ ��������
                
                Debug.Log("������� 3: �������� ���������");
                break;
            case 4:
                // ������: ������� �����������
                
                Debug.Log("������� 4: ������� �����");
                break;
            case 5:
                // ������: ��������� �����������
                
                Debug.Log("������� 5: �����������");
                break;
            case 6:
                // ������: ��������� �����
                Debug.Log("������� 6: ��������� �������!");
                break;
            default:
                Debug.Log("�������� �����������!");
                break;
        }
    }
}
