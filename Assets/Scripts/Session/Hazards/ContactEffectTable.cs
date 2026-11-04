public static class ContactEffectTable
{
    public const string HazardPathogenName = "Anthrax";

    /// <summary>
    /// Hull drain for the storm and for living hostiles stays in the sim. This table never adds a second drain.
    /// Revealed does not change the result.
    /// </summary>
    public static ContactEffect Resolve(ContactState contact)
    {
        var effect = new ContactEffect();
        if (contact == null)
        {
            return effect;
        }

        effect.ContactId = contact.Id ?? string.Empty;
        if (!contact.Overlapping)
        {
            return effect;
        }

        if (contact.Kind == ContactKind.Hazard)
        {
            effect.PathogenResourceName = HazardPathogenName;
            return effect;
        }

        if (contact.Kind == ContactKind.Hostile && contact.Health > 0f)
        {
            effect.RaiseAlarm = true;
            effect.SpawnBoarder = true;
        }

        return effect;
    }
}
