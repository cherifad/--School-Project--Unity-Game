/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Demo.Objects;
using Opsive.UltimateCharacterController.Demo.UI;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// The DemoManager will control the objects in the demo scene as well as the text shown.
    /// </summary>
    public class DemoManager : MonoBehaviour
    {
        /// <summary>
        /// Container for each zone within the demo scene.
        /// </summary>
        [System.Serializable]
        public class DemoZone
        {
            [Tooltip("The header text.")]
            [SerializeField] protected string m_Header;
            [Tooltip("The description text.")]
            [SerializeField] protected string m_Description;
            [Tooltip("The text that appears beneath the header requiring action.")]
            [SerializeField] protected string m_Action;
            [Tooltip("The location the character should teleport to when moving to this zone.")]
            [SerializeField] protected Transform m_TeleportLocation;
            [Tooltip("The trigger that enables the header and description text.")]
            [SerializeField] protected DemoZoneTrigger m_DemoZoneTrigger;
            [Tooltip("The objects that the trigger should enable.")]
            [SerializeField] protected MonoBehaviour[] m_EnableObjects;
            [Tooltip("The objects that the trigger should activate/deactivate.")]
            [SerializeField] protected GameObject[] m_ToggleObjects;

            public string Header { get { return m_Header; } }
            public string Description { get { return m_Description; } }
            public string Action { get { return m_Action; } }
            public Transform TeleportLocation { get { return m_TeleportLocation; } }
            public DemoZoneTrigger DemoZoneTrigger { get { return m_DemoZoneTrigger; } }
            public MonoBehaviour[] EnableObjects { get { return m_EnableObjects; } }
            public GameObject[] ToggleObjects { get { return m_ToggleObjects; } }

            private int m_Index;
            private SpawnPoint m_SpawnPoint;
            public int Index { get { return m_Index; } }
            public SpawnPoint SpawnPoint { get { return m_SpawnPoint; } }

            /// <summary>
            /// Initializes the zone.
            /// </summary>
            /// <param name="index">The index of the DemoZone.</param>
            public void Initialize(int index)
            {
                m_Index = index;

                // Assign the spawn point so the character will know where to spawn upon death.
                m_SpawnPoint = m_TeleportLocation.GetComponent<SpawnPoint>();
                m_SpawnPoint.Grouping = index;

                // The toggled objects should start disabled.
                for (int i = 0; i < m_ToggleObjects.Length; ++i) {
                    m_ToggleObjects[i].SetActive(false);
                }
            }
        }

        [Tooltip("A reference to the character.")]
        [SerializeField] protected GameObject m_Character;
        [Tooltip("Is the character allowed to free roam the scene at the very start?")]
        [SerializeField] protected bool m_FreeRoam;
        [Tooltip("A reference used to determine the character's perspective selection at the start.")]
        [SerializeField] protected GameObject m_PerspectiveSelection;
        [Tooltip("A reference to the panel which shows the demo text.")]
        [SerializeField] protected GameObject m_TextPanel;
        [Tooltip("A reference to the Text component which shows the demo header text.")]
        [SerializeField] protected Text m_Header;
        [Tooltip("A reference to the Text component which shows the demo description text.")]
        [SerializeField] protected Text m_Description;
        [Tooltip("A reference to the Text component which shows the demo action text.")]
        [SerializeField] protected Text m_Action;
        [Tooltip("A reference to the GameObject which shows the next zone arrow.")]
        [SerializeField] protected GameObject m_NextZoneArrow;
        [Tooltip("A reference to the GameObject which shows the previous zone arrow.")]
        [SerializeField] protected GameObject m_PreviousZoneArrow;
        [Tooltip("A list of all of the zones within the scene.")]
        [SerializeField] protected DemoZone[] m_DemoZones;
        [Tooltip("Should the ItemTypes be picked up when the character spawns within free roam mode?")]
        [SerializeField] protected bool m_FreeRoamPickupItemTypes = true;
        [Tooltip("An array of ItemTypes to be picked up when free roaming.")]
        [SerializeField] protected ItemTypeCount[] m_FreeRoamItemTypeCounts;

        public GameObject Character { get { return m_Character; } }
        public bool FreeRoam { get { return m_FreeRoam; } set { m_FreeRoam = value; } }
        public GameObject PerspectiveSelection { get { return m_PerspectiveSelection; } set { m_PerspectiveSelection = value; } }
        public DemoZone[] DemoZones { get { return m_DemoZones; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Health m_CharacterHealth;
        private Respawner m_CharacterRespawner;
        private Dictionary<DemoZoneTrigger, DemoZone> m_DemoZoneTriggerDemoZoneMap = new Dictionary<DemoZoneTrigger, DemoZone>();
        private int m_ActiveZoneIndex;
        private List<Door> m_Doors = new List<Door>();
        private int m_EnterFrame;
        private bool m_FullAccess;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
            var demoZones = new List<DemoZone>(m_DemoZones);
            for (int i = demoZones.Count - 1; i > -1; --i) {
                // The demo zone may belong to the other perspective.
                if (demoZones[i].DemoZoneTrigger == null) {
                    demoZones.RemoveAt(i);
                }
            }
            m_DemoZones = demoZones.ToArray();
#endif
            for (int i = 0; i < m_DemoZones.Length; ++i) {
                if (m_DemoZones[i].TeleportLocation == null) {
                    continue;
                }

                m_DemoZones[i].Initialize(i);
                m_DemoZoneTriggerDemoZoneMap.Add(m_DemoZones[i].DemoZoneTrigger, m_DemoZones[i]);
            }
            
            // Enable the UI after the character has spawned.
            m_TextPanel.SetActive(false);
            m_PreviousZoneArrow.SetActive(false);
            m_NextZoneArrow.SetActive(false);
            m_Action.enabled = false;
            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            m_CharacterHealth = m_Character.GetComponent<Health>();
            m_CharacterRespawner = m_Character.GetComponent<Respawner>();

            // Disable the demo components if the character is null. This allows for free roaming within the demo scene.
            if (m_FreeRoam) {
                m_FullAccess = true;
                if (m_PerspectiveSelection != null) {
                    m_PerspectiveSelection.SetActive(false);
                }

                var uiZones = GetComponentsInChildren<UIZone>();
                for (int i = 0; i < uiZones.Length; ++i) {
                    uiZones[i].enabled = false;
                }

                // All of the doors should be opened with free roam.
                for (int i = 0; i < m_Doors.Count; ++i) {
                    m_Doors[i].CloseOnTriggerExit = false;
                    m_Doors[i].OpenClose(true, true, false);
                }

                // The enable objects should be enabled.
                for (int i = 0; i < m_DemoZones.Length; ++i) {
                    for (int j = 0; j < m_DemoZones[i].EnableObjects.Length; ++j) {
                        m_DemoZones[i].EnableObjects[j].enabled = true;
                    }
                }

                // The character needs to be assigned to the camera.
                var camera = UnityEngineUtility.FindCamera(null);
                var cameraController = camera.GetComponent<Camera.CameraController>();
                cameraController.Character = m_Character;

                // The character doesn't start out with any items.
                if (m_FreeRoamItemTypeCounts != null && m_FreeRoamPickupItemTypes) {
                    var inventory = m_Character.GetComponent<InventoryBase>();
                    if (inventory != null) {
                        for (int i = 0; i < m_FreeRoamItemTypeCounts.Length; ++i) {
                            inventory.PickupItemType(m_FreeRoamItemTypeCounts[i].ItemType, m_FreeRoamItemTypeCounts[i].Count, -1, true, false);
                        }
                    }
                }

                EventHandler.ExecuteEvent(m_Character, "OnCharacterSnapAnimator");
                enabled = false;
                return;
            }

            // The cursor needs to be visible.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            // Show the perspective selection menu.
            if (m_PerspectiveSelection != null) {
                // The character should be disabled until the perspective is set.
                m_CharacterLocomotion.SetActive(false, true);

                m_PerspectiveSelection.SetActive(true);
            } else {
                // Determine if the character is a first or third person character.
                var firstPersonPerspective = m_CharacterLocomotion.MovementTypeFullName.Contains("FirstPerson");
                SelectStartingPerspective(firstPersonPerspective);
            }
#elif FIRST_PERSON_CONTROLLER
            SelectStartingPerspective(true);
#else
            SelectStartingPerspective(false);
#endif
        }

        /// <summary>
        /// Keep the mouse visible when the perspective screen is active.
        /// </summary>
        private void Update()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Registers the door with the DemoManager.
        /// </summary>
        /// <param name="door">The door that should be registered.</param>
        public void RegisterDoor(Door door)
        {
            m_Doors.Add(door);
        }

        /// <summary>
        /// The character has entered a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character entered.</param>
        public void EnteredTriggerZone(DemoZoneTrigger demoZoneTrigger)
        {
            DemoZone demoZone;
            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out demoZone)) {
                return;
            }

            if (m_CharacterHealth.Value == 0) {
                return;
            }

            ActiveDemoZone(demoZone, false);
        }

        /// <summary>
        /// Activates the specified demo zone.
        /// </summary>
        /// <param name="demoZone">The demo zone to active.</param>
        /// <param name="teleport">Should the character be teleported to the demo zone?</param>
        private void ActiveDemoZone(DemoZone demoZone, bool teleport)
        {
            if (demoZone.Index == m_ActiveZoneIndex) {
                return;
            }

            // The ride ability should be force stopped.
            var ride = m_CharacterLocomotion.GetAbility<Character.Abilities.Ride>();
            if (ride != null && ride.IsActive) {
                m_CharacterLocomotion.TryStopAbility(ride, true);
            }

            // Only one zone can be active at a time.
            if (m_ActiveZoneIndex != -1) {
                ExitedTriggerZone(m_DemoZones[m_ActiveZoneIndex].DemoZoneTrigger);
            }

            m_ActiveZoneIndex = demoZone.Index;
            ShowText(demoZone.Header, demoZone.Description, demoZone.Action);
            m_PreviousZoneArrow.SetActive(m_ActiveZoneIndex != 0);
            m_NextZoneArrow.SetActive(m_ActiveZoneIndex != m_DemoZones.Length - 1);
            m_EnterFrame = Time.frameCount;
            for (int i = 0; i < demoZone.EnableObjects.Length; ++i) {
                demoZone.EnableObjects[i].enabled = true;
            }
            for (int i = 0; i < demoZone.ToggleObjects.Length; ++i) {
                demoZone.ToggleObjects[i].SetActive(true);
            }

            // When the character reaches the outside section all doors should be unlocked.
            if (!m_FullAccess && m_ActiveZoneIndex >= m_DemoZones.Length - 5) {
                for (int i = 0; i < m_Doors.Count; ++i) {
                    m_Doors[i].CloseOnTriggerExit = false;
                    m_Doors[i].OpenClose(true, true, false);
                }
                m_FullAccess = true;
            }

            if (teleport) {
                m_CharacterLocomotion.SetPositionAndRotation(demoZone.TeleportLocation.position, demoZone.TeleportLocation.rotation, true);
            }

            // Set the group after the state so the default state doesn't override the grouping value.
            m_CharacterRespawner.Grouping = demoZone.SpawnPoint.Grouping;
        }

        /// <summary>
        /// The character has exited a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character exited.</param>
        public void ExitedTriggerZone(DemoZoneTrigger demoZoneTrigger)
        {
            DemoZone demoZone;
            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out demoZone)) {
                return;
            }
            for (int i = 0; i < demoZone.ToggleObjects.Length; ++i) {
                demoZone.ToggleObjects[i].SetActive(false);
            }
            // Show standard text if the demo zone isn't the last demo zone.
            if (m_ActiveZoneIndex == demoZone.Index && demoZone.Index != m_DemoZones.Length - 1 && m_EnterFrame != Time.frameCount) {
                ShowText(AssetInfo.Name + " Demo", "\nUse the arrows to teleport to different zones.\n\nPress Escape to show the cursor.", string.Empty);
                m_ActiveZoneIndex = -1;
            }
        }

        /// <summary>
        /// Teleports the character to the next or pervious zone.
        /// </summary>
        /// <param name="next">Should the character be teleported to the next zone? If false the previous zone will be used.</param>
        public void Teleport(bool next)
        {
            var targetIndex = Mathf.Clamp(m_ActiveZoneIndex + (next ? 1 : -1), 0, m_DemoZones.Length - 1);
            if (targetIndex == m_ActiveZoneIndex) {
                return;
            }

            ActiveDemoZone(m_DemoZones[targetIndex], true);
        }

        /// <summary>
        /// Sets the starting perspective on the character.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the character start in a first person perspective?</param>
        public void SelectStartingPerspective(bool firstPersonPerspective)
        {
            m_CharacterLocomotion.SetActive(true, true);

            // Set the perspective on the camera.
            var camera = UnityEngineUtility.FindCamera(null);
            var cameraController = camera.GetComponent<Camera.CameraController>();
            // Ensure the camera starts with the correct view type.
            cameraController.FirstPersonViewTypeFullName = "Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes.Combat";
            cameraController.ThirdPersonViewTypeFullName = "Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes.Adventure";
            cameraController.Character = m_Character;
            cameraController.SetPerspective(firstPersonPerspective, true);

            // Set the starting position.
            m_ActiveZoneIndex = -1;
            ActiveDemoZone(m_DemoZones[0], true);

            // The cursor should be hidden to start the demo.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            enabled = false;

            // The character and camera are ready to go - disable the perspective selection panel.
            if (m_PerspectiveSelection != null) {
                m_PerspectiveSelection.SetActive(false);
            }
        }

        /// <summary>
        /// Shows the text in the UI with the specified header and description.
        /// </summary>
        /// <param name="header">The header that should be shown.</param>
        /// <param name="description">The description that should be shown.</param>
        /// <param name="action">The action that should be shown.</param>
        private void ShowText(string header, string description, string action)
        {
            if (string.IsNullOrEmpty(header)) {
                m_TextPanel.SetActive(false);
                return;
            }

            m_TextPanel.SetActive(true);
            m_Header.text = "--- " + header + " ---";
            m_Description.text = description.Replace("{AssetName}", AssetInfo.Name);
            m_Action.text = action;
            m_Action.enabled = !string.IsNullOrEmpty(action);
        }
    }
}