using UnityEngine;
using UnityEngine.UI;

public class DragTestMine : DragTest
{
    public float drag;
    Text _infoText;

    void Start()
    {
        _infoText = GameObject.Find("info text 2").GetComponent<Text>();
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            disabledEnginesAtPosition = rb.position;
            force = 0;

            var a = 1 - drag * Time.fixedDeltaTime;
            d = .5f * rb.velocity.sqrMagnitude / a;
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        rb.velocity *= Mathf.Clamp01(1f - drag * .02f);

        _infoText.text = "speed: " + rb.velocity.magnitude;

        if (rb.velocity.magnitude < .0001)
        {
            _infoText.text = "braking distance: " + (rb.position - disabledEnginesAtPosition).magnitude +
                             "\nestimated: " + d;
        }
    }
}
