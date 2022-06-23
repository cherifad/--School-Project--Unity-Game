/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    /// <summary>
    /// Adds and serializes CameraController view types.
    /// </summary>
    public static class ViewTypeBuilder
    {
        private static Dictionary<Type, AddViewState> s_AddViewState = new Dictionary<Type, AddViewState>();

        /// <summary>
        /// Adds the view type with the specified type.
        /// </summary>
        /// <param name="cameraController">The camera to add the ability to.</param>
        /// <param name="viewType">The type of view type to add.</param>
        /// <returns>The added view type.</returns>
        public static ViewType AddViewType(CameraController cameraController, Type viewType)
        {
            var viewTypes = cameraController.ViewTypes;
            if (viewTypes == null) {
                viewTypes = new ViewType[1];
            } else {
                Array.Resize(ref viewTypes, viewTypes.Length + 1);
            }

            var viewTypeObj = Activator.CreateInstance(viewType) as ViewType;
            viewTypes[viewTypes.Length - 1] = viewTypeObj;
            cameraController.ViewTypes = viewTypes;
            if (string.IsNullOrEmpty(cameraController.ViewTypeFullName) && !(viewTypeObj is Transition)) {
                cameraController.ViewTypeFullName = viewType.FullName;
            }

#if FIRST_PERSON_CONTROLLER
            if (viewTypeObj is FirstPersonController.Camera.ViewTypes.FirstPerson) {
                // A first person camera must be added to the first peron view types.
                UnityEngine.Camera firstPersonCamera = null;
                FirstPersonController.Camera.ViewTypes.FirstPerson firstPersonViewType;
                for (int i = 0; i < viewTypes.Length; ++i) {
                    if ((firstPersonViewType = viewTypes[i] as FirstPersonController.Camera.ViewTypes.FirstPerson) != null &&
                        firstPersonViewType.FirstPersonCamera != null) {
                        firstPersonCamera = firstPersonViewType.FirstPersonCamera;
                        break;
                    }
                }

                // If the camera is null then a new first person camera should be created.
                if (firstPersonCamera == null) {
                    UnityEngine.Transform firstPersonCameraTransform;
                    if ((firstPersonCameraTransform = cameraController.transform.Find("First Person Camera")) != null) {
                        firstPersonCamera = firstPersonCameraTransform.GetComponent<UnityEngine.Camera>();
                    }

                    if (firstPersonCamera == null) {
                        var cameraGameObject = new UnityEngine.GameObject("First Person Camera");
                        cameraGameObject.transform.SetParentOrigin(cameraController.transform);
                        firstPersonCamera = cameraGameObject.AddComponent<UnityEngine.Camera>();
                        firstPersonCamera.clearFlags = UnityEngine.CameraClearFlags.Depth;
                        firstPersonCamera.fieldOfView = 60f;
                        firstPersonCamera.nearClipPlane = 0.01f;
                        firstPersonCamera.depth = 0;
                        firstPersonCamera.renderingPath = cameraController.GetComponent<UnityEngine.Camera>().renderingPath;
                    }
                }

                (viewTypeObj as FirstPersonController.Camera.ViewTypes.FirstPerson).FirstPersonCamera = firstPersonCamera;
            }
#endif

#if UNITY_EDITOR
            var addViewState = GetAddViewState(viewType);
            if (addViewState != null) {
                var presetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(addViewState.PresetGUID);
                if (!string.IsNullOrEmpty(presetPath)) {
                    var preset = UnityEditor.AssetDatabase.LoadAssetAtPath(presetPath, typeof(StateSystem.PersistablePreset)) as StateSystem.PersistablePreset;
                    if (preset != null) {
                        var states = viewTypeObj.States;
                        Array.Resize(ref states, 2);
                        states[1] = states[0];
                        states[0] = new StateSystem.State(addViewState.Name, preset, null);
                        viewTypeObj.States = states;
                    }
                }
            }
#endif

            SerializeViewTypes(cameraController);
            cameraController.SetViewType(viewType, false);
            return viewTypeObj;
        }

        /// <summary>
        /// Serialize all of the view types to the ViewTypeData array.
        /// </summary>
        /// <param name="cameraController">The camera controller to serialize.</param>
        public static void SerializeViewTypes(CameraController cameraController)
        {
            var viewTypes = new List<ViewType>(cameraController.ViewTypes);
            cameraController.ViewTypeData = Serialization.Serialize<ViewType>(viewTypes);
            cameraController.ViewTypes = viewTypes.ToArray();
        }

        /// <summary>
        /// Returns the AddViewState of the specified view type.
        /// </summary>
        /// <param name="type">The view type.</param>
        /// <returns>The DefaultAbilityIndex of the specified ability type. Can be null.</returns>
        private static AddViewState GetAddViewState(Type type)
        {
            AddViewState addViewState;
            if (s_AddViewState.TryGetValue(type, out addViewState)) {
                return addViewState;
            }

            if (type.GetCustomAttributes(typeof(AddViewState), true).Length > 0) {
                addViewState = type.GetCustomAttributes(typeof(AddViewState), true)[0] as AddViewState;
            }
            s_AddViewState.Add(type, addViewState);
            return addViewState;
        }
    }
}