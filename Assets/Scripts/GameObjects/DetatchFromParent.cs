public class DetatchFromParent : BaseDisposeCallback
{
    public override void DisposeItem(float currentTime)
    {
        if (this.transform.parent != null)
        {
            this.transform.parent = null;
        }
    }
}
