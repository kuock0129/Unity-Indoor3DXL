// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class DetectionUiMenuManager : MonoBehaviour
    {
        [Header("Ui buttons")]
        [SerializeField] private OVRInput.RawButton m_actionButton = OVRInput.RawButton.A;

        [Header("Ui elements ref.")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private GameObject m_initialPanel;
        [SerializeField] private GameObject m_noPermissionPanel;
        [SerializeField] private GameObject m_selectObjectPanel;      // NEW
        [SerializeField] private GameObject m_scaleObjectPanel;       // NEW
        [SerializeField] private Text m_labelInfromation;
        [SerializeField] private AudioSource m_buttonSound;

        public bool IsInputActive { get; set; } = false;

        public UnityEvent<bool> OnPause;

        // Menu state flags
        private bool m_initialMenu;
        private bool m_selectObjectMenu;     // MISSING - Added
        private bool m_scaleObjectMenu;      // MISSING - Added

        // start menu
        private int m_objectsDetected = 0;
        private int m_objectsIdentified = 0;

        // pause menu
        public bool IsPaused { get; private set; } = true;

        #region Unity Functions
        private IEnumerator Start()
        {
            // Hide all panels initially
            m_initialPanel.SetActive(false);
            m_noPermissionPanel.SetActive(false);
            m_selectObjectPanel.SetActive(false);
            m_scaleObjectPanel.SetActive(false);
            m_loadingPanel.SetActive(true);

            IsInputActive = true;

            // Wait until Sentis model is loaded
            var sentisInference = FindFirstObjectByType<SentisInferenceRunManager>();
            while (!sentisInference.IsModelLoaded)
            {
                yield return null;
            }
            m_loadingPanel.SetActive(false);

            while (!PassthroughCameraPermissions.HasCameraPermission.HasValue)
            {
                yield return null;
            }

            if (PassthroughCameraPermissions.HasCameraPermission == false)
            {
                OnNoPermissionMenu();
            }
            else
            {
                OnInitialMenu(true);  // Assuming you want to check for scene permission here
            }
        }

        private void Update()
        {
            if (!IsInputActive)
                return;

            if (m_initialMenu)
            {
                InitialMenuUpdate();
            }
            else if (m_selectObjectMenu)
            {
                SelectObjectMenuUpdate();
            }
            else if (m_scaleObjectMenu)
            {
                ScaleObjectMenuUpdate();
            }
        }
        #endregion

        #region Ui state: No permissions Menu
        private void OnNoPermissionMenu()
        {
            // Reset all menu states
            m_initialMenu = false;
            m_selectObjectMenu = false;
            m_scaleObjectMenu = false;
            IsPaused = true;

            // Hide all panels except no permission
            m_initialPanel.SetActive(false);
            m_selectObjectPanel.SetActive(false);
            m_scaleObjectPanel.SetActive(false);
            m_noPermissionPanel.SetActive(true);
        }
        #endregion

        #region Ui state: Initial Menu
        public void OnInitialMenu(bool hasScenePermission)
        {
            // Check if we have the Scene data permission
            if (hasScenePermission)
            {
                // Set menu states
                m_initialMenu = true;
                m_selectObjectMenu = false;
                m_scaleObjectMenu = false;
                IsPaused = true;
                IsInputActive = true;

                // Show only initial panel
                m_initialPanel.SetActive(true);
                m_selectObjectPanel.SetActive(false);
                m_scaleObjectPanel.SetActive(false);
                m_noPermissionPanel.SetActive(false);
                m_loadingPanel.SetActive(false);
            }
            else
            {
                OnNoPermissionMenu();
            }
        }

        private void InitialMenuUpdate()
        {
            if (OVRInput.GetUp(m_actionButton) || Input.GetKeyUp(KeyCode.Return))
            {
                m_buttonSound?.Play();
                OnSelectObjectMenu();  // FIXED: Changed from OnPauseMenu(false)
            }
        }
        #endregion

        #region Ui state: Select Object Menu
        private void OnSelectObjectMenu()
        {
            // Set menu states
            m_initialMenu = false;
            m_selectObjectMenu = true;
            m_scaleObjectMenu = false;
            IsPaused = true;

            // Show only select object panel
            m_initialPanel.SetActive(false);
            m_selectObjectPanel.SetActive(true);
            m_scaleObjectPanel.SetActive(false);
            m_noPermissionPanel.SetActive(false);
            m_loadingPanel.SetActive(false);
        }

        private void SelectObjectMenuUpdate()
        {
            // Check for trigger press on one controller
            if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) ||
                OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) ||
                Input.GetKeyUp(KeyCode.Space))  // For testing in editor
            {
                m_buttonSound?.Play();
                OnScaleObjectMenu();
            }
        }
        #endregion

        #region Ui state: Scale Object Menu
        private void OnScaleObjectMenu()
        {
            // Set menu states
            m_initialMenu = false;
            m_selectObjectMenu = false;
            m_scaleObjectMenu = true;
            IsPaused = true;

            // Show only scale object panel
            m_initialPanel.SetActive(false);
            m_selectObjectPanel.SetActive(false);
            m_scaleObjectPanel.SetActive(true);
            m_noPermissionPanel.SetActive(false);
            m_loadingPanel.SetActive(false);
        }

        private void ScaleObjectMenuUpdate()
        {
            // Check for triggers on both controllers
            if ((OVRInput.Get(OVRInput.RawButton.RIndexTrigger) &&
                 OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) ||
                 Input.GetKeyDown(KeyCode.B))  // For testing in editor - changed to GetKeyDown
            {
                m_buttonSound?.Play();
                OnDetectionMode();
            }
        }

        private void OnDetectionMode()
        {
            // Set menu states
            m_initialMenu = false;
            m_selectObjectMenu = false;
            m_scaleObjectMenu = false;
            IsPaused = false;  // Start detection

            // Hide all menu panels
            m_initialPanel.SetActive(false);
            m_selectObjectPanel.SetActive(false);
            m_scaleObjectPanel.SetActive(false);
            m_noPermissionPanel.SetActive(false);
            m_loadingPanel.SetActive(false);

            OnPause?.Invoke(false);  // Start the detection system
        }
        #endregion

        #region Ui state: detection information
        private void UpdateLabelInformation()
        {
            m_labelInfromation.text = $"Unity Sentis version: 2.1.1\nAI model: Yolo\nDetecting objects: {m_objectsDetected}\n";
        }

        public void OnObjectsDetected(int objects)
        {
            m_objectsDetected = objects;
            UpdateLabelInformation();
        }

        public void OnObjectsIndentified(int objects)
        {
            if (objects < 0)
            {
                // reset the counter
                m_objectsIdentified = 0;
            }
            else
            {
                m_objectsIdentified += objects;
            }
            UpdateLabelInformation();
        }
        #endregion

        #region REMOVED: Old OnPauseMenu - No longer needed
        // The old OnPauseMenu method has been replaced by the new workflow
        #endregion
    }
}