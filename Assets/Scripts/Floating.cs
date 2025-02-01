using UnityEngine;

public class Floating : MonoBehaviour
{
    public bool rotateAroundRandomAxis;
    public Vector3 constantRotation;
    public float rotationSpeed = .8f;
    public Space rotationSpace = Space.World;
    [Space(12)]
    public bool randomFloatX;
    public bool randomFloatY;
    public bool randomFloatZ;
    public float floatSpeed = 1;
    public float floatAmplitude = .2f;
    public Space translationSpace = Space.World;

    Vector3 _randomAxis;
    float _randomSeed;

    void Start()
    {
        _randomAxis = new(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

        _randomSeed = Random.value * 2;
    }

    void Update()
    {
        var coef = Time.deltaTime * 60;
        
        if (rotateAroundRandomAxis)
        {
            transform.Rotate(_randomAxis, rotationSpeed);
        }

        if (randomFloatX || randomFloatY || randomFloatZ)
        {
            Vector3 moveBy = new Vector3();

            if (randomFloatX)
                moveBy.x += Mathf.Sin((Time.time + _randomSeed) * floatSpeed) * floatAmplitude * coef;
            if (randomFloatY)
                moveBy.y += Mathf.Sin((Time.time + _randomSeed) * floatSpeed) * floatAmplitude * coef;
            if (randomFloatZ)
                moveBy.z += Mathf.Sin((Time.time + _randomSeed) * floatSpeed) * floatAmplitude * coef;

            transform.Translate(moveBy, translationSpace);
        }
        
        transform.Rotate(constantRotation, rotationSpeed * Time.deltaTime, rotationSpace);
    }
}
