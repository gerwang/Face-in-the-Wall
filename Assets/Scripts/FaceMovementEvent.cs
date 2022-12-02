public class FaceMovementEvent
{
    public float[] values;
    public int dropIndex;
    public bool isDrop;
    public float time;

    public FaceMovementEvent(float[] values, int dropIndex, bool isDrop, float time)
    {
        this.values = values;
        this.dropIndex = dropIndex;
        this.isDrop = isDrop;
        this.time = time;
    }
}