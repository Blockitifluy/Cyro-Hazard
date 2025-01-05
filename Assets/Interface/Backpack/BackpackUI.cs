using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BackpackUI : MonoBehaviour
{
    public float TimeBetweenUpdate = 0.9f;
    public VisualTreeAsset SubBackpack;

    private float Timer = 0.0f;
    private float LastUpdate = 0.0f;

    private GameObject _PlayerObject;
    private PlayerBehaviour _PlayerBehaviour;
    private InputActionMap _PlayerInput;
    private InputAction _OpenAction;
    private UIDocument _UIDoc;

    public void ToggleMenu(InputAction.CallbackContext context)
    {
        _UIDoc.enabled = !_UIDoc.enabled;
    }

    public void Awake()
    {
        _UIDoc = GetComponent<UIDocument>();
        Debug.Log(_UIDoc);
    }

    // Start is called before the first frame update
    public void Start()
    {
        _PlayerObject = GameObject.FindGameObjectWithTag("Player");
        _PlayerBehaviour = _PlayerObject.GetComponent<PlayerBehaviour>();
        _PlayerInput = _PlayerBehaviour.GetInputAction();
        _OpenAction = _PlayerInput.FindAction("open-backpack");
        _OpenAction.performed += ToggleMenu;
    }

    public void ClearBackpack()
    {
        var Selection = _UIDoc.rootVisualElement.Q<ListView>("Selection");
        Selection.hierarchy.Clear();
    }

    public void UpdateBackpack()
    {
        var root = _UIDoc.rootVisualElement;

        if (root == null)
            return;

        var Selection = root.Q<ListView>("Selection");
        var Backpacks = _PlayerBehaviour.DetectBackpacks();

        ClearBackpack();

        foreach (Backpack pck in Backpacks)
        {
            var packUI = SubBackpack.Instantiate();

            var packLabel = packUI.Q<Label>("title");
            packLabel.text = pck.Name;

            var weightLabel = packUI.Q<Label>("weight");
            weightLabel.text = $"{pck.CurrentWeight}/{pck.MaxWeight}";

            Selection.hierarchy.Add(packUI);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        Timer += Time.deltaTime;

        if (Timer - LastUpdate >= TimeBetweenUpdate && _UIDoc.enabled)
        {
            UpdateBackpack();
            LastUpdate = Timer;
        }
    }
}
