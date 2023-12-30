using TMPro;
using UnityEngine;
using static UniverseController;

public class Caption : CachedMonoBehaviour
{
    [HideInInspector]
    public Ship targetShip;
    TextMeshProUGUI _caption;
    Content _actualContent;

    public struct Content
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
    }

    void Start()
    {
        _caption = transform.Find("text").GetComponent<TextMeshProUGUI>();
    }

    public void Setup(Ship ship)
    {
        targetShip = ship;
        _actualContent = new(ship.name, ship.rb.velocity.magnitude, "...");
    }

    public void Update()
    {
        UpdatePosition();
        
        UpdateText(_actualContent);
    }

    void UpdatePosition()
    {
        var zOffset = targetShip.shipCollider.bounds.extents.z;
        // print("extents: " + yOffset);
        var coords = universeController.mainCamera.WorldToScreenPoint(targetShip.transformCached.position + Vector3.back * zOffset);
        coords = new(coords.x, coords.y, coords.z);
        transformCached.position = coords;
    }

    // TODO: Updatovat, jen když se zmení hodnoty
    public void UpdateText(Content content)
    {
        _caption.text = $"<b>{content.name}</b>\n{content.speed}\n<i>{content.customField}</i>";
    }
}
