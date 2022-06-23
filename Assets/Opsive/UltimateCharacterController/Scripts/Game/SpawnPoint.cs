/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// Specifies a location that the object can spawn.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        /// <summary>
        /// Specifies the shape in which the spawn point should randomly be determined.
        /// </summary>
        public enum SpawnShape
        {
            Point,  // The spawn point will be determined at the transform position.
            Sphere, // The spawn point will be determined within a random sphere.
            Box     // The spawn point will be determined within a box.
        }

        [Tooltip("An index value used to group multiple sets of spawn points. A value of -1 will ignore the grouping.")]
        [SerializeField] protected int m_Grouping = -1;
        [Tooltip("Specifies the shape in which the spawn point should randomly be determined.")]
        [SerializeField] protected SpawnShape m_Shape;
        [Tooltip("The size of the spawn shape.")]
        [SerializeField] protected float m_Size;
        [Tooltip("Specifies the height of the ground check.")]
        [SerializeField] protected float m_GroundSnapHeight;
        [Tooltip("Should the character spawn with a random y direction?")]
        [SerializeField] protected bool m_RandomDirection;
        [Tooltip("Should a check be performed to determine if there are any objects obstructing the spawn point?")]
        [SerializeField] protected bool m_CheckForObstruction;
        [Tooltip("The layers which can obstruct the spawn point.")]
        [SerializeField] protected LayerMask m_ObstructionLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.SubCharacter |
                                                                1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("If checking for obstruction, specifies how many times the location should be determined before it is decided that there are no valid spawn locations.")]
        [SerializeField] protected int m_PlacementAttempts = 10;
#if UNITY_EDITOR
        [Tooltip("The color to draw the editor gizmo in (editor only).")]
        [SerializeField] protected Color m_GizmoColor = new Color(1, 0, 0, 0.3f);
#endif

        public int Grouping
        {
            get { return m_Grouping; }
            set
            {
                if (m_Grouping != value) {
                    // The SpawnPointManager needs to be aware of the change so it can update its internal mapping.
                    if (Application.isPlaying) {
                        SpawnPointManager.UpdateSpawnPointGrouping(this, value);
                    }
                    m_Grouping = value;
                }
            }
        }
        public SpawnShape Shape { get { return m_Shape; } set { m_Shape = value; } }
        public float Size { get { return m_Size; } set { m_Size = value; } }
        public float GroundSnapHeight { get { return m_GroundSnapHeight; } set { m_GroundSnapHeight = value; } }
        public bool RandomDirection { get { return m_RandomDirection; } set { m_RandomDirection = value; } }
        public bool CheckForObstruction { get { return m_CheckForObstruction; } set { m_CheckForObstruction = value; } }
        public int PlacementAttempts { get { return m_PlacementAttempts; } set { m_PlacementAttempts = value; } }
#if UNITY_EDITOR
        public Color GizmoColor { get { return m_GizmoColor; } set { m_GizmoColor = value; } }
#endif

        private Transform m_Transform;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Adds the spawn point to the manager.
        /// </summary>
        private void OnEnable()
        {
            SpawnPointManager.AddSpawnPoint(this);
        }

        /// <summary>
        /// Gets the position and rotation of the spawn point. If false is returned then the point wasn't successfully retrieved.
        /// </summary>
        /// <param name="position">The position of the spawn point.</param>
        /// <param name="rotation">The rotation of the spawn point.</param>
        /// <returns>True if the spawn point was successfully retrieved.</returns>
        public bool GetPlacement(ref Vector3 position, ref Quaternion rotation)
        {
            position = RandomPosition();

            // Ensure the spawn point is clear of any obstructing objects.
            if (m_CheckForObstruction) {
                var attempt = 0;
                var success = false;
                while (attempt < m_PlacementAttempts) {
                    if (m_Shape == SpawnShape.Point) {
                        // A point will always succeed.
                        success = true;
                    } else if (m_Shape == SpawnShape.Sphere) {
                        if (!Physics.CheckSphere(position, m_Size, m_ObstructionLayers, QueryTriggerInteraction.Ignore)) {
                            success = true;
                            break;
                        }
                    } else { // Box.
                        var extents = Vector3.zero;
                        extents.x = extents.z = m_Size / 2;
                        extents.y = m_GroundSnapHeight / 2;
                        var boxPosition = m_Transform.TransformPoint(extents);
                        if (!Physics.CheckBox(boxPosition, extents, m_Transform.rotation, m_ObstructionLayers, QueryTriggerInteraction.Ignore)) {
                            success = true;
                            break;
                        }
                    }

                    position = RandomPosition();
                    attempt++;
                }

                // No valid position was found - return false.
                if (!success) {
                    return false;
                }
            }

            // If the ground snap height is positive then the position should be located on the ground.
            if (m_GroundSnapHeight > 0) {
                RaycastHit raycastHit;
                if (Physics.Raycast(position + m_Transform.up * m_GroundSnapHeight, -m_Transform.up, out raycastHit, m_GroundSnapHeight + 0.2f, m_ObstructionLayers, QueryTriggerInteraction.Ignore)) {
                    position = raycastHit.point + m_Transform.up * 0.01f;
                }
            }

            // Optionally rotate a random spawn direction.
            if (m_RandomDirection) {
                rotation = Quaternion.Euler(m_Transform.up * Random.Range(0, 360));
            } else {
                rotation = m_Transform.rotation;
            }

            return true;
        }

        /// <summary>
        /// Retruns a random position based on the shape.
        /// </summary>
        /// <returns></returns>
        private Vector3 RandomPosition()
        {
            var localPosition = Vector3.zero;
            if (m_Shape == SpawnShape.Sphere) {
                localPosition = Random.insideUnitSphere * m_Size;
                localPosition.y = 0;
            } else if (m_Shape == SpawnShape.Box) {
                var halfSize = m_Size / 2;
                localPosition.x = Random.Range(-halfSize, halfSize);
                localPosition.z = Random.Range(-halfSize, halfSize);
            }

            return m_Transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// Removes the spawn point from the manager.
        /// </summary>
        private void OnDisable()
        {
            SpawnPointManager.RemoveSpawnPoint(this);
        }
    }
}