using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//������Ч�ڲ���
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;//������Ч
    public AudioClip silencershootsound;//������Ч��������
    public AudioClip reloadSoundAmmotLeft;//���ӵ���Ч
    public AudioClip reloadSoundOutOfAmmo;//���ӵ�����ǹ˨��һ����ϻ���꣩
    public AudioClip aimSound;//��׼��Ч
    
}

public class Weapon_AutomaticGun : Weapon
{
    [Header("��������λ��")]
    [Tooltip("�����λ��")]public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ���Ч�����λ��
    [Tooltip("�ӵ����׳���λ��")] public Transform CasingBulletSpawnPoint;

    [Header("ǹе����")]
    [Tooltip("�������")] public float range;
    [Tooltip("��������")] public float fireRate;
    private float originRate;//ԭʼ����
    private float SpreadFactor;//�����һ��ƫ����
    private float fireTimer;//��ʱ�� ������������
    private float bulletForce;//�ӵ��������
    [Tooltip("��ǰ������ÿ����ϻ�ӵ���")] public int bulletMag;
    [Tooltip("��ǰ�ӵ���")] public int currentBullets;
    [Tooltip("������")] public int bulletLeft;

    [Header("��Ч")]
    public Light muzzleFlashLight;//����ƹ�
    private float lightDuration;//�ƹ����ʱ��
    public ParticleSystem muzzlePatic;//�ƹ����������Ч1
    public ParticleSystem sparkPatic;//�ƹ����������Ч2�������ӣ�
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("��Դ")]
    private AudioSource mainAudioSource;
    public SoundClips SoundClips;

    private void Awake()
    {
        mainAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        muzzleFlashLight.enabled = false;
        lightDuration = 0.02f;
        range = 300f;
        bulletLeft = bulletMag * 5;
        currentBullets = bulletMag;
    }


    
    private void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            GunFire();//��ǹ���
        }

        //��ʱ��
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    //���
    public override void GunFire()
    {
        //1.��������
        //2.��ǰû���ӵ���
        //�����Է���
        if (fireTimer < fireRate || currentBullets <= 0) return;

        StartCoroutine(MuzzleFlashLight());//����ƹ�
        muzzlePatic.Emit(1);//����һ��ǹ�ڻ�������
        sparkPatic.Emit(Random.Range(minSparkEmission,maxSparkEmission));//����ǹ�ڻ�������

        RaycastHit hit;
        Vector3 shootDirection = ShootPoint.forward;//��ǰ�����
        shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor,SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position,shootDirection,out hit,range))
        {
            print(hit.transform.gameObject.name);
        }
        
        mainAudioSource.clip = SoundClips.shootSound;
        mainAudioSource.Play();
        fireTimer = 0;//���ü�ʱ��
        currentBullets--;//��ǰ�ӵ����ۼ�
    }

    //���ÿ���ĵƹ�
    public IEnumerator MuzzleFlashLight()
    {
        muzzleFlashLight.enabled=true;
        yield return new WaitForSeconds(lightDuration);
        muzzleFlashLight.enabled = false;
    }

    public override void AimIn()
    {
    }

    public override void AimOut()
    {
    }

    public override void DoReloadAnimation()
    {
    }

    public override void ExpaningCrossUpdate(float Degree)
    {
    }

    public override void Reload()
    {
    }
}
