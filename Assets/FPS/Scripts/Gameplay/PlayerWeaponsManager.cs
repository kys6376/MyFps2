using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ���� ��ü ����
    /// </summary>
    public enum WeaponSwithState
    {
        Up,
        Down,
        PutDownPrvious,
        PutUpNew,
    }

    /// <summary>
    /// �÷��̾ ���� ����(WeaponController)���� �����ϴ� Ŭ����
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        //���� ���� - ������ �����Ҷ� ó�� �������� ���޵Ǵ� ���� ����Ʈ(�κ��丮)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        //���� ����
        //���⸦ �����ϴ� ������Ʈ
        public Transform weaponParentSocket;

        //�÷��̾ �����߿� ��� �ٴϴ� ���� ����Ʈ
        private WeaponController[] weaponSlots = new WeaponController[9];
        //���� ����Ʈ(����)�� Ȱ��ȭ�� ���⸦ �����ϴ� �ε���
        public int ActiveWeaponIndex { get; private set; }

        //���� ��ü
        public UnityAction<WeaponController> OnSwitchToWeapon;  //���� ��ü�Ҷ����� ��ϵ� �Լ� ȣ��

        private WeaponSwithState weaponSwithState;          //���� ��ü�� ����

        private PlayerInputHandler playerInputHandler;

        //���� ��ü�� ���Ǵ� ���� ��ġ
        private Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPostion;
        public Transform downWeaponPostion;
        public Transform aimingWeaponPosition;

        private int weaponSwitchNewIndex;           //���� �ٲ�� ���� �ε���

        private float weaponSwitchTimeStarted = 0f;
        [SerializeField] private float weaponSwitchDelay = 1f;

        //�� ����
        public bool IsPointingAtEnemy { get; private set; }        //�� ���� ����
        public Camera weaponCamera;                                //weaponCamera���� Ray�� �� Ȯ��

        //����
        //ī�޶� ����
        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;          //ī�޶� �⺻ FOV ��
        [SerializeField] private float weaponFovMutiplier = 1f;        //FOV ���� ���

        public bool IsAiming { get; private set; }                      //���� ���� ����
        [SerializeField] private float aimingAnimationSpeed = 10f;      //���� �̵�, Fov ���� Lerp�ӵ�

        //��鸲
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaultBobAmount = 0.05f;         //���� ��鸲 ��
        [SerializeField] private float aimingBobAmount = 0.02f;          //������ ��鸲 ��

        private float weaponBobFactor;          //��鸲 ���
        private Vector3 lastCharacterPosition;  //���� �����ӿ����� �̵��ӵ��� ���ϱ� ���� ����

        private Vector3 weaponBobLocalPosition; //��鸰 �� ���� ��갪, �̵����� ������ 0
        #endregion

        private void Start()
        {
            //����
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            //�ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            //��Ƽ�� ���� show �Լ� ���
            OnSwitchToWeapon += OnWeaponSwitched;

            //Fov �ʱⰪ ����
            SetFov(defaultFov);

            //���� ���� ���� ����
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }

        private void Update()
        {
            //���� ��Ƽ�� ����
            WeaponController activeWeapon = GetActiveWeapon();

            //���� �Է°� ó��
            IsAiming = playerInputHandler.GetAimInputHeld();

            if(!IsAiming && (weaponSwithState == WeaponSwithState.Up || weaponSwithState == WeaponSwithState.Down))
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
            }

            //�� ����
            IsPointingAtEnemy = false;
            if (activeWeapon)
            {
                RaycastHit hit;
                if(Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out hit, 300f))
                {
                    //�ݶ��̴� üũ - ��(Health)
                    Health health = hit.collider.GetComponent<Health>();
                    if (health != null)
                    {
                        IsPointingAtEnemy = true;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeaponAiming();
            UpdateWeaponSwitching();

            //���� ���� ��ġ
            weaponParentSocket.localPosition = weaponMainLocalPosition + weaponBobLocalPosition;
        }

        //ī�޶� Fov �� ����: ����, �ܾƿ�
        private void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMutiplier;
        }

        //���� ���ؿ� ���� ����: ������ġ ����, Fov�� ����
        void UpdateWeaponAiming()
        {
            //���⸦ ��� �������� ���� ����
            if (weaponSwithState == WeaponSwithState.Up)
            {
                WeaponController activeWeapon = GetActiveWeapon();

                if (IsAiming && activeWeapon)    //���ؽ�: ����Ʈ -> Aiming ��ġ�� �̵�, fov: ����Ʈ -> aimZoomRatio
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        aimingWeaponPosition.localPosition + activeWeapon.aimOffset,
                        aimingAnimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        activeWeapon.aimZoomRatio * defaultFov, aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);
                }
                else            //������ Ǯ������: Aiming ��ġ -> ����Ʈ ��ġ�� �̵� fov: aimZoomRatio -> default
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        defaultWeaponPostion.localPosition,
                        aimingAnimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                        defaultFov, aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);
                }
            }
        }

        //�̵��� ���� ���� ��鸰 �� ���ϱ�
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0)
            {
                //�÷��̾ �� �����ӵ��� �̵��� �Ÿ�
                //playerCharacterController.transform.position - lastCharacterPosition
                //���� �����ӿ��� �÷��̾� �̵� �ӵ�
                Vector3 playerCharacterVelocity =
                    (playerCharacterController.transform.position - lastCharacterPosition)/Time.deltaTime;

                float characterMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)
                {
                    characterMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude /
                        (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                //�ӵ��� ���� ��鸲 ���
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, characterMovementFactor, bobSharpness * Time.deltaTime);

                //��鸲��(���ؽ�, ����)
                float bobAmount = IsAiming ? aimingBobAmount : defaultBobAmount;
                float frequency = bobFrequency;
                //�¿� ��鸲
                float vBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                //���Ʒ� ��鸲 (�¿� ��鸲�� ����)
                float hBobValue = ((Mathf.Sin((Time.time * frequency) * 0.5f) + 0.5f) * bobAmount * weaponBobFactor);

                //
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = Mathf.Abs(vBobValue);


                //�÷��̾��� ���� �������� ������ ��ġ�� ����
                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }

        //���¿� ���� ���� ����
        void UpdateWeaponSwitching()
        {
            //Lerp ����
            float switchingTimeFactor = 0f;
            if (weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            //�����ð����� ���� ���� �ٲٱ�
            if (switchingTimeFactor >= 1f)
            {
                if (weaponSwithState == WeaponSwithState.PutDownPrvious)
                {
                    //���繫�� false, ���ο� ���� true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if (oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }

                    ActiveWeaponIndex = weaponSwitchNewIndex;
                    WeaponController newWeapon = GetActiveWeapon();
                    OnSwitchToWeapon?.Invoke(newWeapon);

                    switchingTimeFactor = 0f;
                    if (newWeapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;
                        weaponSwithState = WeaponSwithState.PutUpNew;
                    }
                    else
                    {
                        weaponSwithState = WeaponSwithState.Down;
                    }
                }
                else if (weaponSwithState == WeaponSwithState.PutUpNew)
                {
                    weaponSwithState = WeaponSwithState.Up;
                }
            }

            //�����ð����� ������ ��ġ �̵�
            if (weaponSwithState == WeaponSwithState.PutDownPrvious)
            {
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPostion.localPosition, downWeaponPostion.localPosition, switchingTimeFactor);
            }
            else if (weaponSwithState == WeaponSwithState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(downWeaponPostion.localPosition, defaultWeaponPostion.localPosition, switchingTimeFactor);
            }
        }




        //weaponSlots�� ���� ���������� ������ WeaponController ������Ʈ �߰�
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            //�߰��ϴ� ���� ���� ���� üũ - �ߺ��˻�
            if (HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("Has Same Weapon");
                return false;
            }

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                    weaponInstance.transform.localPosition = Vector3.zero;
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = this.gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
            }

            Debug.Log("weaponSlots full");
            return false;
        }

        //�Ű������� ���� 
        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab)
                {
                    return weaponSlots[i];
                }
            }

            return null;
        }

        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        //������ ���Կ� ���Ⱑ �ִ��� ����
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if (index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }

            return null;
        }

        //0~9  
        //���� �ٲٱ�, ���� ��� �ִ� ���� false, ���ο� ���� true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;    //���� ��Ƽ���� ���� �ε���
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);
                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            //���� ��Ƽ���� ���� �ε����� ���� ��ü
            SwitchToWeaponIndex(newWeaponIndex);
        }

        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            //newWeaponIndex �� üũ
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;
                weaponSwitchTimeStarted = Time.time;

                //���� ��Ƽ���� ���Ⱑ �ִ���?
                if (GetActiveWeapon() == null)
                {
                    weaponMainLocalPosition = downWeaponPostion.position;
                    weaponSwithState = WeaponSwithState.PutUpNew;
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(weaponController);
                }
                else
                {
                    weaponSwithState = WeaponSwithState.PutDownPrvious;
                }
            }
        }

        //���԰� �Ÿ�
        private int GetDistanceBetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;

            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = fromSlotIndex - toSlotIndex;
            }

            if (distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = distanceBetweenSlots + weaponSlots.Length;
            }

            return distanceBetweenSlots;
        }

        void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }
    }
}