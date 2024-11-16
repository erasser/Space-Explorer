public class Laser : Projectile
{
    new void FixedUpdate()
    {
        base.FixedUpdate();

        UpdatePosition();
    }

    void UpdatePosition()
    {
        transform.Translate(velocity);
    }

}
