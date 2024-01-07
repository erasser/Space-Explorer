using TMPro;
using UnityEngine;
using static UniverseController;

public class Caption : CachedMonoBehaviour
{
    [HideInInspector]
    public Ship targetShip;
    TextMeshProUGUI _caption;
    RectTransform _armor;
    // Content _actualContent;
    Damageable _damageable;

    /*public struct Content
    {
        public string name;
        public string speed;
        public string customField;

        public Content(string shipName, float velocityMagnitude, string customText = "")
        {
            name = shipName;
            speed = velocityMagnitude.ToString();
            customField = customText;
        }
    }*/

    void Start()
    {
        _caption = transform.Find("text").GetComponent<TextMeshProUGUI>();
        _armor = transform.Find("bar_armor").GetComponent<RectTransform>();
        _damageable = targetShip.transform.GetComponent<Damageable>();
    }

    public void Setup(Ship ship)
    {
        targetShip = ship;
        // _actualContent = new(ship.name, ship.rb.velocity.magnitude, "...");
    }

    public void Update()
    {
        if (!targetShip)  // TODO: Vyřešit přes Event
        {
            DestroyImmediate(gameObject);
            return;
        }

        UpdatePosition();
        
        UpdateText(/*_actualContent*/);
        
        UpdateBars();
    }

    void UpdatePosition()
    {
        var coords = universeController.mainCamera.WorldToScreenPoint(targetShip.transformCached.position + targetShip.shipCollider.bounds.extents.z * 2 * Vector3.back);
        // coords = new(coords.x, coords.y, coords.z);
        transformCached.position = coords;
    }

    public void UpdateText(/*Content content*/)
    {
        if (!targetShip.velocityEstimator) return;

        _caption.text = $"<b>{targetShip.name}</b>\n" +
                        $"speed: {Mathf.Round(targetShip.rb.velocity.magnitude)} m/s\n" +
                        $"<i>est. speed: {Mathf.Round(targetShip.velocityEstimator.GetVelocityEstimate().magnitude)}</i>";
    }

    void UpdateBars()
    {
        if (_damageable)
            _armor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _damageable.currentArmor);
    }
}
