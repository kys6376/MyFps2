using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ũ�ν��� �׸��� ���� ������
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public float CrossHairSize;
        public Color CrossHairColor;
    }

    /// <summary>
    /// ���� �� Ÿ��
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Sniper,
    }

    /// <summary>
    /// ����(�ѱ�)�� �����ϴ� Ŭ����
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //���� Ȱ��ȭ, ��Ȱ��ȭ
        public GameObject weaponRoot;

        public GameObject Owner { get; set; }           //������ ����
        public GameObject SourcePrefab { get; set; }    //���⸦ ������ �������� ������
        public bool IsWeaponActive { get; private set; }    //���� Ȱ��ȭ ����

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;

        //Shooting
        public WeaponShootType shootType;

        [SerializeField] private float maxAmmo = 8f;            //�����Ҽ� �ִ� �ִ� �Ѿ� ����
        private float currentAmmo;

        [SerializeField] private float delayBetweenShots = 0.5f;    //�� ����
        private float lastTimeShot;                                 //���������� ���� �ð�

        //Vfx, Sfx
        public Transform weaponMuzzle;                              //�ѱ� ��ġ
        public GameObject muzzleFlashPrefab;                        //�ѱ� �߻� ����Ʈ ȿ��
        public AudioClip shootSfx;                                  //�� �߻� ����

        //CrossHair
        public CrossHairData crosshairDefault;              //�⺻, ����
        public CrossHairData crosshairTargetInSight;        //���� ����������, Ÿ���� �Ǿ�����

        //����
        public float aimZoomRatio = 1f;             //���ؽ� ���� ������
        public Vector3 aimOffset;                   //���ؽ� ���� ��ġ ������

        //�ݵ�
        public float recoilForce = 0.5f;

        //Projectile
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleWorldVelocity { get; private set; }
        private Vector3 lastMuzzlePosition;
        public float CurrentCharge { get; private set; }
        #endregion

        private void Awake()
        {
            //����
            shootAudioSource = this.GetComponent<AudioSource>();
        }

        private void Start()
        {
            //�ʱ�ȭ
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time;
        }

        //���� Ȱ��ȭ, ��Ȱ��ȭ
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            //this ����� ����
            if (show == true && switchWeaponSfx != null)
            {
                //���� ���� ȿ���� �÷���
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }

            IsWeaponActive = show;
        }

        //Ű �Է¿� ���� �� Ÿ�� ����
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch (shootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Automatic:
                    if (inputHeld)
                    {
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Charge:
                    break;
                case WeaponShootType.Sniper:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    break;
            }

            return false;
        }

        bool TryShoot()
        {
            if (currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                Debug.Log($"currentAmmo: {currentAmmo}");

                HandleShoot();
                return true;
            }

            return false;
        }

        //�� ����
        void HandleShoot()
        {
            //Vfx
            if (muzzleFlashPrefab)
            {
                GameObject effectGo = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle);
                Destroy(effectGo, 2f);
            }

            //Sfx
            if (shootSfx)
            {
                shootAudioSource.PlayOneShot(shootSfx);
            }

            //���� �ð� ����
            lastTimeShot = Time.time;
        }

    }
}