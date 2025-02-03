using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public SoundClips soundClips;//
    private Animator animator;
    private PlayerController playerController;

    public bool IS_AUTORIFLE;//�Ƿ����Զ�����
    public bool IS_SEMIGUN;//�Ƿ��ǰ��Զ�����

    [Header("��������λ��")]
    [Tooltip("�����λ��")]public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ���Ч�����λ��
    [Tooltip("�ӵ����׳���λ��")] public Transform CasingBulletSpawnPoint;

    [Header("�ӵ�Ԥ�������Ч")]
    public Transform bulletPrefab;//
    public Transform casingPrefab;//

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

    [Header("UI")]
    public Image[] crossQuarterImgs;//׼��
    public float currentExpanedDegree;//��ǰ׼�ĵĿ��϶�
    private float crossExpanedDegree;//ÿ֡׼�Ŀ��϶�
    private float maxCrossDegree;//��󿪺϶�
    public Text ammoTextUI;
    public Text shootModeTextUI;

    public PlayerController.MovementsState state;

    [Header("��λ����")]
    [SerializeField][Tooltip("��װ�ӵ�����")]private KeyCode reloadInputName = KeyCode.R;
    [Tooltip("�Զ����Զ��л�����")]private KeyCode inspectlInputName = KeyCode.I;
    [Tooltip("�Զ����Զ��л�����")] private KeyCode GunShootModelInputName = KeyCode.X;

    /*ʹ��ö������ȫ�Զ�����Զ�*/
    public ShootMode shootingMode;
    private bool GunShootInput;//����ȫ�Զ��Ͱ��Զ� ����ļ�λ���뷢���仯
    private int modeNum;//ģʽ�л���һ���м������1��ȫ�Զ�ģʽ��2�����Զ�ģʽ��
    private string shootModeName;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();
        mainAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        muzzleFlashLight.enabled = false;
        crossExpanedDegree = 50f;
        maxCrossDegree = 300f;
        lightDuration = 0.02f;
        range = 300f;
        bulletForce = 100f;
        bulletLeft = bulletMag * 5;
        currentBullets = bulletMag;
        originRate = fireRate;//
        UpdateAmmoUI();

        //���ݲ�ͬǹе ��Ϸ�տ�ʼʱʹ�ò�ͬ���ģʽ����
        if (IS_AUTORIFLE)
        {
            modeNum = 1;
            shootModeName = "ȫ�Զ�";
            shootingMode = ShootMode.AutoRifle;
            UpdateAmmoUI();
        }
        if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "���Զ�";
            shootingMode = ShootMode.SemiGun;
            UpdateAmmoUI();
        }
    }


    
    private void Update()
    {
        //�Զ�ǹе������뷽ʽ ������ GeyMouseButton �� GetMouseButtonDown ���л�
        if (IS_AUTORIFLE)
        {
            //�л����ģʽ��ȫ�Զ��Ͱ��Զ�)
            if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
            {
                modeNum = 1;
                shootModeName = "ȫ�Զ�";
                shootingMode = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "���Զ�";
                shootingMode = ShootMode.SemiGun;
                UpdateAmmoUI();
            }

            switch (shootingMode)
            {
                case ShootMode.AutoRifle:
                    GunShootInput =  Input.GetMouseButton(0);
                    fireRate = originRate;
                    break;
                case ShootMode.SemiGun:
                    GunShootInput = Input.GetMouseButtonDown(0);
                    fireRate = 0.2f;      
                    break;
            }
        }
        else
        {
            GunShootInput = Input.GetMouseButtonDown(0);
        }

        state = playerController.state;
        if (state ==PlayerController.MovementsState.walking && Vector3.SqrMagnitude(playerController.moveDirection)>0 && state != PlayerController.MovementsState.runing && state != PlayerController.MovementsState.crouching)
        {
            //�ƶ�ʱ��׼�Ŀ��϶�
            ExpaningCrossUpdate(crossExpanedDegree);
        }
        else if (state != PlayerController.MovementsState.walking && state == PlayerController.MovementsState.runing && state != PlayerController.MovementsState.crouching)
        {
            //����ʱ��׼�Ŀ��϶ȣ�2����
            ExpaningCrossUpdate(crossExpanedDegree*2);
        }
        else
        {
            //վ�������¶ף�������׼�Ŀ��϶�
            ExpaningCrossUpdate(0);
        }

        //���»�����,��ǰ�ӵ���С�ڵ�ϻ�ӵ���������������0
        //�ж�����û�л�����ʱ�� �����в��Ż�������
        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletMag && bulletLeft > 0  )
        {
            //Reload();
            //TODO
            DoReloadAnimation();
        }

        SpreadFactor = 0.1f;

        if (GunShootInput && currentBullets >0)
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
        if (fireTimer < fireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("take_out")) return;

        StartCoroutine(MuzzleFlashLight());//����ƹ�
        muzzlePatic.Emit(1);//����һ��ǹ�ڻ�������
        sparkPatic.Emit(Random.Range(minSparkEmission,maxSparkEmission));//����ǹ�ڻ�������
        StartCoroutine(Shoot_Cross());

        //������ͨ���𶯻���ʹ�ö����ĵ��뵭��Ч����
        animator.CrossFadeInFixedTime("fire",0.1f);

        RaycastHit hit;
        Vector3 shootDirection = ShootPoint.forward;//��ǰ�����
        shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor,SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position,shootDirection,out hit,range))
        {
            Transform bullet;

            bullet =  (Transform)Instantiate(bulletPrefab,BulletShootPoint.transform.position,BulletShootPoint.transform.rotation);
            //���ӵ���βһ����ǰ���ٶ���(�������ߴ��ȥ��ƫ��ֵ��
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;
            print(hit.transform.gameObject.name);
        }
        //ʵ���׵���
        Instantiate(casingPrefab,CasingBulletSpawnPoint.transform.position,CasingBulletSpawnPoint.transform.rotation);
        
        mainAudioSource.clip = SoundClips.shootSound;
        mainAudioSource.Play();//�����������Ч
        fireTimer = 0;//���ü�ʱ��
        currentBullets--;//��ǰ�ӵ����ۼ�
        UpdateAmmoUI();


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

    
    public override void Reload()
    {
        if (bulletLeft <= 0) return;
        //������Ҫ�����ӵ�
        int bulletToLoad = bulletMag - currentBullets;
        //���㱸���۳����ӵ���
        int bulletToReduce =  bulletLeft >= bulletToLoad ?bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce;//��������
        currentBullets += bulletToReduce;//��ǰ�ӵ�����
        UpdateAmmoUI();
    }
    
    public override void ExpaningCrossUpdate(float expandDegree)
    {
        if (currentExpanedDegree < expandDegree - 5)
        {
            ExpendCross(150*Time.deltaTime);
        }
        else if (currentExpanedDegree > expandDegree + 5)
        {
            ExpendCross(-300 * Time.deltaTime);
        }
    }

    //�ı�׼�ĵĿ��϶� ����¼��ǰ׼�Ŀ��϶�
    public void ExpendCross(float add)
    {
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add,0,0);//��׼��
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0,0);//��׼��
        crossQuarterImgs[2].transform.localPosition += new Vector3(-0,add,0);//��׼��
        crossQuarterImgs[3].transform.localPosition += new Vector3(0,-add,0);//��׼��

        currentExpanedDegree += add;//���浱ǰ׼�Ŀ��϶�
        currentExpanedDegree = Mathf.Clamp(currentExpanedDegree, 0, maxCrossDegree);//����׼�Ŀ��϶ȴ�С

    }

    //Я�� ����׼�Ŀ��϶� 1ִ֡��5��
    //ֻ�������ʱ˲������׼��
    public IEnumerator Shoot_Cross()
    {
        yield return null;
        for (int i = 0; i<5;i++)
        {
            ExpendCross(Time.deltaTime * 500);
        }
    }

    //�����ӵ�UI
    public void UpdateAmmoUI()
    {
        ammoTextUI.text = currentBullets + "/" + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    public enum ShootMode
    {
        AutoRifle,
        SemiGun
    };
}
