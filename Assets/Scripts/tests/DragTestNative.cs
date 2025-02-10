using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DragTestNative : DragTest
{
    Text _infoText;
    float _slowingStartedAt;
    float _stoppedMovementAt;
    float _v;
    float _stoppingSqrVelocity;
    public Transform target;
    Vector3 _toTarget;

    void Start()
    {
        _infoText = GameObject.Find("info text 1").GetComponent<Text>();
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            disabledEnginesAtPosition = rb.position;
            force = 0;
            _slowingStartedAt = Time.time;
            _v = rb.velocity.magnitude;
            _stoppingSqrVelocity = rb.velocity.sqrMagnitude;
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        transform.LookAt(target);

        _toTarget = target.position - transform.position;
        var distance = _toTarget.magnitude;

        d = rb.velocity.magnitude * (1 / rb.drag - Time.fixedDeltaTime);

        d += 1f;

        if (distance < d)
        {
            force = 0;
            // rb.velocity = Vector3.zero;
        }
        else
            force = initialForce;

        /*
        if (rb.velocity.magnitude < .0001 && _slowingStartedAt > 0 && _stoppedMovementAt == 0)
        {
            // if (_slowingStartedAt > 0)
            _stoppedMovementAt = Time.time;

            // Shrnutí: Vyšel jsem ze vzorce pro zrychlení:     a = (v - v₀) / Δt,
            // vzorce pro zpomalení:                            v = v₀ * Mathf.Clamp01(1 - rb.drag * Time.fixedDeltaTime) a
            // vzorce pro brzdnou dráhu:                        d = v²/(2a)
            // Pozn.: Zrychlení vychází záporně (protože je to zpomalení), je po třeba někde převrátit známénko
            // Pozn.: Z nějakého důvodu jsem musel ze vzorce pro brzdnou dráhu vypustit násobení 2 🤔

            // brzdná dráha: d = v²/(2a)
            // d = _stoppingSqrVelocity / a;  // ok
            // d = _v * _v / (-_v * (z - 1) / (z * Δt));  // ok
            // d = - _v * _v / (_v * ((1 - rb.drag * Time.fixedDeltaTime) - 1) / ((1 - rb.drag * Time.fixedDeltaTime) * Δt));  // ok

            // d = - _v * _v * ((1 - rb.drag * Δt) * Δt / (_v * (- rb.drag * Δt)));  // ok
            // d = - _v * _v * ((1 - rb.drag * Δt) * Δt / (_v * (- rb.drag * Δt)));  // ok
            // d = - _v * _v * ((Δt - rb.drag * Δt * Δt) / (- _v * rb.drag * Δt));  // ok
            // d = - _v * _v * ((1 - rb.drag * Δt) / (- _v * rb.drag));  // ok
            // d = _v * ((1 - rb.drag * Δt) / rb.drag);  // ok
            // d = _v * ((1 - rb.drag * Δt) / rb.drag);  // ok

            d = _v * (1 / rb.drag - Δt);  <== To je ono!

            _infoText.text = "braking distance: " + s +
                             "\nestimated: " + d;

            EditorApplication.isPaused = true;
        }
        else
            _infoText.text = "speed: " + rb.velocity.magnitude;*/
    }
}
