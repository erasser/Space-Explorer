using TMPro;
using UnityEngine;
using static UniverseController;

public class Caption : MonoBehaviour
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
        transform.position = MainCamera.WorldToScreenPoint(targetShip.transform.position + targetShip.shipCollider.bounds.extents.z * 2 * Vector3.back);
    }

    public void UpdateText(/*Content content*/)
    {
        // if (!targetShip.velocityEstimator) return;
        var state = targetShip.GetComponent<MyNavMeshAgent>()?.state;
        // var state = agent

        _caption.text = $"<b><color=yellow>{targetShip.name}</color></b>\n" +
                        $"speed: <color=white>{Mathf.Round(targetShip.rb.velocity.magnitude)} m/s</color>\n" +
                        // $"<i>est. speed: {Mathf.Round(targetShip.velocityEstimator.GetVelocityEstimate().magnitude)}</i>";
                        $"state: <i><color=white>{state}</color></i>";
    }

    void UpdateBars()
    {
        if (_damageable)
            _armor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _damageable.currentArmor);
    }
}
