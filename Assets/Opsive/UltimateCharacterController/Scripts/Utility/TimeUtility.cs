/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System.Collections;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Utility functions related to time.
    /// </summary>
    public class TimeUtility
    {
        // The target framerate. Application.targetFramerate can return 0 so it isn't used.
        private const int c_TargetFramerate = 60;

        /// <summary>
        /// Returns an alternative delta time which is based on framerate where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based delta time</returns>
        public static float FramerateDeltaTime
        {
            get { return Time.deltaTime * c_TargetFramerate; }
        }

        /// <summary>
        /// Returns an alternative delta time which is based on framerate and timescale where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based delta time.</returns>
        public static float FramerateDeltaTimeScaled
        {
            get { return Time.deltaTime * c_TargetFramerate * Time.timeScale; }
        }

        /// <summary>
        /// Returns an alternative fixed delta time which is based on framerate where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based fixed delta time.</returns>
        public static float FramerateFixedDeltaTime
        {
            get { return Time.fixedDeltaTime * c_TargetFramerate; }
        }

        /// <summary>
        /// Returns an alternative fixed delta time which is based on framerate and timescale where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based fixed delta time.</returns>
        public static float FramerateFixedDeltaTimeScaled
        {
            get { return FramerateFixedDeltaTime * Time.timeScale; }
        }

        /// <summary>
        /// Returns the fixed delta time modified by the timescale.
        /// </summary>
        /// <returns>Fixed delta time modified by the timescale.</returns>
        public static float FixedDeltaTimeScaled
        {
            get { return Time.fixedDeltaTime * Time.timeScale; }
        }
    }
}