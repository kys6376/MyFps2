using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 적 디텍팅 구현
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables
        private ActorManager actorManagert;

        public UnityAction OnDetectedTarget;     //적을 감지하면 등록된 함수 호출
        public UnityAction OnLostTarget;         //적을 놓치면 등록된 함수 호출
        #endregion

        private void Start()
        {
            //참조
            actorManagert = GameObject.FindObjectOfType<ActorManager>();
        }

        //디텍팅
        public void HandTargetDetection(Actor actor, Collider[] selfCollider)
        {

        }

        //적을 감지하면 
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }

        //적을 놓치면
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }
    }
}
