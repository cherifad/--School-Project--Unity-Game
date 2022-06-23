/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// The DeterministicObjectManager acts as a central organizer for determining when the characters, cameras, and deterministic objects should update. The update order that the objects
    /// are updated matter and the DeterministicObjectManager ensures the objects are updated in the correct order to allow for smooth movement.
    /// </summary>
    public class DeterministicObjectManager : MonoBehaviour
    {
        /// <summary>
        /// A small storage class used for storing the fixed and smooth location. This component will also move the interpolate the objects during the Update loop.
        /// </summary>
        private class SmoothFixedLocation
        {
            private Transform m_Transform;

            private Vector3 m_FixedPosition;
            private Quaternion m_FixedRotation;
            private Vector3 m_SmoothPosition;
            private Quaternion m_SmoothRotation;

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="transform">The transform that is being managed by the DeterministicObjectManager.</param>
            protected void Initialize(Transform transform)
            {
                m_Transform = transform;

                m_FixedPosition = m_SmoothPosition = m_Transform.position;
                m_FixedRotation = m_SmoothRotation = m_Transform.rotation;
            }

            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position.</param>
            public virtual void SmoothMove(float interpAmount)
            {
                m_Transform.position = Vector3.Lerp(m_SmoothPosition, m_FixedPosition, interpAmount);
                m_Transform.rotation = Quaternion.Slerp(m_SmoothRotation, m_FixedRotation, interpAmount);
            }

            /// <summary>
            /// Restores the location back to the fixed location. This will be performed immediately before the object is moved within FixedUpdate.
            /// </summary>
            public void RestoreFixedPosition()
            {
                m_Transform.position = m_SmoothPosition = m_FixedPosition;
                m_Transform.rotation = m_SmoothRotation = m_FixedRotation;
            }

            /// <summary>
            /// Assigns the fixed location. This will be performed immediately after the object is moved within FixedUpdate.
            /// </summary>
            public void AssignFixedPosition()
            {
                m_FixedPosition = m_Transform.position;
                m_FixedRotation = m_Transform.rotation;
            }

            /// <summary>
            /// Immediately set the object's position.
            /// </summary>
            /// <param name="position">The position of the object.</param>
            public void SetPosition(Vector3 position)
            {
                m_Transform.position = m_FixedPosition = m_SmoothPosition = position;
            }

            /// <summary>
            /// Immediately set the object's rotation.
            /// </summary>
            /// <param name="position">The rotation of the object.</param>
            public void SetRotation(Quaternion rotation)
            {
                m_Transform.rotation = m_FixedRotation = m_SmoothRotation = rotation;
            }
        }

        /// <summary>
        /// Extends the SmoothFixedLocation class for characters.
        /// </summary>
        private class DeterministicCharacter : SmoothFixedLocation
        {
            private UltimateCharacterLocomotion m_CharacterLocomotion;
            private UltimateCharacterLocomotionHandler m_CharacterHandler;
            private CharacterIKBase m_CharacterIK;
            private float m_HorizontalMovement;
            private float m_ForwardMovement;
            private float m_DeltaYawRotation;

            public UltimateCharacterLocomotion CharacterLocomotion { get { return m_CharacterLocomotion; } }
            public float HorizontalMovement { set { m_HorizontalMovement = value; } }
            public float ForwardMovement { set { m_ForwardMovement = value; } }
            public float DeltaYawRotation { set { m_DeltaYawRotation = value; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="transform">The character that is being managed by the DeterministicObjectManager.</param>
            public void Initialize(UltimateCharacterLocomotion characterLocomotion)
            {
                m_CharacterLocomotion = characterLocomotion;
                m_CharacterHandler = m_CharacterLocomotion.GetComponent<UltimateCharacterLocomotionHandler>();
                m_CharacterIK = m_CharacterLocomotion.GetComponent<CharacterIKBase>();

                // The class is pooled so reset any variables.
                m_HorizontalMovement = m_ForwardMovement = m_DeltaYawRotation = 0;
                Initialize(characterLocomotion.transform);
            }

            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position.</param>
            public override void SmoothMove(float interpAmount)
            {
                base.SmoothMove(interpAmount);

                if (m_CharacterIK != null && m_CharacterIK.enabled) {
                    m_CharacterIK.SmoothMove();
                }
            }

            /// <summary>
            /// Moves the character according to the input variables.
            /// </summary>
            public void FixedMove()
            {
                if (m_CharacterHandler != null) {
                    m_DeltaYawRotation = m_CharacterHandler.GetDeltaYawRotation();
                }
                m_CharacterLocomotion.Move(m_HorizontalMovement, m_ForwardMovement, m_DeltaYawRotation);
            }
        }

        /// <summary>
        /// Extends the SmoothFixedLocation class for deterministic objects.
        /// </summary>
        private class DeterministicObject : SmoothFixedLocation
        {
            private IDeterministicObject m_DeterministicObject;

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="deterministicObject">The deterministic object that is being managed by the DeterministicObjectManager.</param>
            public void Initialize(IDeterministicObject deterministicObject)
            {
                m_DeterministicObject = deterministicObject;
                Initialize(m_DeterministicObject.transform);
            }

            /// <summary>
            /// Moves the deterministic object.
            /// </summary>
            public void FixedMove()
            {
                m_DeterministicObject.Move();
            }
        }

        /// <summary>
        /// Moves and rotates the camera.
        /// </summary>
        private class DeterministicCamera : SmoothFixedLocation
        {
            private CameraController m_CameraController;
            private Vector2 m_LookVector;

            public CameraController CameraController { get { return m_CameraController; } }
            public Vector2 LookVector { set { m_LookVector = value; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="cameraController">The camera controller that is being managed by the DeterministicObjectManager.</param>
            public void Initialize(CameraController cameraController)
            {
                Initialize(cameraController.transform);

                m_CameraController = cameraController;
            }

            /// <summary>
            /// Rotates the camera.
            /// </summary>
            public void Rotate()
            {
                m_CameraController.Rotate(m_LookVector.x, m_LookVector.y);
            }

            /// <summary>
            /// Calls the Move method of the CameraController.
            /// </summary>
            public void Move()
            {
                m_CameraController.Move(m_LookVector.x, m_LookVector.y);
            }
        }

        private static DeterministicObjectManager s_Instance;
        private static DeterministicObjectManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Deterministic Object Manager").AddComponent<DeterministicObjectManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("The number of starting characters. For best performance this value should be the maximum number of characters that are active within the scene.")]
        [SerializeField] protected int m_StartCharacterCount = 1;
        [Tooltip("The number of starting cameras. For best performance this value should be the maximum number of cameras that are active within the scene.")]
        [SerializeField] protected int m_StartCameraCount = 1;
        [Tooltip("The number of starting deterministic objects. For best performance this value should be the maximum number of deterministic objects that are active within the scene.")]
        [SerializeField] protected int m_StartDeterministicObjectCount;
#if UNITY_2017_2_OR_NEWER
        [Tooltip("Should the Auto Sync Transforms be enabled? See this page for more info: https://docs.unity3d.com/ScriptReference/Physics-autoSyncTransforms.html.")]
        [SerializeField] protected bool m_AutoSyncTransforms;
#endif

        private DeterministicCharacter[] m_Characters;
        private DeterministicCamera[] m_Cameras;
        private DeterministicObject[] m_DeterministicObjects;
        private int m_CharacterCount;
        private int m_CameraCount;
        private int m_DeterministicObjectCount;

        private float m_FixedTime;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Characters = new DeterministicCharacter[m_StartCharacterCount];
            m_Cameras = new DeterministicCamera[m_StartCameraCount];
            m_DeterministicObjects = new DeterministicObject[m_StartDeterministicObjectCount];

#if UNITY_2017_2_OR_NEWER
            Physics.autoSyncTransforms = false;
#endif
        }

        /// <summary>
        /// Registers the character to be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="characterLocomotion">The character that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the character registered.</returns>
        public static int RegisterCharacter(UltimateCharacterLocomotion characterLocomotion)
        {
            return Instance.RegisterCharacterInternal(characterLocomotion);
        }

        /// <summary>
        /// Internal method which registers the character to be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="characterLocomotion">The character that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the character registered.</returns>
        private int RegisterCharacterInternal(UltimateCharacterLocomotion characterLocomotion)
        {
            if (m_CharacterCount == m_Characters.Length) {
                System.Array.Resize(ref m_Characters, m_Characters.Length + 1);
                Debug.LogWarning("Characters array resized. For best performance increase the size of the Start Character Count variable " +
                                 "within the Deterministic Object Manager to a value of at least " + (m_CharacterCount + 1));
            }
            m_Characters[m_CharacterCount] = ObjectPool.Get<DeterministicCharacter>();
            m_Characters[m_CharacterCount].Initialize(characterLocomotion);
            m_CharacterCount++;
            return m_CharacterCount - 1;
        }

        /// <summary>
        /// Registers the camera to be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="cameraController">The camera that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the camera registered.</returns>
        public static int RegisterCamera(CameraController cameraController)
        {
            return Instance.RegisterCameraInternal(cameraController);
        }

        /// <summary>
        /// Intenral method which registers the camera to be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="cameraController">The camera that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the camera registered.</returns>
        private int RegisterCameraInternal(CameraController cameraController)
        {
            if (m_CameraCount == m_Cameras.Length) {
                System.Array.Resize(ref m_Cameras, m_Cameras.Length + 1);
                Debug.LogWarning("Cameras array resized. For best performance increase the size of the Start Camera Count variable " +
                                 "within the Deterministic Object Manager to a value of at least " + (m_CameraCount + 1));
            }
            m_Cameras[m_CameraCount] = ObjectPool.Get<DeterministicCamera>();
            m_Cameras[m_CameraCount].Initialize(cameraController);
            m_CameraCount++;
            return m_CameraCount - 1;
        }

        /// <summary>
        /// Registers the deterministic object that should be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="deterministicObject">The deterministic object that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the kinematci object registered.</returns>
        public static int RegisterObject(IDeterministicObject deterministicObject)
        {
            return Instance.RegisterObjectInternal(deterministicObject);
        }

        /// <summary>
        /// Internal method which registers the deterministic object that should be managed by the DeterministicObjectManager.
        /// </summary>
        /// <param name="deterministicObject">The deterministic object that should be managed by the DeterministicObjectManager.</param>
        /// <returns>The index of the kinematci object registered.</returns>
        private int RegisterObjectInternal(IDeterministicObject deterministicObject)
        {
            if (m_DeterministicObjectCount == m_DeterministicObjects.Length) {
                System.Array.Resize(ref m_DeterministicObjects, m_DeterministicObjects.Length + 1);
                Debug.LogWarning("Deterministic objects array resized. For best performance increase the size of the Start Deterministic Object Count variable " +
                                 "within the Deterministic Object Manager to a value of at least " + (m_DeterministicObjectCount + 1));
            }
            m_DeterministicObjects[m_DeterministicObjectCount] = ObjectPool.Get<DeterministicObject>();
            m_DeterministicObjects[m_DeterministicObjectCount].Initialize(deterministicObject);
            m_DeterministicObjectCount++;
            return m_DeterministicObjectCount - 1;
        }

        /// <summary>
        /// Sets the yaw rotation of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="yawRotation">The yaw rotation that the character should rotate towards.</param>
        public static void SetCharacterDeltaYawRotation(int characterIndex, float yawRotation)
        {
            Instance.SetCharacterDeltaYawRotationInternal(characterIndex, yawRotation);
        }

        /// <summary>
        /// Internal method which sets the yaw rotation of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="yawRotation">The yaw rotation that the character should rotate towards.</param>
        private void SetCharacterDeltaYawRotationInternal(int characterIndex, float yawRotation)
        {
            m_Characters[characterIndex].DeltaYawRotation = yawRotation;
        }

        /// <summary>
        /// Sets the horizontal and forward input values of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        public static void SetCharacterMovementInput(int characterIndex, float horizontalMovement, float forwardMovement)
        {
            Instance.SetCharacterMovementInputInternal(characterIndex, horizontalMovement, forwardMovement);
        }

        /// <summary>
        /// Internal method which sets the horizontal and forward input values of the character.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        private void SetCharacterMovementInputInternal(int characterIndex, float horizontalMovement, float forwardMovement)
        {
            m_Characters[characterIndex].HorizontalMovement = horizontalMovement;
            m_Characters[characterIndex].ForwardMovement = forwardMovement;
        }

        /// <summary>
        /// Immediately sets the character's position.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCharacterPosition(int characterIndex, Vector3 position)
        {
            Instance.SetCharacterPositionInternal(characterIndex, position);
        }

        /// <summary>
        /// Internal methodh which immediately sets the character's position.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCharacterPositionInternal(int characterIndex, Vector3 position)
        {
            m_Characters[characterIndex].SetPosition(position);
        }

        /// <summary>
        /// Immediately sets the character's rotation.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCharacterRotation(int characterIndex, Quaternion rotation)
        {
            Instance.SetCharacterRotationInternal(characterIndex, rotation);
        }

        /// <summary>
        /// Internal methodh which immediately sets the character's rotation.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCharacterRotationInternal(int characterIndex, Quaternion rotation)
        {
            m_Characters[characterIndex].SetRotation(rotation);
        }

        /// <summary>
        /// Ensures the character specified by the character index is updated before the character specified by the other character index.
        /// </summary>
        /// <param name="characterIndex">The original character which should have a higher priority.</param>
        /// <param name="otherCharacterIndex">The character which may have a higher priority compared to the original character</param>
        public static void SetCharacterPriorityUpdateOrder(int characterIndex, int otherCharacterIndex)
        {
            // Don't update the index mid-loop - use the scheduler to update the index before the next time the characters are updated.
            Scheduler.ScheduleFixed(Time.fixedDeltaTime - 0.01f, Instance.SetCharacterPriorityOrderInternal, characterIndex, otherCharacterIndex);
        }

        /// <summary>
        /// Internal method which ensures the character specified by the character index is updated before the character specified by the other character index.
        /// </summary>
        /// <param name="characterIndex">The original character which should have a higher priority.</param>
        /// <param name="otherCharacterIndex">The character which may have a higher priority compared to the original character</param>
        public void SetCharacterPriorityOrderInternal(int characterIndex, int otherCharacterIndex)
        {
            // If the character index is already lower then the other character index then the character already has update priority.
            if (characterIndex < otherCharacterIndex) {
                return;
            }

            // A swap is necessary.
            var otherCharacter = m_Characters[otherCharacterIndex];
            m_Characters[otherCharacterIndex] = m_Characters[characterIndex];
            m_Characters[characterIndex] = otherCharacter;

            m_Characters[characterIndex].CharacterLocomotion.DeterministicObjectIndex = characterIndex;
            m_Characters[otherCharacterIndex].CharacterLocomotion.DeterministicObjectIndex = otherCharacterIndex;
        }

        /// <summary>
        /// Sets the look vector of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="lookVector">The look vector of the camera.</param>
        public static void SetCameraLookVector(int cameraIndex, Vector2 lookVector)
        {
            Instance.SetCameraLookVectorInternal(cameraIndex, lookVector);
        }

        /// <summary>
        /// Internal method which sets the look vector of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="lookVector">The look vector of the camera.</param>
        private void SetCameraLookVectorInternal(int cameraIndex, Vector2 lookVector)
        {
            m_Cameras[cameraIndex].LookVector = lookVector;
        }

        /// <summary>
        /// Sets the position of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="position">The position of the camera.</param>
        public static void SetCameraPosition(int cameraIndex, Vector3 position)
        {
            Instance.SetCameraPositionInternal(cameraIndex, position);
        }

        /// <summary>
        /// Internal method which sets the position of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="position">The position of the camera.</param>
        private void SetCameraPositionInternal(int cameraIndex, Vector3 position)
        {
            m_Cameras[cameraIndex].SetPosition(position);
        }

        /// <summary>
        /// Sets the rotation of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="rotation">The rotation of the camera.</param>
        public static void SetCameraRotation(int cameraIndex, Quaternion rotation)
        {
            Instance.SetCameraRotationInternal(cameraIndex, rotation);
        }

        /// <summary>
        /// Internal method which sets the rotation of the camera.
        /// </summary>
        /// <param name="cameraIndex">The index of the camera within the cameras array.</param>
        /// <param name="rotation">The rotation of the camera.</param>
        private void SetCameraRotationInternal(int cameraIndex, Quaternion rotation)
        {
            m_Cameras[cameraIndex].SetRotation(rotation);
        }

        /// <summary>
        /// Smoothly moves the objects.
        /// </summary>
        private void Update()
        {
            // If the times are equal then the fixed framerate is ticked at the same rate as the variable framerate. The most recent fixed position should
            // be used in this case.
            var interpAmount = (Time.time == m_FixedTime) ? 1 : (Time.time - m_FixedTime) / Time.fixedDeltaTime;
            for (int i = 0; i < m_DeterministicObjectCount; ++i) {
                m_DeterministicObjects[i].SmoothMove(interpAmount);
            }
            for (int i = 0; i < m_CameraCount; ++i) {
                m_Cameras[i].SmoothMove(interpAmount);
            }
            // The character should move after the camera so the IK component will use the most recent values.
            for (int i = 0; i < m_CharacterCount; ++i) {
                m_Characters[i].SmoothMove(interpAmount);
            }
        }

        /// <summary>
        /// Moves all of the deterministic objects.
        /// </summary>
        private void FixedUpdate()
        {
            // Before moving the objects first restore the fixed position of the deterministic object and the character. This will ensure the objects are using the
            // location values of the last FixedUpdate tick.
            for (int i = 0; i < m_DeterministicObjectCount; ++i) {
                m_DeterministicObjects[i].RestoreFixedPosition();
            }
            for (int i = 0; i < m_CharacterCount; ++i) {
                m_Characters[i].RestoreFixedPosition();
            }
            for (int i = 0; i < m_CameraCount; ++i) {
                m_Cameras[i].RestoreFixedPosition();
            }

            // After restoring the location do the actual movement. The location must be restored for both the deterministic objects and characters before either object is
            // moved so the character will using the most up to date locational value for the deterministic object.
            for (int i = 0; i < m_DeterministicObjectCount; ++i) {
                m_DeterministicObjects[i].FixedMove();
            }

#if UNITY_2017_2_OR_NEWER
            // Perform a sync transforms so the character will correctly respond to any deterministic object changes.
            Physics.SyncTransforms();
#endif

            // The camera should be updated immediately after the deterministic objects so if the character is on a platform the camera will correctly move with that platform.
            // The camera only rotates within FixedUpdate so it manages its own fixed rotation.
            for (int i = 0; i < m_CameraCount; ++i) {
                m_Cameras[i].Rotate();
            }
            for (int i = 0; i < m_CharacterCount; ++i) {
                m_Characters[i].FixedMove();
            }
            // After the character has updated the camera should update one more time to account for the new character position.
            for (int i = 0; i < m_CameraCount; ++i) {
                m_Cameras[i].Move();
            }

            // After the move is complete the location should be stored. This location will be used the next time FixedUpdate is called.
            for (int i = 0; i < m_DeterministicObjectCount; ++i) {
                m_DeterministicObjects[i].AssignFixedPosition();
            }
            for (int i = 0; i < m_CharacterCount; ++i) {
                m_Characters[i].AssignFixedPosition();
            }
            for (int i = 0; i < m_CameraCount; ++i) {
                m_Cameras[i].AssignFixedPosition();
            }

            // Remember the time so SmoothMove can determine how much interpolation is necessary.
            m_FixedTime = Time.time;
        }

        /// <summary>
        /// Stops managing the character at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        public static void UnregisterCharacter(int characterIndex)
        {
            Instance.UnregisterCharacterInternal(characterIndex);
        }

        /// <summary>
        /// Internal method which stops managing the character at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the character within the characters array.</param>
        private void UnregisterCharacterInternal(int characterIndex)
        {
            if (characterIndex < 0) {
                return;
            }

            ObjectPool.Return(m_Characters[characterIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = characterIndex + 1; i < m_CharacterCount; ++i) {
                m_Characters[i - 1] = m_Characters[i];
                m_Characters[i - 1].CharacterLocomotion.DeterministicObjectIndex = i - 1;
            }
            m_CharacterCount--;
        }

        /// <summary>
        /// Stops managing the camera at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the camera within the cameras array.</param>
        public static void UnregisterCamera(int cameraIndex)
        {
            Instance.UnregisterCameraInternal(cameraIndex);
        }

        /// <summary>
        /// Internal method which stops managing the camera at the specified index.
        /// </summary>
        /// <param name="characterIndex">The index of the camera within the cameras array.</param>
        private void UnregisterCameraInternal(int cameraIndex)
        {
            if (cameraIndex < 0) {
                return;
            }

            ObjectPool.Return(m_Cameras[cameraIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = cameraIndex + 1; i < m_CameraCount; ++i) {
                m_Cameras[i - 1] = m_Cameras[i];
                m_Cameras[i - 1].CameraController.DeterministicObjectIndex = i - 1;
            }
            m_CameraCount--;
        }

        /// <summary>
        /// Stops managing the deterministic object at the specified index.
        /// </summary>
        /// <param name="deterministicObjectIndex">The index of the deterministic object within the characters array.</param>
        public static void UnregisterDeterministicObject(int deterministicObjectIndex)
        {
            Instance.UnregisterDeterministicObjectInternal(deterministicObjectIndex);
        }

        /// <summary>
        /// Internal method which stops managing the deterministic object at the specified index.
        /// </summary>
        /// <param name="deterministicObjectIndex">The index of the deterministic object within the characters array.</param>
        private void UnregisterDeterministicObjectInternal(int deterministicObjectIndex)
        {
            if (deterministicObjectIndex < 0) {
                return;
            }

            ObjectPool.Return(m_DeterministicObjects[deterministicObjectIndex]);
            // Keep the array packed by shifting all of the subsequent elements over by one.
            for (int i = deterministicObjectIndex + 1; i < m_DeterministicObjectCount; ++i) {
                m_DeterministicObjects[i - 1] = m_DeterministicObjects[i];
            }
            m_DeterministicObjectCount--;
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
    }
}