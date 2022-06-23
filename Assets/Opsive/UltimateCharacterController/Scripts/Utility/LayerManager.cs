/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Easy access to the Unity layer system.
    /// </summary>
    public class LayerManager : MonoBehaviour
    {
        // Built-in Unity layers
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;

        // Custom layers
        public const int Enemy = 26;
        public const int MovingPlatform = 27;
        public const int VisualEffect = 28;
        public const int Overlay = 29;
        public const int SubCharacter = 30;
        public const int Character = 31;
        
        [Tooltip("Layer Mask that specifies the layer that the enemies use.")]
        [SerializeField] protected LayerMask m_EnemyLayers = 1 << Enemy;
        [Tooltip("Layer Mask that specifies any layers that are invisible to the character (such as water or invisible planes placed on top of stairs). ")]
        [SerializeField] protected LayerMask m_InvisibleLayers = (1 << TransparentFX) | (1 << IgnoreRaycast) | (1 << UI) | (1 << Overlay) | (1 << VisualEffect) | (1 << SubCharacter);
        [Tooltip("Layer mask that specifies any layers that represent a solid object (such as the ground or a moving platform).")]
        [SerializeField] protected LayerMask m_SolidObjectLayers = (1 << Default) | (1 << TransparentFX) | (1 << MovingPlatform) | (1 << Enemy) | (1 << Character);

        public LayerMask EnemyLayers { get { return m_EnemyLayers; } }
        public LayerMask InvisibleLayers { get { return m_InvisibleLayers; } }
        public LayerMask SolidObjectLayers { get { return m_SolidObjectLayers; } }

        // Represents the mask that ignores any invisible objects.
        public int IgnoreInvisibleLayers { get { return ~m_InvisibleLayers; } }
        // Represents the mask that ignores any invisible objects and the character.
        public int IgnoreInvisibleCharacterLayers { get { return ~(m_InvisibleLayers | m_CharacterLayer); } }
        // Represents the mask that ignores any invisible objects and the character/water.
        public int IgnoreInvisibleCharacterWaterLayers { get { return ~(m_InvisibleLayers | m_CharacterLayer | (1 << Water)); } }

        private LayerMask m_CharacterLayer = 1 << Character;
        private static Dictionary<Collider, List<Collider>> s_IgnoreCollisionMap;

        public LayerMask CharacterLayer { get { return m_CharacterLayer; } }

        /// <summary>
        /// Setups the layer collisions.
        /// </summary>
        public void Awake()
        {
            Physics.IgnoreLayerCollision(Character, Water);
            Physics.IgnoreLayerCollision(IgnoreRaycast, VisualEffect);
            Physics.IgnoreLayerCollision(SubCharacter, Default);
            Physics.IgnoreLayerCollision(SubCharacter, VisualEffect);
            Physics.IgnoreLayerCollision(VisualEffect, VisualEffect);

            m_CharacterLayer = 1 << gameObject.layer;
        }

        /// <summary>
        /// Ignore the collision between the main collider and the other collider.
        /// </summary>
        /// <param name="mainCollider">The main collider collision to ignore.</param>
        /// <param name="otherCollider">The collider to ignore.</param>
        public static void IgnoreCollision(Collider mainCollider, Collider otherCollider)
        {
            // Keep a mapping of the colliders that mainCollider is ignorning so the collision can easily be reverted.
            if (s_IgnoreCollisionMap == null) {
                s_IgnoreCollisionMap = new Dictionary<Collider, List<Collider>>();
            }

            // Add the collider to the list so it can be reverted.
            List<Collider> colliderList;
            if (!s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList)) {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(mainCollider, colliderList);
            }
            colliderList.Add(otherCollider);

            // The otherCollider must also keep track of the mainCollder. This allows otherCollider to be removed before mainCollider.
            if (!s_IgnoreCollisionMap.TryGetValue(otherCollider, out colliderList)) {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(otherCollider, colliderList);
            }
            colliderList.Add(mainCollider);

            // Do the actual ignore.
            Physics.IgnoreCollision(mainCollider, otherCollider);
        }

        /// <summary>
        /// The main collider should no longer ignore any collisions.
        /// </summary>
        /// <param name="mainCollider">The collider to revert the collisions on.</param>
        public static void RevertCollision(Collider mainCollider)
        {
            List<Collider> colliderList;
            List<Collider> otherColliderList;
            // Revert the IgnoreCollision setting on all of the colliders that the object is currently ignoring.
            if (s_IgnoreCollisionMap != null && s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList)) {
                for (int i = 0; i < colliderList.Count; ++i) {
                    if (!mainCollider.enabled || !mainCollider.gameObject.activeInHierarchy || !colliderList[i].enabled || !colliderList[i].gameObject.activeInHierarchy) {
                        continue;
                    }

                    Physics.IgnoreCollision(mainCollider, colliderList[i], false);

                    // A two way map was added when the initial IgnoreCollision was added. Remove that second map because the IgnoreCollision has been removed.
                    if (s_IgnoreCollisionMap.TryGetValue(colliderList[i], out otherColliderList)) {
                        for (int j = 0; j < otherColliderList.Count; ++j) {
                            if (otherColliderList[j].Equals(mainCollider)) {
                                otherColliderList.RemoveAt(j);
                                break;
                            }
                        }
                    }
                }
                colliderList.Clear();
            }
        }
    }
}