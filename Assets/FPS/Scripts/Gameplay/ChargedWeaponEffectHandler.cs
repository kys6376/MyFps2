using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ChargedWeaponEffectHandler : MonoBehaviour
    {
        #region Variables
        public GameObject chargingObject;               //�����ϴ� �߻�ü
        public GameObject spiningFram;                  //�߻�ü�� ����� �ִ� ȸ���ϴ� ������
        public GameObject distOrbitParticlePrefab;      //�߻�ü�� ����� �ִ� ȸ���ϴ� ����Ʈ

        public MinMaxVector3 scale;                     //�߻�ü�� ũ�� ������

        [SerializeField] private Vector3 offset;
        public Transform parentTransform;

        public MinMaxFloat orbitY;                      //����Ʈ ������
        public MinMaxVector3 radius;                    //����Ʈ ������

        public MinMaxFloat spiningSpeed;                //ȸ�� ������

        //sfx
        public AudioClip chargeSound;
        public AudioClip loopChargeWeaponSfx;

        private float fadeLoopDuration = 0.5f;
        [SerializeField] public bool useProceduralPitchOnLoop;

        public float maxProceduralPitchValue = 2.0f;

        private AudioSource audioSource;
        private AudioSource audioSourceLoop;

        //
        public GameObject particleInstance {  get; private set; }
        private ParticleSystem disckOrbitParticle;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;

        private WeaponController weaponController;

        private float lastChargeTriggerTimeStamp;
        private float endChargeTime;
        private float chargeRatio;
        #endregion

        private void Awake()
        {
            //chargeSound play
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargeSound;
            audioSource.playOnAwake = false;

            //loopChargeWeaponSfx play
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeaponSfx;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
        }

        void SpawnParticleSystem()
        {
            particleInstance = Instantiate(distOrbitParticlePrefab, parentTransform != null ? parentTransform : transform);
            particleInstance.transform.localPosition += offset;

            FindReference();
        }

        void FindReference()
        {
            disckOrbitParticle = particleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = disckOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();
        }

        private void Update()
        {
            //�ѹ��� ��ü �����
            if (particleInstance == null)
            {
                SpawnParticleSystem();
            }

            disckOrbitParticle.gameObject.SetActive(weaponController.IsWeaponActive);
            chargeRatio = weaponController.CurrentCharge;

            //disk, frame
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);
            if(spiningFram)
            {
                spiningFram.transform.localRotation *= Quaternion.Euler(0f,
                    spiningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime,
                    0f); 
            }

            //VFX particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            disckOrbitParticle.transform.localScale = radius.GetValueFromRatio(chargeRatio);

            //SFX
            if(chargeRatio > 0f)
            {
                if(audioSourceLoop.isPlaying == false &&
                    weaponController.lastChargeTriggerTimeStamp > lastChargeTriggerTimeStamp)
                {
                    lastChargeTriggerTimeStamp = weaponController.lastChargeTriggerTimeStamp;
                    if (useProceduralPitchOnLoop == false)
                    {
                        endChargeTime = Time.time + chargeSound.length;
                        audioSource.Play();
                    }
                    else //
                    {
                        audioSource.Play();
                        audioSourceLoop.Play();
                    } //
                }
            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
            if(useProceduralPitchOnLoop == false)   //�ΰ��� ���� ���̵� ȿ���� ���� ǥ��
            {
                float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);
                audioSource.volume = volumeRatio;
                audioSourceLoop.volume = 1f - volumeRatio;
            }
            else //���������� ����ӵ��� ���� ȿ��
            {
                audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio);
            }
        }
    }
}
