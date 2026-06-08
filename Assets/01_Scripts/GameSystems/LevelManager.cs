using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public LevelDatabase levelDatabase;
    public int currentLevelIndex = 0;
    public Car carPrefab;
    public Material redMat;
    public Material blueMat;
    public Material yellowMat;
    public Material greenMat;
    public Material pinkMat;

    [Header("Level Up UI")]
    public Button levelUpButton;
    public TextMeshProUGUI levelUpButtonText;
    public string levelUpButtonLabel = "LEVEL UP";
    public TextMeshProUGUI endMessageText;
    public Button restartButton;
    public TextMeshProUGUI restartButtonText;
    public string endMessage = "That's the end of the case study";
    public string restartButtonLabel = "BACK TO START";

    private GameObject currentLevelInstance;
    private ParkingLane[] currentLevelLanes;
    private readonly List<Car> spawnedCars = new List<Car>();
    private bool isLevelCompleted = false;
    private bool isLoadingLevel = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        setupLevelUpButton();
        setupEndScreen();
        hideLevelUpButton();
        hideEndScreen();

        loadLevel(currentLevelIndex);
    }

    public void loadLevel(int index)
    {
        isLoadingLevel = true;
        isLevelCompleted = false;
        hideLevelUpButton();
        hideEndScreen();

        for (int i = 0; i < spawnedCars.Count; i++)
        {
            if (spawnedCars[i] != null)
            {
                Destroy(spawnedCars[i].gameObject);
            }
        }
        spawnedCars.Clear();

        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        if (levelDatabase == null || levelDatabase.levels == null || levelDatabase.levels.Count == 0)
        {
            isLoadingLevel = false;
            return;
        }

        if (index >= levelDatabase.levels.Count) index = 0;
        if (index < 0) index = 0;
        currentLevelIndex = index;

        LevelData levelData = levelDatabase.levels[index];

        if (TrackManager.instance != null)
        {
            TrackManager.instance.carsOnTrack.Clear();
            TrackManager.instance.blockedWaypoints.Clear();
            TrackManager.instance.setTrackCapacity(levelData.trackCapacity);
        }

        currentLevelInstance = Instantiate(levelData.levelLayoutPrefab);

        LevelLayout layout = currentLevelInstance.GetComponent<LevelLayout>();
        if (layout != null)
        {
            TrackManager.instance.waypoints.Clear();
            TrackManager.instance.waypoints.AddRange(layout.waypoints);
        }

        ParkingLane[] lanesInLevel = currentLevelInstance.GetComponentsInChildren<ParkingLane>();
        currentLevelLanes = lanesInLevel;

        for (int i = 0; i < levelData.laneSetups.Length; i++)
        {
            if (i >= lanesInLevel.Length) break;

            ParkingLane currentLane = lanesInLevel[i];
            CarColor[] colorsForThisLane = levelData.laneSetups[i].carColors;

            for (int j = 0; j < colorsForThisLane.Length; j++)
            {
                if (j >= currentLane.parkPositions.Length) break;

                CarColor colorToSpawn = colorsForThisLane[j];

                Car newCar = Instantiate(carPrefab, currentLane.parkPositions[j].position, currentLane.parkPositions[j].rotation);
                newCar.transform.localScale = carPrefab.transform.localScale;
                newCar.setupCarColor(colorToSpawn, getMaterialForColor(colorToSpawn));

                currentLane.parkedCars.Add(newCar);
                spawnedCars.Add(newCar);
            }

            currentLane.updateLaneTargetColor();
        }

        isLoadingLevel = false;
        for (int i = 0; i < currentLevelLanes.Length; i++)
        {
            if (currentLevelLanes[i] != null)
            {
                currentLevelLanes[i].checkIfCompleted();
            }
        }
        checkLevelCompleted();
    }

    public void checkLevelCompleted()
    {
        if (isLoadingLevel || isLevelCompleted || currentLevelLanes == null) return;

        if (TrackManager.instance != null && TrackManager.instance.carsOnTrack.Count > 0) return;

        for (int i = 0; i < currentLevelLanes.Length; i++)
        {
            ParkingLane lane = currentLevelLanes[i];
            if (lane == null) continue;
            if (lane.getTotalExpectedCars() == 0) continue;
            if (!lane.IsCompleted) return;
        }

        isLevelCompleted = true;
        if (hasNextLevel())
        {
            showLevelUpButton();
        }
        else
        {
            showEndScreen();
        }
    }

    private Material getMaterialForColor(CarColor color)
    {
        switch (color)
        {
            case CarColor.red: return redMat;
            case CarColor.blue: return blueMat;
            case CarColor.yellow: return yellowMat;
            case CarColor.green: return greenMat;
            case CarColor.pink: return pinkMat;
            default: return blueMat;
        }
    }

    public void nextLevel()
    {
        if (levelDatabase == null || levelDatabase.levels == null || levelDatabase.levels.Count == 0) return;
        if (!hasNextLevel()) return;

        currentLevelIndex++;
        loadLevel(currentLevelIndex);
    }

    public void restartFromFirstLevel()
    {
        loadLevel(0);
    }

    private bool hasNextLevel()
    {
        return levelDatabase != null
            && levelDatabase.levels != null
            && currentLevelIndex + 1 < levelDatabase.levels.Count;
    }

    private void setupLevelUpButton()
    {
        if (levelUpButton == null)
        {
            levelUpButton = createLevelUpButton();
        }

        if (levelUpButton == null) return;

        if (levelUpButtonText == null)
        {
            levelUpButtonText = levelUpButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (levelUpButtonText != null)
        {
            levelUpButtonText.text = levelUpButtonLabel;
        }

        levelUpButton.onClick.RemoveListener(nextLevel);
        levelUpButton.onClick.AddListener(nextLevel);
    }

    private void setupEndScreen()
    {
        Canvas canvas = getOrCreateCanvas();
        ensureEventSystemExists();

        if (endMessageText == null)
        {
            GameObject messageObj = new GameObject("End Message");
            messageObj.transform.SetParent(canvas.transform, false);

            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.5f, 0.5f);
            messageRect.anchorMax = new Vector2(0.5f, 0.5f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.anchoredPosition = new Vector2(0f, 58f);
            messageRect.sizeDelta = new Vector2(720f, 90f);

            endMessageText = messageObj.AddComponent<TextMeshProUGUI>();
            endMessageText.alignment = TextAlignmentOptions.Center;
            endMessageText.fontSize = 38f;
            endMessageText.fontStyle = FontStyles.Bold;
            endMessageText.color = Color.white;
        }

        endMessageText.text = endMessage;

        if (restartButton == null)
        {
            restartButton = createButton(canvas, "Restart Button", new Vector2(0.5f, 0.5f), new Vector2(0f, -36f), new Vector2(280f, 72f), restartButtonLabel);
        }

        if (restartButtonText == null)
        {
            restartButtonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (restartButtonText != null)
        {
            restartButtonText.text = restartButtonLabel;
        }

        restartButton.onClick.RemoveListener(restartFromFirstLevel);
        restartButton.onClick.AddListener(restartFromFirstLevel);
    }

    private Button createLevelUpButton()
    {
        Canvas canvas = getOrCreateCanvas();
        ensureEventSystemExists();

        return createButton(canvas, "Level Up Button", new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(240f, 72f), levelUpButtonLabel);
    }

    private Canvas getOrCreateCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private void ensureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    private Button createButton(Canvas canvas, string objectName, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, string labelText)
    {
        GameObject buttonObj = new GameObject(objectName);
        buttonObj.transform.SetParent(canvas.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = anchor;
        buttonRect.anchorMax = anchor;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = size;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.75f, 0.4f, 0.95f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.15f, 0.9f, 0.5f, 1f);
        colors.pressedColor = new Color(0.07f, 0.55f, 0.28f, 1f);
        button.colors = colors;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 30f;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;

        return button;
    }

    private void showLevelUpButton()
    {
        if (levelUpButton == null) return;
        levelUpButton.gameObject.SetActive(true);
    }

    private void hideLevelUpButton()
    {
        if (levelUpButton == null) return;
        levelUpButton.gameObject.SetActive(false);
    }

    private void showEndScreen()
    {
        if (endMessageText != null) endMessageText.gameObject.SetActive(true);
        if (restartButton != null) restartButton.gameObject.SetActive(true);
    }

    private void hideEndScreen()
    {
        if (endMessageText != null) endMessageText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
    }
}
