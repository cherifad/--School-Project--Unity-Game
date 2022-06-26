using com.ootii.Helpers;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors.Combat
{
    /// <summary>
    /// An AttackStyle gives us details about a specific attack. We'll use
    /// these details to determine who can be hit and when.
    /// </summary>
    [Serializable]
    public class AttackStyle : ICombatStyle
    {
        /// <summary>
        /// Unique ID for the attack style (within the list)
        /// </summary>
        public string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Type of weapon this style can be associated with
        /// </summary>
        [Tooltip("Type of weapon this style can be associated with.")]
        public string _ItemType = "";
        public string ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }

        /// <summary>
        /// Helps define the animation that is tied to the style 
        /// </summary>
        [Tooltip("Helps define the animation that is tied to the style.")]
        public int _Form = -1;
        public int Form
        {
            get
            {
                if (_Form > -1) { return _Form; }
                return _ParameterID;
            }

            set
            {
                _Form = value;
                _Style = value;
                _ParameterID = value;
            }
        }

        /// <summary>
        /// Parameter value sent to the animator through the attack motion
        /// </summary>
        [Tooltip("Parameter value sent to the animator through the attack motion.")]
        public int _Parameter = 0;
        public int Parameter
        {
            get { return _Parameter; }
            set { _Parameter = value; }
        }

        /// <summary>
        /// Helps define the animation that is tied to the style. (Note: This is deprecated... use Form)
        /// </summary>
        public int _ParameterID = 0;       

        /// <summary>
        /// Helps define the animation that is tied to the style. (Note: This is deprecated... use Form)
        /// </summary>
        public int _Style = 0;      

        /// <summary>
        /// Defines the inventory slot ID that holds the weapon that is doing the attack
        /// </summary>
        [Tooltip("Defines the inventory slot ID that holds the weapon that is doing the attack.")]
        public string _InventorySlotID = "";
        public string InventorySlotID
        {
            get { return _InventorySlotID; }
            set { _InventorySlotID = value; }
        }

        /// <summary>
        /// Delay before the attack can be used again.
        /// </summary>
        [Tooltip("Delay before the attack can be used again (in seconds).")]
        public float _Delay = 0f;
        public float Delay
        {
            get { return _Delay; }
            set { _Delay = value; }
        }

        /// <summary>
        /// Determines if the attack is able to be stopped.
        /// </summary>
        [Tooltip("Determines if the attack is able to be stopped.")]
        public bool _IsInterruptible = true;
        public bool IsInterruptible
        {
            get { return _IsInterruptible; }
            set { _IsInterruptible = value; }
        }

        /// <summary>
        /// Flags that determine the effects that the combat style has.
        /// </summary>
        [Tooltip("Flags that determine the effects that the combat style has.")]
        public int _Effects = EnumCombatStyleEffect.NONE;
        public int Effects
        {
            get { return _Effects; }
            set { _Effects = value; }
        }

        /// <summary>
        /// Direction of the attack relative to the character's forward
        /// </summary>
        [Tooltip("Direction of the attack relative to the character's forward.")]
        public Vector3 _Forward = Vector3.forward;
        public Vector3 Forward
        {
            get { return _Forward; }
            set { _Forward = value; }
        }

        /// <summary>
        /// Horizontal field-of-attack centered on the Forward. This determines
        /// the horizontal range of the attack.
        /// </summary>
        [Tooltip("Horizontal field-of-attack centered on the Forward. This determines the horizontal range of the attack.")]
        public float _HorizontalFOA = 120f;
        public float HorizontalFOA
        {
            get { return _HorizontalFOA; }
            set { _HorizontalFOA = value; }
        }

        /// <summary>
        /// Vertical field-of-attack centered on the Forward. This determines
        /// the vertical range of the attack.
        /// </summary>
        [Tooltip("Vertical field-of-attack centered on the Forward. This determines the vertical range of the attack.")]
        public float _VerticalFOA = 90f;
        public float VerticalFOA
        {
            get { return _VerticalFOA; }
            set { _VerticalFOA = value; }
        }

        /// <summary>
        /// Minimum range for the attack (0 means use the combatant + weapon)
        /// </summary>
        [Tooltip("Minimum range for the attack (0 means use the combatant + weapon).")]
        public float _MinRange = 0f;
        public float MinRange
        {
            get { return _MinRange; }
            set { _MinRange = value; }
        }

        /// <summary>
        /// Maximum range for the attack (0 means use the combatant + weapon)
        /// </summary>
        [Tooltip("Maximum range for the attack (0 means use the combatant + weapon).")]
        public float _MaxRange = 0f;
        public float MaxRange
        {
            get { return _MaxRange; }
            set { _MaxRange = value; }
        }

        /// <summary>
        /// Amount to multiply the damage by
        /// </summary>
        [Tooltip("Amount to multiply the damage by.")]
        public float _DamageModifier = 1f;
        public float DamageModifier
        {
            get { return _DamageModifier; }
            set { _DamageModifier = value; }
        }

        /// <summary>
        /// Determines the next attack style to use during a chain
        /// </summary>
        [Tooltip("Determines the next attack style to use during a chain.")]
        public int _NextAttackStyleIndex = -1;
        public int NextAttackStyleIndex
        {
            get { return _NextAttackStyleIndex; }
            set { _NextAttackStyleIndex = value; }
        }

        /// <summary>
        /// Track the last time the attack was used
        /// </summary>
        protected float mLastAttackTime = 0f;
        public float LastAttackTime
        {
            get { return mLastAttackTime; }
            set { mLastAttackTime = value; }
        }          

        /// <summary>
        /// Default constructor
        /// </summary>
        public AttackStyle()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="rSource">Source to copy from</param>
        public AttackStyle(AttackStyle rSource)
        {
            if (rSource == null) { return; }

            _Name = rSource._Name;
            // CDL 07/01/2018  -_Form was missing
            _Form = rSource._Form;
            _ParameterID = rSource._ParameterID;
            _Style = rSource._Style;
            // CDL 07/01/2018 also missing : _Delay
            _Delay = rSource._Delay;
            _IsInterruptible = rSource._IsInterruptible;
            _Forward = rSource._Forward;
            _HorizontalFOA = rSource._HorizontalFOA;
            _VerticalFOA = rSource._VerticalFOA;
            _MinRange = rSource._MinRange;
            _MaxRange = rSource._MaxRange;
            _DamageModifier = rSource._DamageModifier;
        }

#if UNITY_EDITOR
        public bool EditorDrawAllFields = true;

        public bool OnInspectorGUI(UnityEngine.Object rTarget)
        {
            bool lIsDirty = false;

            EditorHelper.DrawSmallTitle(this.Name.Length > 0 ? this.Name : "Attack Style");

            if (EditorHelper.TextField("Name", "ID of the attack style.", this.Name, rTarget))
            {
                lIsDirty = true;
                this.Name = EditorHelper.FieldStringValue;
            }

            if (EditorDrawAllFields && EditorHelper.TextField("Item Type", "Type of item this attack style is valid for.", this.ItemType, rTarget))
            {
                lIsDirty = true;
                this.ItemType = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.IntField("Form", "Motion form used to determine the animation to play.", this.Form, rTarget))
            {
                lIsDirty = true;
                this.Form = EditorHelper.FieldIntValue;
            }

            if (EditorHelper.IntField("Parameter", "Parameter value sent to the animator to help determine the animation to play.", this.Parameter, rTarget))
            {
                lIsDirty = true;
                this.Parameter = EditorHelper.FieldIntValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.TextField("Slot ID", "Inventory slot ID that defines the weapon used for the attack.", this.InventorySlotID, rTarget))
            {
                lIsDirty = true;
                this.InventorySlotID = EditorHelper.FieldStringValue;
            }

            if (EditorHelper.FloatField("Delay", "Time (in seconds) before the attack can be used again.", this.Delay, rTarget))
            {
                lIsDirty = true;
                this.Delay = EditorHelper.FieldFloatValue;
            }

            if (EditorHelper.BoolField("Is Blockable", "Determines if the attack can be blocked", this.IsInterruptible, rTarget))
            {
                lIsDirty = true;
                this.IsInterruptible = EditorHelper.FieldBoolValue;
            }

            GUILayout.Space(5f);

            if (EditorHelper.Vector3Field("Attack Forward", "Normalized center of the attack. FOA values are based on this.", this.Forward, rTarget))
            {
                lIsDirty = true;
                this.Forward = EditorHelper.FieldVector3Value.normalized;
            }

            EditorGUILayout.BeginHorizontal();

            EditorHelper.LabelField("Field of Attack", "Horizontal and vertical field of attack when colliders are not used.", EditorGUIUtility.labelWidth - 4f);

            if (EditorHelper.FloatField(this.HorizontalFOA, "Horizontal FOA", rTarget, 0f, 31f))
            {
                lIsDirty = true;
                this.HorizontalFOA = EditorHelper.FieldFloatValue;
            }

            if (EditorHelper.FloatField(this.VerticalFOA, "Vertical FOA", rTarget, 0f, 31f))
            {
                lIsDirty = true;
                this.VerticalFOA = EditorHelper.FieldFloatValue;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorHelper.LabelField("Range", "Min and Max range of the attack. Set to 0 to use combatant + weapon range.", EditorGUIUtility.labelWidth - 4f);

            if (EditorHelper.FloatField(this.MinRange, "Min Range", rTarget, 0f, 31f))
            {
                lIsDirty = true;
                this.MinRange = EditorHelper.FieldFloatValue;
            }

            if (EditorHelper.FloatField(this.MaxRange, "Max Range", rTarget, 0f, 31f))
            {
                lIsDirty = true;
                this.MaxRange = EditorHelper.FieldFloatValue;
            }

            EditorGUILayout.EndHorizontal();

            if (EditorHelper.FloatField("Damage Modifier", "Multiplier that this style applies to the weapon's damage.", this.DamageModifier, rTarget))
            {
                lIsDirty = true;
                this.DamageModifier = EditorHelper.FieldFloatValue;
            }

            if (EditorDrawAllFields && EditorHelper.IntField("Next Attack Style", "When chaining attacks, the next attack style that we'll default to.", this.NextAttackStyleIndex, rTarget))
            {
                lIsDirty = true;
                this.NextAttackStyleIndex = EditorHelper.FieldIntValue;
            }

            return lIsDirty;
        }
#endif
    }
}
