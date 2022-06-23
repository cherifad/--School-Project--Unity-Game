/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Utility
{
    /// <summary>
    /// Helper class which will get an InspectorDrawer for the specified type.
    /// </summary>
    public class InspectorDrawerUtility
    {
        private static Dictionary<Type, InspectorDrawer> s_InspectorDrawerTypeMap;

        /// <summary>
        /// The object has been enabled again.
        /// </summary>
        public static void OnEnable()
        {
            s_InspectorDrawerTypeMap = null;
        }

        /// <summary>
        /// Returns the InspectorDrawer for the specified type.
        /// </summary>
        /// <param name="type">The type to retrieve the InspectorDrawer of.</param>
        /// <returns>The found InspectorDrawer. Can be null.</returns>
        public static InspectorDrawer InspectorDrawerForType(Type type)
        {
            if (type == null) {
                return null;
            }

            if (s_InspectorDrawerTypeMap == null) {
                BuildInspectorDrawerMap();
            }

            InspectorDrawer inspectorDrawer;
            if (!s_InspectorDrawerTypeMap.TryGetValue(type, out inspectorDrawer)) {
                return InspectorDrawerForType(type.BaseType);
            }

            return inspectorDrawer;
        }

        /// <summary>
        /// Builds the dictionary which contains all of the InspectorDrawers.
        /// </summary>
        private static void BuildInspectorDrawerMap()
        {
            s_InspectorDrawerTypeMap = new Dictionary<Type, InspectorDrawer>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from InspectorDrawer.
                    if (!typeof(InspectorDrawer).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // Create the InspectorDrawer if the type has the InspectorDrawerAttribute.
                    InspectorDrawerAttribute[] attribute;
                    if ((attribute = assemblyTypes[j].GetCustomAttributes(typeof(InspectorDrawerAttribute), false) as InspectorDrawerAttribute[]).Length > 0) {
                        var inspectorDrawer = Activator.CreateInstance(assemblyTypes[j]) as InspectorDrawer;
                        s_InspectorDrawerTypeMap.Add(attribute[0].Type, inspectorDrawer);
                    }
                }
            }
        }
    }
}