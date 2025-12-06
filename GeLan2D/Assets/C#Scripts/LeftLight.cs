using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftLight : Interactable
{
    public FlickerTimedLight2D targetLight;
    
    public bool canBeTurnedOff = false; // �Ƿ������ٴΰ�E�صƣ�Ĭ��ֻ�ܿ�һ��
    private bool isLightOn = false; // ��¼��ǰ�Ƶ�״̬
    public bool isUsed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void OnInteract()
    {
        // �������ֻ��ʹ��һ�����ѱ�ʹ�ã���ֱ�ӷ���
        

        targetLight.Activate();
        Debug.Log("111");
        
    }
}
