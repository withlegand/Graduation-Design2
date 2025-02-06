using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

//武器音效内部类
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;//开火音效
    public AudioClip silencershootsound;//开火音效带消音器
    public AudioClip reloadSoundAmmotLeft;//换子弹音效
    public AudioClip reloadSoundOutOfAmmo;//换子弹并拉枪栓（一个弹匣打完）
    public AudioClip aimSound;//瞄准音效
    
}

public class Weapon_AutomaticGun : Weapon
{
    
    private Animator animator;
    private PlayerController playerController;
    private Camera mainCamera;
    public Camera gunCamera;

    public bool IS_AUTORIFLE;//是否是自动武器
    public bool IS_SEMIGUN;//是否是半自动武器

    [Header("武器部件位置")]
    [Tooltip("射击的位置")]public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹特效打出的位置
    [Tooltip("子弹壳抛出的位置")] public Transform CasingBulletSpawnPoint;

    [Header("子弹预制体和特效")]
    public Transform bulletPrefab;//子弹
    public Transform casingPrefab;//子弹抛壳

    [Header("枪械属性")]
    [Tooltip("武器射程")] private float range;
    [Tooltip("武器射速")] public float fireRate;
    private float originRate;//原始射速
    private float SpreadFactor;//射击的一点偏移量
    private float fireTimer;//计时器 控制武器射速
    private float bulletForce;//子弹发射的力
    [Tooltip("当前武器的每个弹匣子弹数")] public int bulletMag;
    [Tooltip("当前子弹数")] public int currentBullets;
    [Tooltip("备弹数")] public int bulletLeft;
    public bool isSilencer;//是否装备消音器

    [Header("特效")]
    public Light muzzleFlashLight;//开火灯光
    private float lightDuration;//灯光持续时间
    public ParticleSystem muzzlePatic;//灯光火焰粒子特效1
    public ParticleSystem sparkPatic;//灯光火焰粒子特效2（火星子）
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("音源")]
    private AudioSource mainAudioSource;
    public SoundClips SoundClips;
    public SoundClips shootAudioSource;

    [Header("UI")]
    public Image[] crossQuarterImgs;//准心
    public float currentExpanedDegree;//当前准心的开合度
    private float crossExpanedDegree;//每帧准心开合度
    private float maxCrossDegree;//最大开合度
    public Text ammoTextUI;
    public Text shootModeTextUI;

    public PlayerController.MovementsState state;
    private bool isReloading;//判断是否在装弹
    private bool isAiming;//判断是否在瞄准

    private Vector3 sniperingFiflePosition;//枪默认的初始位置
    public  Vector3 sniperingFifleOnPosition;//开始瞄准的模型位置

    [Header("键位设置")]
    [SerializeField][Tooltip("填装子弹按键")]private KeyCode reloadInputName = KeyCode.R;
    [Tooltip("自动半自动切换按键")]private KeyCode inspectlInputName = KeyCode.F;
    [Tooltip("自动半自动切换按键")] private KeyCode GunShootModelInputName = KeyCode.X;

    /*使用枚举区分全自动与半自动*/
    public ShootMode shootingMode;
    private bool GunShootInput;//根据全自动和半自动 射击的键位输入发生变化
    private int modeNum;//模式切换的一个中间参数（1，全自动模式；2，半自动模式）
    private string shootModeName;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();
        mainAudioSource = GetComponent<AudioSource>();
        //shootAudioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        sniperingFiflePosition = transform.localPosition;
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

        //根据不同枪械 游戏刚开始时使用不同射击模式设置
        if (IS_AUTORIFLE)
        {
            modeNum = 1;
            shootModeName = "全自动";
            shootingMode = ShootMode.AutoRifle;
            UpdateAmmoUI();
        }
        if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "半自动";
            shootingMode = ShootMode.SemiGun;
            UpdateAmmoUI();
        }
    }


    
    private void Update()
    {
        //自动枪械鼠标输入方式 可以在 GeyMouseButton 和 GetMouseButtonDown 里切换
        if (IS_AUTORIFLE)
        {
            //切换射击模式（全自动和半自动)
            if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
            {
                modeNum = 1;
                shootModeName = "全自动";
                shootingMode = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "半自动";
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
            //移动时的准心开合度
            ExpaningCrossUpdate(crossExpanedDegree);
        }
        else if (state != PlayerController.MovementsState.walking && state == PlayerController.MovementsState.runing && state != PlayerController.MovementsState.crouching)
        {
            //奔跑时的准心开合度（2倍）
            ExpaningCrossUpdate(crossExpanedDegree*2);
        }
        else
        {
            //站立或者下蹲，不调整准心开合度
            ExpaningCrossUpdate(0);
        }

        

        if (GunShootInput && currentBullets >0)
        {
            GunFire();//开枪射击
        }

        //计时器
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        //播放行走跑步动画
        animator.SetBool("Run",playerController.isRun);
        animator.SetBool("Walk", playerController.isWalk);
        

        //两种换子弹动画
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if ((info.IsName("reload_ammo_left")) || info.IsName("reload_out_of_ammo"))
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }


        //按下换单键,当前子弹数小于弹匣子弹数，备弹量大于0
        //判断现在没有换弹的时候 才运行播放换弹动画
        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletMag && bulletLeft > 0 && !isReloading )
        {
            
            DoReloadAnimation();
        }

        //鼠标右键进入瞄准
        if (Input.GetMouseButton(1) && !isReloading && !playerController.isRun)
        {
            isAiming = true;
            animator.SetBool("Aim",isAiming);
            //瞄准时需要微调一下枪的模型位置
            transform.localPosition = sniperingFifleOnPosition;
        }
        else
        {
            isAiming = false;
            animator.SetBool("Aim", isAiming);
            //瞄准时需要微调一下枪的模型位置
            transform.localPosition = sniperingFiflePosition;
        }


        //腰射和瞄准射击精度不同
        SpreadFactor = (isAiming) ? 0.01f: 0.1f;

        if (Input.GetKeyDown(inspectlInputName))
        {
            animator.SetTrigger("Inspect");
        }
    }

    //射击
    public override void GunFire()
    {
        //1.控制射速
        //2.当前没有子弹了
        //不可以发射
        if (fireTimer < fireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("take_out") || isReloading) return;

        StartCoroutine(MuzzleFlashLight());//开火灯光
        muzzlePatic.Emit(1);//发射一个枪口火焰粒子
        sparkPatic.Emit(Random.Range(minSparkEmission,maxSparkEmission));//发射枪口火星粒子
        StartCoroutine(Shoot_Cross());

        //播放普通开火动画（使用动画的淡入淡出效果）
        animator.CrossFadeInFixedTime("fire",0.1f);

        if (!isAiming)
        {
            //播放普通开火动画（使用动画的淡入淡出效果）
            animator.CrossFadeInFixedTime("Fire", 0.1f);
        }
        else
        {
            //瞄准状态下，播放瞄准开火动画
            animator.Play("aim_fire", 0, 0);
        }

        RaycastHit hit;
        Vector3 shootDirection = ShootPoint.forward;//向前方射击
        shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor,SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position,shootDirection,out hit,range))
        {
            Transform bullet;

            bullet =  (Transform)Instantiate(bulletPrefab,BulletShootPoint.transform.position,BulletShootPoint.transform.rotation);
            //给子弹拖尾一个向前的速度力(加上射线打出去的偏移值）
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;
            print(hit.transform.gameObject.name);
        }
        //实例抛弹壳
        Instantiate(casingPrefab,CasingBulletSpawnPoint.transform.position,CasingBulletSpawnPoint.transform.rotation);//实例抛弹壳
        //根据是否装备消音器，切换不同的射击音效
        mainAudioSource.clip = isSilencer?SoundClips.silencershootsound: SoundClips.shootSound;
        mainAudioSource.Play();//播放设射击音效
        fireTimer = 0;//重置计时器
        currentBullets--;//当前子弹数累减
        UpdateAmmoUI();


    }

    //设置开火的灯光
    public IEnumerator MuzzleFlashLight()
    {
        muzzleFlashLight.enabled=true;
        yield return new WaitForSeconds(lightDuration);
        muzzleFlashLight.enabled = false;
    }

    //进入瞄准，隐藏准心，摄像机视野变近
    public override void AimIn()
    {
        float currentVelocity = 0f;
        for (int i = 0; i < crossQuarterImgs.Length; i++)
        {
            crossQuarterImgs[i].gameObject.SetActive(false);
        }

        //瞄准时摄像机视野变近
        mainCamera.fieldOfView = Mathf.SmoothDamp(30,60,ref currentVelocity,0.1f);
        mainAudioSource.clip = SoundClips.aimSound;
        mainAudioSource.Play();
        print("miaozhun");
    }

    //
    public override void AimOut()
    {
        float currentVelocity = 0f;
        for (int i = 0; i < crossQuarterImgs.Length; i++)
        {
            crossQuarterImgs[i].gameObject.SetActive(true);
        }

        
        mainCamera.fieldOfView = Mathf.SmoothDamp(60, 30, ref currentVelocity, 0.5f);
        mainAudioSource.clip = SoundClips.aimSound;
        mainAudioSource.Play();
    }

    public override void DoReloadAnimation()
    {
        if (currentBullets > 0 && bulletLeft > 0)
        {
            animator.Play("reload_ammo_left",0,0);
            Reload();
            mainAudioSource.clip =SoundClips.reloadSoundAmmotLeft;
            mainAudioSource.Play();
        }
        if (currentBullets == 0 && bulletLeft > 0)
        {
            animator.Play("reload_out_of_ammo", 0, 0);
            Reload();
            mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
            mainAudioSource.Play();
        }
    }

    
    public override void Reload()
    {
        if (bulletLeft <= 0) return;
        //计算需要填充的子弹
        int bulletToLoad = bulletMag - currentBullets;
        //计算备弹扣除的子弹数
        int bulletToReduce =  bulletLeft >= bulletToLoad ?bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce;//备弹减少
        currentBullets += bulletToReduce;//当前子弹增加
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

    //改变准心的开合度 并记录当前准心开合度
    public void ExpendCross(float add)
    {
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add,0,0);//左准心
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0,0);//右准心
        crossQuarterImgs[2].transform.localPosition += new Vector3(-0,add,0);//上准心
        crossQuarterImgs[3].transform.localPosition += new Vector3(0,-add,0);//下准心

        currentExpanedDegree += add;//保存当前准心开合度
        currentExpanedDegree = Mathf.Clamp(currentExpanedDegree, 0, maxCrossDegree);//限制准心开合度大小

    }

    //携程 调用准心开合度 1帧执行5次
    //只负责射击时瞬间增大准心
    public IEnumerator Shoot_Cross()
    {
        yield return null;
        for (int i = 0; i<5;i++)
        {
            ExpendCross(Time.deltaTime * 500);
        }
    }

    //更新子弹UI
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
