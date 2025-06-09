// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class SentisInferenceUiManager : MonoBehaviour
    {
        [Header("Placement configureation")]
        [SerializeField] private EnvironmentRayCastSampleManager m_environmentRaycast;
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        private PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("UI display references")]
        [SerializeField] private SentisObjectDetectedUiManager m_detectionCanvas;
        [SerializeField] private RawImage m_displayImage;
        [SerializeField] private Sprite m_boxTexture;
        //[SerializeField] private Color m_boxColor;
        [SerializeField] private Color m_furnitureColor = Color.blue;
        [SerializeField] private Color m_electronicsColor = Color.cyan;
        [SerializeField] private Color m_foodColor = Color.green;
        [SerializeField] private Color m_animalColor = Color.yellow;
        [SerializeField] private Color m_vehicleColor = Color.red;
        [SerializeField] private Color m_personColor = Color.magenta;
        [SerializeField] private Color m_accessoryColor = Color.gray;
        //[SerializeField] private Color m_sportsColor = Color.orange;
        [SerializeField] private Color m_sportsColor = new Color(1f, 0.5f, 0f); // orange
        [SerializeField] private Color m_utensilColor = Color.white;
        [SerializeField] private Color m_trafficColor = Color.black;
        //[SerializeField] private Color m_miscColor = Color.brown;
        [SerializeField] private Color m_miscColor = new Color(0.6f, 0.3f, 0f); // brown
        [SerializeField] private Color m_defaultColor = Color.white;
        [SerializeField] private Font m_font;
        [SerializeField] private Color m_fontColor;
        [SerializeField] private int m_fontSize = 80;
        [Space(10)]
        public UnityEvent<int> OnObjectsDetected;

        public List<BoundingBox> BoxDrawn = new();

        private string[] m_labels;
        private List<GameObject> m_boxPool = new();
        private Transform m_displayLocation;

        // Dictionary for better color management
        private Dictionary<string, Color> m_categoryColors;

        //bounding box data
        public struct BoundingBox
        {
            public float CenterX;
            public float CenterY;
            public float Width;
            public float Height;
            public string Label;
            public Vector3? WorldPos;
            public string ClassName;
        }

        #region Unity Functions
        private void Start()
        {
            m_displayLocation = m_displayImage.transform;
            InitializeCategoryColors();
        }
        #endregion

        #region Detection Functions
        public void OnObjectDetectionError()
        {
            // Clear current boxes
            ClearAnnotations();

            // Set obejct found to 0
            OnObjectsDetected?.Invoke(0);
        }
        #endregion

        #region BoundingBoxes functions
        public void SetLabels(TextAsset labelsAsset)
        {
            //Parse neural net m_labels
            m_labels = labelsAsset.text.Split('\n');
        }

        public void SetDetectionCapture(Texture image)
        {
            m_displayImage.texture = image;
            m_detectionCanvas.CapturePosition();
        }

        private void InitializeCategoryColors()
        {
            m_categoryColors = new Dictionary<string, Color>();

            // Person
            m_categoryColors["person"] = m_personColor;

            // Vehicles
            m_categoryColors["bicycle"] = m_vehicleColor;
            m_categoryColors["car"] = m_vehicleColor;
            m_categoryColors["motorbike"] = m_vehicleColor;
            m_categoryColors["aeroplane"] = m_vehicleColor;
            m_categoryColors["bus"] = m_vehicleColor;
            m_categoryColors["train"] = m_vehicleColor;
            m_categoryColors["truck"] = m_vehicleColor;
            m_categoryColors["boat"] = m_vehicleColor;

            // Traffic
            m_categoryColors["traffic light"] = m_trafficColor;
            m_categoryColors["fire hydrant"] = m_trafficColor;
            m_categoryColors["stop sign"] = m_trafficColor;
            m_categoryColors["parking meter"] = m_trafficColor;

            // Animals
            m_categoryColors["bird"] = m_animalColor;
            m_categoryColors["cat"] = m_animalColor;
            m_categoryColors["dog"] = m_animalColor;
            m_categoryColors["horse"] = m_animalColor;
            m_categoryColors["sheep"] = m_animalColor;
            m_categoryColors["cow"] = m_animalColor;
            m_categoryColors["elephant"] = m_animalColor;
            m_categoryColors["bear"] = m_animalColor;
            m_categoryColors["zebra"] = m_animalColor;
            m_categoryColors["giraffe"] = m_animalColor;

            // Accessories
            m_categoryColors["backpack"] = m_accessoryColor;
            m_categoryColors["umbrella"] = m_accessoryColor;
            m_categoryColors["handbag"] = m_accessoryColor;
            m_categoryColors["tie"] = m_accessoryColor;
            m_categoryColors["suitcase"] = m_accessoryColor;
            m_categoryColors["toothbrush"] = m_accessoryColor;

            // Sports
            m_categoryColors["frisbee"] = m_sportsColor;
            m_categoryColors["skis"] = m_sportsColor;
            m_categoryColors["snowboard"] = m_sportsColor;
            m_categoryColors["sports ball"] = m_sportsColor;
            m_categoryColors["kite"] = m_sportsColor;
            m_categoryColors["baseball bat"] = m_sportsColor;
            m_categoryColors["baseball glove"] = m_sportsColor;
            m_categoryColors["skateboard"] = m_sportsColor;
            m_categoryColors["surfboard"] = m_sportsColor;
            m_categoryColors["tennis racket"] = m_sportsColor;

            // Food & Drink
            m_categoryColors["bottle"] = m_foodColor;
            m_categoryColors["wine glass"] = m_foodColor;
            m_categoryColors["cup"] = m_foodColor;
            m_categoryColors["bowl"] = m_foodColor;
            m_categoryColors["banana"] = m_foodColor;
            m_categoryColors["apple"] = m_foodColor;
            m_categoryColors["sandwich"] = m_foodColor;
            m_categoryColors["orange"] = m_foodColor;
            m_categoryColors["broccoli"] = m_foodColor;
            m_categoryColors["carrot"] = m_foodColor;
            m_categoryColors["hot dog"] = m_foodColor;
            m_categoryColors["pizza"] = m_foodColor;
            m_categoryColors["donut"] = m_foodColor;
            m_categoryColors["cake"] = m_foodColor;

            // Utensils
            m_categoryColors["fork"] = m_utensilColor;
            m_categoryColors["knife"] = m_utensilColor;
            m_categoryColors["spoon"] = m_utensilColor;

            // Furniture
            m_categoryColors["chair"] = m_furnitureColor;
            m_categoryColors["sofa"] = m_furnitureColor;
            m_categoryColors["bed"] = m_furnitureColor;
            m_categoryColors["diningtable"] = m_furnitureColor;
            m_categoryColors["toilet"] = m_furnitureColor;
            m_categoryColors["bench"] = m_furnitureColor;

            // Electronics
            m_categoryColors["tvmonitor"] = m_electronicsColor;
            m_categoryColors["laptop"] = m_electronicsColor;
            m_categoryColors["mouse"] = m_electronicsColor;
            m_categoryColors["remote"] = m_electronicsColor;
            m_categoryColors["keyboard"] = m_electronicsColor;
            m_categoryColors["cell phone"] = m_electronicsColor;
            m_categoryColors["microwave"] = m_electronicsColor;
            m_categoryColors["oven"] = m_electronicsColor;
            m_categoryColors["toaster"] = m_electronicsColor;
            m_categoryColors["sink"] = m_electronicsColor;
            m_categoryColors["refrigerator"] = m_electronicsColor;
            m_categoryColors["hair drier"] = m_electronicsColor;

            // Miscellaneous
            m_categoryColors["pottedplant"] = m_miscColor;
            m_categoryColors["book"] = m_miscColor;
            m_categoryColors["clock"] = m_miscColor;
            m_categoryColors["vase"] = m_miscColor;
            m_categoryColors["scissors"] = m_miscColor;
            m_categoryColors["teddy bear"] = m_miscColor;
        }

        private Color GetCategoryColor(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Debug.Log("Empty or null class name detected, using default color");
                return m_defaultColor;
            }

            // Log the original class name for debugging
            Debug.Log($"Detected class name: '{className}'");

            // Clean the class name - remove underscores, convert to lowercase, trim whitespace
            string cleanClassName = className.ToLower().Replace("_", " ").Trim();
            Debug.Log($"Cleaned class name: '{cleanClassName}'");

            // Try exact match first
            if (m_categoryColors.ContainsKey(cleanClassName))
            {
                Color foundColor = m_categoryColors[cleanClassName];
                Debug.Log($"Found exact match for '{cleanClassName}', using color: {foundColor}");
                return foundColor;
            }

            Debug.Log($"No color match found for class '{className}' (cleaned: '{cleanClassName}'), using default color: {m_defaultColor}");
            return m_defaultColor;
        }

        public void DrawUIBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {
            // Updte canvas position
            m_detectionCanvas.UpdatePosition();

            // Clear current boxes
            ClearAnnotations();

            var displayWidth = m_displayImage.rectTransform.rect.width;
            var displayHeight = m_displayImage.rectTransform.rect.height;

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;

            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            var boxesFound = output.shape[0];
            if (boxesFound <= 0)
            {
                OnObjectsDetected?.Invoke(0);
                return;
            }
            var maxBoxes = Mathf.Min(boxesFound, 200);

            OnObjectsDetected?.Invoke(maxBoxes);

            //Get the camera intrinsics
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var camRes = intrinsics.Resolution;

            //Draw the bounding boxes
            for (var n = 0; n < maxBoxes; n++)
            {
                // Get bounding box center coordinates
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var perX = (centerX + halfWidth) / displayWidth;
                var perY = (centerY + halfHeight) / displayHeight;

                // Get object class name
                var classname = m_labels[labelIDs[n]].Replace(" ", "_");

                // Get the 3D marker world position using Depth Raycast
                var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
                var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
                var worldPos = m_environmentRaycast.PlaceGameObjectByScreenPos(ray);

                // Create a new bounding box
                var box = new BoundingBox
                {
                    CenterX = centerX,
                    CenterY = centerY,
                    ClassName = classname,
                    Width = output[n, 2] * scaleX,
                    Height = output[n, 3] * scaleY,
                    Label = $"Id: {n} Class: {classname} Center (px): {(int)centerX},{(int)centerY} Center (%): {perX:0.00},{perY:0.00}",
                    WorldPos = worldPos,
                };

                // Add to the list of boxes
                BoxDrawn.Add(box);

                // Draw 2D box
                DrawBox(box, n);
            }
        }

        private void ClearAnnotations()
        {
            foreach (var box in m_boxPool)
            {
                box?.SetActive(false);
            }
            BoxDrawn.Clear();
        }

        private void DrawBox(BoundingBox box, int id)
        {
            // Get the category color for both box and label
            Color categoryColor = GetCategoryColor(box.ClassName);

            //Create the bounding box graphic or get from pool
            GameObject panel;
            if (id < m_boxPool.Count)
            {
                panel = m_boxPool[id];
                if (panel == null)
                {
                    panel = CreateNewBox(categoryColor);

                }
                else
                {
                    panel.SetActive(true);
                    // Update color for reused panels
                    var img = panel.GetComponent<Image>();
                    if (img != null) img.color = categoryColor;

                    // Update label color to match box color
                    var labelComponent = panel.GetComponentInChildren<Text>();
                    if (labelComponent != null) labelComponent.color = categoryColor;
                }
            }
            else
            {
                panel = CreateNewBox(categoryColor);

            }

            //Set box position
            panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY, box.WorldPos.HasValue ? box.WorldPos.Value.z : 0.0f);
            //Set box rotation
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - m_detectionCanvas.GetCapturedCameraPosition());
            //Set box size
            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(box.Width, box.Height);
            //Set label text and color
            var label = panel.GetComponentInChildren<Text>();
            label.text = box.Label;
            label.fontSize = 12;
            label.color = categoryColor; // Set label color to match box color
        }

        private GameObject CreateNewBox(Color color)
        {
            //Create the box and set image
            var panel = new GameObject("ObjectBox");
            _ = panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = m_boxTexture;
            img.type = Image.Type.Sliced;
            img.fillCenter = false;
            panel.transform.SetParent(m_displayLocation, false);

            //Create the label
            var text = new GameObject("ObjectLabel");
            _ = text.AddComponent<CanvasRenderer>();
            text.transform.SetParent(panel.transform, false);
            var txt = text.AddComponent<Text>();
            txt.font = m_font;
            txt.color = color; // Use the same color as the box
            txt.fontSize = m_fontSize;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rt2 = text.GetComponent<RectTransform>();
            rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
            rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
            rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
            rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
            rt2.anchorMin = new Vector2(0, 0);
            rt2.anchorMax = new Vector2(1, 1);

            m_boxPool.Add(panel);
            return panel;
        }
        #endregion
    }
}