using System.Collections;
using System.Collections.Generic;
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
    private PlayerController playerController;

    [Header("武器部件位置")]
    [Tooltip("射击的位置")]public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹特效打出的位置
    [Tooltip("子弹壳抛出的位置")] public Transform CasingBulletSpawnPoint;

    [Header("子弹预制体和特效")]
    public Transform bulletPrefab;//
    public Transform casingPrefab;//

    [Header("枪械属性")]
    [Tooltip("武器射程")] public float range;
    [Tooltip("武器射速")] public float fireRate;
    private float originRate;//原始射速
    private float SpreadFactor;//射击的一点偏移量
    private float fireTimer;//计时器 控制武器射速
    private float bulletForce;//子弹发射的力
    [Tooltip("当前武器的每个弹匣子弹数")] public int bulletMag;
    [Tooltip("当前子弹数")] public int currentBullets;
    [Tooltip("备弹数")] public int bulletLeft;

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

    [Header("UI")]
    public Image[] crossQuarterImgs;//准心
    public float currentExpanedDegree;//当前准心的开合度
    private float crossExpanedDegree;//每帧准心开合度
    private float maxCrossDegree;//最大开合度
    public Text ammoTextUI;
    public Text shootModeTextUI;

    public PlayerController.MovementsState state;

    private void Awake()
    {
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
    }


    
    private void Update()
    {
        state = playerController.state;//
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

        if (Input.GetMouseButton(0))
        {
            GunFire();//开枪射击
        }

        //计时器
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    //射击
    public override void GunFire()
    {
        //1.控制射速
        //2.当前没有子弹了
        //不可以发射
        if (fireTimer < fireRate || currentBullets <= 0) return;

        StartCoroutine(MuzzleFlashLight());//开火灯光
        muzzlePatic.Emit(1);//发射一个枪口火焰粒子
        sparkPatic.Emit(Random.Range(minSparkEmission,maxSparkEmission));//发射枪口火星粒子
        StartCoroutine(Shoot_Cross());

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
        Instantiate(casingPrefab,CasingBulletSpawnPoint.transform.position,CasingBulletSpawnPoint.transform.rotation);
        
        mainAudioSource.clip = SoundClips.shootSound;
        mainAudioSource.Play();//播放设射击音效
        fireTimer = 0;//重置计时器
        currentBullets--;//当前子弹数累减

        
    }

    //设置开火的灯光
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
}
