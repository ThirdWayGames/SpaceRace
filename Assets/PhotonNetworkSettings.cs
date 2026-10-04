public static class PhotonNetworkSettings
{
    /// <summary>
    /// One-shot events. Not stored in the room buffer, so a late joiner does not replay them.
    /// </summary>
    public static PhotonTargets EventTarget
    {
        get { return PhotonTargets.AllViaServer; }
    }

    /// <summary>
    /// A single setter that represents current state, and only when a room property is a worse fit.
    /// </summary>
    public static PhotonTargets StateTarget
    {
        get { return PhotonTargets.AllBufferedViaServer; }
    }
}
