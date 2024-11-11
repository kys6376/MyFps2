using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ������ �߻�ü�� �߻��Ҷ� �������� �߻�ü�� ���ӿ�����Ʈ ũ�� ����
    /// </summary>
    public class ChargedProjectileHandler : MonoBehaviour
    {
        #region Variables
        private ProjectileBase projectileBase;

        public GameObject chargeObject;
        public MinMaxVector3 scale;
        #endregion

        private void OnEnable()
        {
            //����
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        void OnShoot()
        {
            chargeObject.transform.localScale = scale.GetValueFromRatio(projectileBase.InitialCharge);
        }
    }
}
