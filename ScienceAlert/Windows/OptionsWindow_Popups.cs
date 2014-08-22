using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Windows
{
    using ProfileManager = ScienceAlertProfileManager;

    internal partial class OptionsWindow : MonoBehaviour, IDrawable
    {
        internal string editText = string.Empty;
        internal string lockName = string.Empty;
        internal PopupDialog popup;

#region save popup
        void SpawnSavePopup()
        {
            editText = ProfileManager.ActiveProfile.name;
            LockControls("ScienceAlertSavePopup");
            popup = PopupDialog.SpawnPopupDialog(new MultiOptionDialog(string.Empty, DrawSaveWindow, "Choose a name for this profile"), false, HighLogic.Skin);
        }



        private void DrawSaveWindow()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                GUI.SetNextControlName("ProfileName");
                editText = GUILayout.TextField(editText, GUILayout.ExpandWidth(true));

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (AudibleButton(new GUIContent("Accept"))) SaveCurrentProfile();
                if (AudibleButton(new GUIContent("Cancel"))) DismissPopup();

                GUI.FocusControl("ProfileName");
            }
            GUILayout.EndVertical();

            if (Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)
                    SaveCurrentProfile();

                if (Event.current.keyCode == KeyCode.Escape)
                    DismissPopup();
            }
            
        }



        private void SaveCurrentProfile()
        {
            popup.Dismiss();

            if (ProfileManager.HaveStoredProfile(editText))
            {
                Log.Warning("Existing profile named '{0}' found! Asking user for overwrite permission.", editText);
                popup = PopupDialog.SpawnPopupDialog(new MultiOptionDialog("Overwrite existing profile?", string.Format("Profile '{0}' already exists!", editText), HighLogic.Skin, new DialogOption[] { new DialogOption("Overwrite", new Callback(SaveCurrentProfileOverwrite)), new DialogOption("Cancel", new Callback(DismissPopup)) }), false, HighLogic.Skin);
            }
            else SaveCurrentProfileOverwrite(); // save to go ahead and save since no existing profile with this key exists
        }



        /// <summary>
        /// Saves the active profile as a stored profile and overwrites any
        /// previous existing version without informing the player
        /// </summary>
        private void SaveCurrentProfileOverwrite()
        {
            Log.Debug("OptionsWindow.SaveCurrentProfileOverwrite");
            ProfileManager.StoreActiveProfile(editText);
            Settings.Instance.Save();
            DismissPopup();
        }
        
#endregion

        private void LockControls(string lockName)
        {
            this.lockName = lockName;
            InputLockManager.SetControlLock(ControlTypes.ACTIONS_ALL, lockName);
            Log.Debug("OptionsWindow_Popups: Locked controls with {0}", lockName);
        }

        private void DismissPopup()
        {
            Log.Debug("OptionsWindow.DismissPopup");
            if (popup) popup.Dismiss();
            InputLockManager.RemoveControlLock(lockName); lockName = string.Empty;
        }

        public bool HasOpenPopup { get { return popup != null; } }
    }
}
