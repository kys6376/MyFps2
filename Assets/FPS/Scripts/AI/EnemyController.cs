using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        //death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPosition;
        #endregion

        private void Start()
        {
            //����
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

        }

        private void OnDamaged(float damage, GameObject damageSource)
        {

        }

        private void OnDie()
        {
            //���� ȿ��
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPosition.position, Quaternion.identity);
            Destroy(effectGo, 5f);

            //Enemy ų
            Destroy(gameObject);
        }
    }
}
