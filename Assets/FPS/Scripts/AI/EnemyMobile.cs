using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Enemy ����
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Attack
    }

    /// <summary>
    /// �̵��ϴ� Enemy�� ���µ��� �����ϴ� Ŭ����
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables
        public Animator animator;
        private EnemyController enemyController;

        public AIState Aistate {  get; private set; }

        public AudioClip movementSound;
        public MinMaxFloat pitchMovementSpeed;

        private AudioSource audioSource;

        //animation parameter
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamager";
        const string k_AnimDeathParameter = "Death";
        #endregion

        private void Start()
        {
            //����
            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            //�ʱ�ȭ
            Aistate = AIState.Patrol;
        }

        private void Update()
        {

        }

        //���¿� ���� Enemy ����
        private void UpdateCurrentAiState()
        {
            switch(Aistate)
            {
                case AIState.Patrol:
                    enemyController.UpdatePathDestination(true);
                    enemyController.SetNavDestination(enemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    break;
                case AIState.Attack:
                    break;
            }
        }

        private void OnDamaged()
        {
            
        }
    }
}
