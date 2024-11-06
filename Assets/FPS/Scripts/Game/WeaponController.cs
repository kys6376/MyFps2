using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ũ�ν��� �׸������� ������
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public float CrossHairSize;
        public Color CrossHairColor;
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

        //CorssHair
        public CrossHairData crosshairDefault;              //�⺻, ����
        public CrossHairData crosshairTargetInSight;        //���� ����������, Ÿ���� �Ǿ�����

        //����
        public float aimZoomRatio = 1f;         //���ؽ� ���� ������
        public Vector3 aimOffset;               //���ؽ� ���� ��ġ ������
        #endregion

        private void Awake()
        {
            //����
            shootAudioSource = this.GetComponent<AudioSource>();
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

    }
}