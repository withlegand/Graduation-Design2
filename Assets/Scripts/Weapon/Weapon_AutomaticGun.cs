using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Header("武器部件位置")]
    [Tooltip("射击的位置")]public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹特效打出的位置
    [Tooltip("子弹壳抛出的位置")] public Transform CasingBulletSpawnPoint;

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

        RaycastHit hit;
        Vector3 shootDirection = ShootPoint.forward;//向前方射击
        shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor,SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position,shootDirection,out hit,range))
        {
            print(hit.transform.gameObject.name);
        }
        
        mainAudioSource.clip = SoundClips.shootSound;
        mainAudioSource.Play();
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

    public override void ExpaningCrossUpdate(float Degree)
    {
    }

    public override void Reload()
    {
    }
}
