public class SpawnData
{
    public int ShipType;
    public int ShipsCount;
    public float DelayBeforeSpawn;

    public SpawnData(int shipType, int shipsCount, float delayBeforeSpawn = 0)
    {
        ShipType = shipType;
        ShipsCount = shipsCount;
        DelayBeforeSpawn = delayBeforeSpawn;
    }

    public override string ToString()
    {
        return "shipType: " + ShipType + ", ShipsCount: " + ShipsCount + ", DelayBeforeSpawn: " + DelayBeforeSpawn;
    }
}
