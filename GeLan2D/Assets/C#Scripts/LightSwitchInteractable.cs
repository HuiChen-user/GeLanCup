using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchInteractable : Interactable // �̳������ Interactable ����
{
    [Header("�ƹ���ƶ���")]
    public GameObject lightGroup; // ��ק��Hierarchy�д����ƹ⡱�ĸ�����򵥸�����
    public GameObject[] objectsToReveal; // һ�����飬���ڴ������������Ҫ��ʾ������

    
    [Header("��Ƶ����")]
    public AudioClip switchSound; // ����ʱ���ŵ���ЧƬ��
    private AudioSource audioSource; // ���ڲ�����Ч�����

    [Header("����״̬")]
    public bool canBeTurnedOff = false; // �Ƿ������ٴΰ�E�صƣ�Ĭ��ֻ�ܿ�һ��
    private bool isLightOn = false; // ��¼��ǰ�Ƶ�״̬
    public bool isUsed = false; // �����Ƿ��ѱ�ʹ�ù������ֻ�ܿ�һ�Σ�

    void Start()
    {
        // 1. ȷ����ʼ״̬���ƺ�Ҫ��ʾ�����嶼�����ص�
        if (lightGroup != null)
        {
            lightGroup.SetActive(false);
        }
        foreach (GameObject obj in objectsToReveal)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. ��ȡ�򴴽�AudioSource������ڲ�����Ч
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // ����������屾��û��AudioSource�����Զ����һ��
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f; // ����һ�����ʵ�Ĭ������
        }
        Debug.Log("�ƹ⿪�س�ʼ����ɣ���ʼ״̬���رա�");
    }

    // ���ģ�������ڷ�Χ�ڰ���Eʱ���ɻ����Զ�����
    protected override void OnInteract()
    {
        // �������ֻ��ʹ��һ�����ѱ�ʹ�ã���ֱ�ӷ���
        if (!canBeTurnedOff && isUsed) return;

        
        // �л��ƹ�״̬
        ToggleLight();
    }

    // �л��ƹ���ʾ/���صķ���
    void ToggleLight()
    {
        // ��ת״̬
        isLightOn = !isLightOn;
        isUsed = true;

        // 1. �������ƹ�
        if (lightGroup != null)
        {
            lightGroup.SetActive(isLightOn);
            Debug.Log($"���ƹ� {(isLightOn ? "����" : "�ر�")}");
        }

        // 2. ����������Ҫ����������
        foreach (GameObject obj in objectsToReveal)
        {
            if (obj != null)
            {
                obj.SetActive(isLightOn);
            }
        }

        // 3. ������Ч��ֻ�ڿ���ʱ���ţ����߸�����Ƶ�����
        if (isLightOn && switchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchSound);
            Debug.Log("���ſ�����Ч");
        }
        else if (!isLightOn && switchSound != null && audioSource != null)
        {
            // ������йص���Ч�����������ﲥ��
            // audioSource.PlayOneShot(switchOffSound);
        }

        // 4. �������ֻ�ܿ�һ�Σ�ʹ�ú�������ؽ�����ʾ��������ײ��
        if (!canBeTurnedOff && isUsed)
        {
            Debug.Log("������ʹ�ã����ú���������");
            // ���ص�ǰ�Ľ�����ʾ
            if (InteractionHintManager.Instance != null)
            {
                InteractionHintManager.Instance.HideHint();
            }
            // ���ô���������ֹ�ٴν���
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // ��ѡ���ڱ༭���п��ӻ�������Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // ������Ĵ�������һ��BoxCollider2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null && collider.isTrigger)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)collider.offset, collider.size);
        }
    }
}