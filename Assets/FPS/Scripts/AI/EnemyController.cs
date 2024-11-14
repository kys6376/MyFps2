using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        /// <summary>
        /// ������ ������: ���͸��� ����
        /// </summary>
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer renderer;
            public int matarialIndex;

            public RendererIndexData(Renderer _renderer, int index)
            {
                renderer = _renderer;
                matarialIndex = index;
            }
        }

        /// <summary>
        /// Ememy�� �����ϴ� Ŭ����
        /// </summary>
        #region Variables
        private Health health;

        //death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPosition;

        //damage
        public UnityAction Damaged;

        //Sfx
        public AudioClip damagesSfx;

        //Vfx
        public Material bodyMaterial;           //�������� �� ���͸���
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;      //������ ȿ���� �÷� �׶���Ʈ ȿ���� ǥ��
        //body Material�� ������ �ִ� ������ ����Ʈ
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField] private float flashOnHitDuration = 0.5f;
        float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagedThisFrame = false;

        //Patrol
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath patrolPath { get; set; }
        private int pathDestinationIndex;               //��ǥ ��������Ʈ �ε���
        private float pathReachingRadius = 1f;          //��������
        #endregion

        private void Start()
        {
            //����
            Agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            //body Material�� ������ �ִ� ������ ������ ���� ����Ʈ �����
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            //
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            //������ ȿ��
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.matarialIndex);
            }


            //
            wasDamagedThisFrame = false;
        }

        private void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && damageSource.GetComponent<EnemyController>() == null)
            {
                //��ϵ� �Լ� ȣ��
                Damaged?.Invoke();

                //�������� �� �ð�
                lastTimeDamaged = Time.time;

                //Sfx
                if (damagesSfx && wasDamagedThisFrame == false)
                {
                    AudioUtility.CreateSfx(damagesSfx, this.transform.position, 0f);
                }
                wasDamagedThisFrame = true;
            }
        }

        private void OnDie()
        {
            //���� ȿ��
            GameObject effectGo = Instantiate(deathVfxPrefab, deathVfxSpawnPosition.position, Quaternion.identity);
            Destroy(effectGo, 5f);

            //Enemy ų
            Destroy(gameObject);
        }

        //��Ʈ���� ��ȿ����? ��Ʈ���� ��������?
        private bool IsPathVaild()
        {
            return patrolPath && patrolPath.wayPoints.Count > 0;
        }

        //���� ����� WayPoint ã��
        private void SetPathDestinationToClosestWayPoint()
        {
            if (IsPathVaild() == false)
            {
                pathDestinationIndex = 0;
                return;
            }

            int closestWayPointIndex = 0;

            for (int i = 0; i < patrolPath.wayPoints.Count; i++)
            {
                float distance = patrolPath.GetDistanceToWaypoint(transform.position, i);
                float closestDistance = patrolPath.GetDistanceToWaypoint(transform.position, closestWayPointIndex);
                if (distance < closestDistance)
                {
                    closestWayPointIndex = i;
                }
            }
            pathDestinationIndex = closestWayPointIndex;
        }

        //��ǥ ������ ��ġ �� ������
        public Vector3 GetDestinationOnPath()
        {
            if (IsPathVaild() == false)
            {
               return this.transform.position;
            }

            return patrolPath.GetPositionOfWayPoint(pathDestinationIndex);
        }

        //��ǥ ���� ���� - Nav �ý��� �̿�
        public void SetNavDestination(Vector3 destination)
        {
            if(Agent)
            {
                Agent.SetDestination(destination);
            }
        }

        //���� ���� �� ���� ��ǥ���� ����
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathVaild() == false)
                return;

            //��������
            float distance = (transform.position - GetDestinationOnPath()).magnitude;
            if(distance <= pathReachingRadius)
            {
                pathDestinationIndex = inverseOrder ? (pathDestinationIndex - 1) : (pathDestinationIndex + 1);
                if(pathDestinationIndex < 0)
                {
                    pathDestinationIndex += patrolPath.wayPoints.Count;
                }
                if(pathDestinationIndex >= patrolPath.wayPoints.Count)
                {
                    pathDestinationIndex -= patrolPath.wayPoints.Count;
                }
            }
        }
    }
}
