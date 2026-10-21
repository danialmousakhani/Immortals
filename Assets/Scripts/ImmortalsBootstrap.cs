using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ImmortalsBootstrap : MonoBehaviour
{
    public static ImmortalsBootstrap Instance;

    public ImmortalsPlayer player;
    private Text statusText;
    private int defeatedEnemies;
    private float dayCycle;

    private Material MaterialOf(Color color, float metallic = 0f)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");

        Material material = new Material(shader);
        material.color = color;

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);

        return material;
    }

    private void Start()
    {
        Instance = this;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Application.targetFrameRate = 60;

        BuildLighting();
        BuildWorld();
        BuildPlayer();
        BuildEnemies();
        BuildInterface();
    }

    private void Update()
    {
        dayCycle += Time.deltaTime * 0.02f;

        float sunlight = (Mathf.Sin(dayCycle) + 1f) * 0.5f;
        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.20f, 0.20f, 0.25f),
            new Color(0.65f, 0.55f, 0.40f),
            sunlight
        );

        if (statusText != null && player != null)
        {
            statusText.text =
                "IMMORTALS | PERSIAN EMPIRE\n" +
                "Mission: Secure the Border Fort\n" +
                "Health: " + player.Health +
                "     Enemies defeated: " + defeatedEnemies;
        }
    }

    private void BuildLighting()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.67f, 0.52f, 0.35f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.006f;

        GameObject sun = new GameObject("Persian Sun");
        Light lightSource = sun.AddComponent<Light>();
        lightSource.type = LightType.Directional;
        lightSource.intensity = 1.25f;
        lightSource.color = new Color(1f, 0.82f, 0.63f);
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
    }

    private GameObject CreateObject(
        string objectName,
        PrimitiveType type,
        Vector3 position,
        Vector3 scale,
        Material material,
        bool hasCollider = true)
    {
        GameObject obj = GameObject.CreatePrimitive(type);

        obj.name = objectName;
        obj.transform.position = position;
        obj.transform.localScale = scale;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = material;

        if (!hasCollider)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        return obj;
    }

    private void BuildWorld()
    {
        Material sand = MaterialOf(new Color(0.62f, 0.46f, 0.28f));
        Material stone = MaterialOf(new Color(0.53f, 0.49f, 0.41f));
        Material darkStone = MaterialOf(new Color(0.29f, 0.28f, 0.25f));
        Material gold = MaterialOf(new Color(0.76f, 0.52f, 0.16f), 0.45f);
        Material dryGrass = MaterialOf(new Color(0.30f, 0.32f, 0.17f));

        CreateObject(
            "Persian Plains",
            PrimitiveType.Plane,
            new Vector3(0f, 0f, 0f),
            new Vector3(25f, 1f, 25f),
            sand
        );

        CreateObject(
            "Royal Road",
            PrimitiveType.Cube,
            new Vector3(0f, 0.04f, 4f),
            new Vector3(3.5f, 0.08f, 36f),
            stone
        );

        // Fortress walls, with an opening for the main gate.
        CreateObject("West Wall", PrimitiveType.Cube,
            new Vector3(-13f, 1.5f, 6f),
            new Vector3(0.8f, 3f, 22f), darkStone);

        CreateObject("East Wall", PrimitiveType.Cube,
            new Vector3(13f, 1.5f, 6f),
            new Vector3(0.8f, 3f, 22f), darkStone);

        CreateObject("Rear Wall", PrimitiveType.Cube,
            new Vector3(0f, 1.5f, 17f),
            new Vector3(26f, 3f, 0.8f), darkStone);

        CreateObject("Front Left Wall", PrimitiveType.Cube,
            new Vector3(-8f, 1.3f, -5f),
            new Vector3(10f, 2.6f, 0.8f), darkStone);

        CreateObject("Front Right Wall", PrimitiveType.Cube,
            new Vector3(8f, 1.3f, -5f),
            new Vector3(10f, 2.6f, 0.8f), darkStone);

        // Monumental Persian-style gate.
        CreateObject("Gate Left Pillar", PrimitiveType.Cube,
            new Vector3(-3f, 1.8f, -5f),
            new Vector3(0.9f, 3.6f, 1.2f), gold);

        CreateObject("Gate Right Pillar", PrimitiveType.Cube,
            new Vector3(3f, 1.8f, -5f),
            new Vector3(0.9f, 3.6f, 1.2f), gold);

        CreateObject("Gate Crown", PrimitiveType.Cube,
            new Vector3(0f, 3.6f, -5f),
            new Vector3(7f, 0.6f, 1.4f), gold);

        // Palace columns and gold-colored capitals.
        for (int i = 0; i < 9; i++)
        {
            float x = -10f + i * 2.5f;

            CreateObject("Palace Column " + i,
                PrimitiveType.Cylinder,
                new Vector3(x, 1.6f, 12f),
                new Vector3(0.65f, 1.6f, 0.65f),
                stone);

            CreateObject("Palace Capital " + i,
                PrimitiveType.Cube,
                new Vector3(x, 3.35f, 12f),
                new Vector3(1.15f, 0.35f, 1.05f),
                gold);
        }

        // Watchtowers.
        for (int i = 0; i < 4; i++)
        {
            float x = i % 2 == 0 ? -11f : 11f;
            float z = i < 2 ? -1f : 14f;

            CreateObject("Watchtower " + i,
                PrimitiveType.Cube,
                new Vector3(x, 2f, z),
                new Vector3(2f, 4f, 2f),
                darkStone);

            CreateObject("Watchtower Roof " + i,
                PrimitiveType.Cube,
                new Vector3(x, 4.1f, z),
                new Vector3(2.5f, 0.35f, 2.5f),
                gold);
        }

        // Decorative stones outside the clear fighting path.
        Random.InitState(173);

        for (int i = 0; i < 55; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-24f, 24f),
                0.1f,
                Random.Range(-20f, 25f)
            );

            if (Mathf.Abs(position.x) < 4f &&
                position.z > -9f && position.z < 13f)
                continue;

            CreateObject(
                "Dryland Rock",
                PrimitiveType.Sphere,
                position,
                Vector3.one * Random.Range(0.3f, 0.9f),
                i % 2 == 0 ? dryGrass : stone,
                false
            );
        }
    }

    private void BuildPlayer()
    {
        GameObject root = new GameObject("Persian Immortal");
        root.transform.position = new Vector3(0f, 0.05f, -11f);

        CharacterController controller =
            root.AddComponent<CharacterController>();

        controller.height = 2f;
        controller.radius = 0.42f;
        controller.center = new Vector3(0f, 1f, 0f);
        controller.stepOffset = 0.3f;

        Material armor = MaterialOf(new Color(0.39f, 0.17f, 0.11f));
        Material skin = MaterialOf(new Color(0.66f, 0.45f, 0.30f));
        Material red = MaterialOf(new Color(0.75f, 0.04f, 0.03f));

        GameObject body = CreateObject(
            "Armor", PrimitiveType.Capsule,
            root.transform.position + Vector3.up,
            new Vector3(0.7f, 0.9f, 0.7f), armor, false
        );
        body.transform.SetParent(root.transform, true);

        GameObject head = CreateObject(
            "Helmet", PrimitiveType.Sphere,
            root.transform.position + Vector3.up * 1.85f,
            Vector3.one * 0.42f, skin, false
        );
        head.transform.SetParent(root.transform, true);

        GameObject crest = CreateObject(
            "Red Helmet Crest", PrimitiveType.Cube,
            root.transform.position + new Vector3(0f, 2.15f, -0.04f),
            new Vector3(0.16f, 0.65f, 0.16f), red, false
        );
        crest.transform.SetParent(root.transform, true);

        player = root.AddComponent<ImmortalsPlayer>();

        Camera cam = Camera.main;

        if (cam == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cam = cameraObject.AddComponent<Camera>();
        }

        cam.fieldOfView = 65f;

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        ImmortalsCamera follow =
            cam.GetComponent<ImmortalsCamera>();

        if (follow == null)
            follow = cam.gameObject.AddComponent<ImmortalsCamera>();

        follow.target = root.transform;
    }

    private void BuildEnemies()
    {
        Material enemyArmor =
            MaterialOf(new Color(0.22f, 0.24f, 0.28f));

        Vector3[] positions =
        {
            new Vector3(-6f, 1f, 1f),
            new Vector3(5f, 1f, 2f),
            new Vector3(-8f, 1f, 7f),
            new Vector3(8f, 1f, 8f),
            new Vector3(-4f, 1f, 12f),
            new Vector3(3f, 1f, 13f),
            new Vector3(0f, 1f, 6f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject enemy = CreateObject(
                "Enemy Soldier " + (i + 1),
                PrimitiveType.Capsule,
                positions[i],
                new Vector3(0.85f, 1f, 0.85f),
                enemyArmor
            );

            ImmortalsEnemy ai = enemy.AddComponent<ImmortalsEnemy>();
            ai.target = player;
        }
    }

    private void BuildInterface()
    {
        if (EventSystem.current == null)
        {
            GameObject eventObject = new GameObject("EventSystem");
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObject = new GameObject("Game HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Mission Panel");
        panel.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        panelRect.sizeDelta = new Vector2(520f, 125f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.05f, 0.04f, 0.78f);

        GameObject textObject = new GameObject("Mission Text");
        textObject.transform.SetParent(panel.transform, false);

        statusText = textObject.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 25;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.MiddleLeft;

        RectTransform textRect = statusText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 8f);
        textRect.offsetMax = new Vector2(-10f, -8f);

        // Movement buttons on the left side of the screen.
        CreateHoldButton(canvasObject.transform, "↑",
            new Vector2(150f, 138f), ImmortalsPlayer.Direction.Up);

        CreateHoldButton(canvasObject.transform, "↓",
            new Vector2(150f, 12f), ImmortalsPlayer.Direction.Down);

        CreateHoldButton(canvasObject.transform, "←",
            new Vector2(86f, 75f), ImmortalsPlayer.Direction.Left);

        CreateHoldButton(canvasObject.transform, "→",
            new Vector2(214f, 75f), ImmortalsPlayer.Direction.Right);

        // Combat actions on the right side.
        CreateActionButton(canvasObject.transform, "ATTACK",
            new Vector2(-30f, 40f), () => player.Attack());

        CreateActionButton(canvasObject.transform, "RUN",
            new Vector2(-30f, 116f), () => player.ToggleRun());
    }

    private void CreateHoldButton(
        Transform parent,
        string label,
        Vector2 position,
        ImmortalsPlayer.Direction direction)
    {
        GameObject obj = MakeButton(
            parent, label, position, false, null);

        ImmortalsHoldButton hold =
            obj.AddComponent<ImmortalsHoldButton>();

        hold.target = player;
        hold.direction = direction;
    }

    private void CreateActionButton(
        Transform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        MakeButton(parent, label, position, true, action);
    }

    private GameObject MakeButton(
        Transform parent,
        string label,
        Vector2 position,
        bool anchorRight,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();

        Vector2 anchor = anchorRight
            ? new Vector2(1f, 0f)
            : new Vector2(0f, 0f);

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(120f, 60f);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.15f, 0.10f, 0.06f, 0.90f);

        Button button = obj.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        if (action != null)
            button.onClick.AddListener(action);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(obj.transform, false);

        Text text = labelObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = label;
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform labelRect = text.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return obj;
    }

    public void RegisterKill()
    {
        defeatedEnemies++;
    }
}

public class ImmortalsPlayer : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }

    public int Health = 100;
    public float walkSpeed = 4f;
    public float runSpeed = 6.8f;

    private CharacterController controller;
    private float verticalSpeed;
    private float attackCooldown;
    private bool running;

    private bool holdUp, holdDown, holdLeft, holdRight;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (holdLeft) x -= 1f;
        if (holdRight) x += 1f;
        if (holdDown) z -= 1f;
        if (holdUp) z += 1f;

        Vector3 input = new Vector3(x, 0f, z).normalized;

        if (Camera.main != null && input.sqrMagnitude > 0.01f)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            input = (forward * z + right * x).normalized;

            if (input.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(input),
                    12f * Time.deltaTime
                );
            }
        }

        if (controller.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;

        verticalSpeed += -20f * Time.deltaTime;

        float speed = running || Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : walkSpeed;

        controller.Move(
            (input * speed + Vector3.up * verticalSpeed)
            * Time.deltaTime
        );

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        attackCooldown -= Time.deltaTime;
    }

    public void SetDirection(Direction direction, bool pressed)
    {
        switch (direction)
        {
            case Direction.Up: holdUp = pressed; break;
            case Direction.Down: holdDown = pressed; break;
            case Direction.Left: holdLeft = pressed; break;
            case Direction.Right: holdRight = pressed; break;
        }
    }

    public void ToggleRun()
    {
        running = !running;
    }

    public void Attack()
    {
        if (attackCooldown > 0f)
            return;

        attackCooldown = 0.55f;

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1.25f
                + Vector3.up,
            1.5f
        );

        foreach (Collider hit in hits)
        {
            ImmortalsEnemy enemy =
                hit.GetComponentInParent<ImmortalsEnemy>();

            if (enemy != null &&
                Vector3.Distance(transform.position,
                    enemy.transform.position) < 3.2f)
            {
                enemy.TakeHit(1);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Health = 100;
            transform.position = new Vector3(0f, 0.05f, -11f);
        }
    }
}

public class ImmortalsEnemy : MonoBehaviour
{
    public ImmortalsPlayer target;
    public int health = 2;
    public float moveSpeed = 1.7f;

    private float attackTimer;

    private void Update()
    {
        if (target == null || health <= 0)
            return;

        Vector3 difference = target.transform.position - transform.position;
        difference.y = 0f;

        float distance = difference.magnitude;

        if (distance > 1.8f && distance < 18f)
        {
            transform.position +=
                difference.normalized * moveSpeed * Time.deltaTime;

            if (difference.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(difference),
                    6f * Time.deltaTime
                );
            }
        }

        if (distance <= 1.8f)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                target.TakeDamage(10);
                attackTimer = 1.4f;
            }
        }
    }

    public void TakeHit(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            if (ImmortalsBootstrap.Instance != null)
                ImmortalsBootstrap.Instance.RegisterKill();

            Destroy(gameObject);
        }
        else
        {
            transform.position -= transform.forward * 0.5f;
        }
    }
}

public class ImmortalsCamera : MonoBehaviour
{
    public Transform target;

    private float yaw;
    private float pitch = 15f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * 3f;
            pitch -= Input.GetAxis("Mouse Y") * 2.5f;
            pitch = Mathf.Clamp(pitch, -5f, 55f);
        }

        // Swipe on the open right-hand side rotates the mobile camera.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.position.x > Screen.width * 0.48f &&
                touch.position.y > Screen.height * 0.30f &&
                touch.phase == TouchPhase.Moved)
            {
                yaw += touch.deltaPosition.x * 0.12f;
                pitch -= touch.deltaPosition.y * 0.08f;
                pitch = Mathf.Clamp(pitch, -5f, 55f);
            }
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * 1.2f;

        transform.position =
            focus + rotation * new Vector3(0f, 1.5f, -6.5f);

        transform.LookAt(focus);
    }
}

public class ImmortalsHoldButton :
    MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ImmortalsPlayer target;
    public ImmortalsPlayer.Direction direction;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null)
            target.SetDirection(direction, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target != null)
            target.SetDirection(direction, false);
    }

    private void OnDisable()
    {
        if (target != null)
            target.SetDirection(direction, false);
    }
}