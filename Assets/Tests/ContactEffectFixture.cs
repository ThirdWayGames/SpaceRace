using NUnit.Framework;

[TestFixture]
public class ContactEffectFixture
{
    [Test]
    public void OverlappingHazardRequestsAnthraxAndNoBoarder()
    {
        var hidden = Contact("radiation", ContactKind.Hazard, 0f, true, false);
        var effect = ContactEffectTable.Resolve(hidden);

        Assert.AreEqual("radiation", effect.ContactId);
        Assert.AreEqual("Anthrax", effect.PathogenResourceName);
        Assert.IsFalse(effect.SpawnBoarder);
        Assert.IsFalse(effect.RaiseAlarm);
        Assert.AreEqual(0f, effect.HullDrainPerSecond);

        var revealed = Contact("radiation", ContactKind.Hazard, 0f, true, true);
        var revealedEffect = ContactEffectTable.Resolve(revealed);
        Assert.AreEqual("Anthrax", revealedEffect.PathogenResourceName);
        Assert.IsFalse(revealedEffect.SpawnBoarder);
    }

    [Test]
    public void OverlappingLivingHostileRequestsAlarmAndBoarder()
    {
        var contact = Contact("raider", ContactKind.Hostile, 30f, true, false);
        var effect = ContactEffectTable.Resolve(contact);

        Assert.AreEqual("raider", effect.ContactId);
        Assert.IsTrue(effect.RaiseAlarm);
        Assert.IsTrue(effect.SpawnBoarder);
        Assert.AreEqual(string.Empty, effect.PathogenResourceName);
        Assert.AreEqual(0f, effect.HullDrainPerSecond);

        contact.Revealed = true;
        var revealed = ContactEffectTable.Resolve(contact);
        Assert.IsTrue(revealed.RaiseAlarm);
        Assert.IsTrue(revealed.SpawnBoarder);
    }

    [Test]
    public void OverlappingDeadHostileRequestsNeither()
    {
        var dead = Contact("raider", ContactKind.Hostile, 0f, true, true);
        AssertNothing(ContactEffectTable.Resolve(dead));

        var destroyed = Contact("raider", ContactKind.Hostile, -1f, true, false);
        AssertNothing(ContactEffectTable.Resolve(destroyed));
    }

    [Test]
    public void NonOverlappingContactRequestsNothing()
    {
        var hazard = Contact("radiation", ContactKind.Hazard, 0f, false, true);
        AssertNothing(ContactEffectTable.Resolve(hazard));

        var hostile = Contact("raider", ContactKind.Hostile, 30f, false, false);
        AssertNothing(ContactEffectTable.Resolve(hostile));
    }

    [Test]
    public void SecondResolveDoesNotRequestAnotherBoarder()
    {
        var contact = Contact("raider", ContactKind.Hostile, 30f, true, true);
        var effect = ContactEffectTable.Resolve(contact);
        Assert.IsTrue(effect.SpawnBoarder);

        var visits = new BoarderVisits();
        Assert.IsTrue(visits.TryBeginVisit(effect.ContactId));

        var again = ContactEffectTable.Resolve(contact);
        Assert.IsTrue(again.SpawnBoarder);
        Assert.IsFalse(visits.TryBeginVisit(again.ContactId));
        Assert.IsTrue(visits.TryBeginVisit("other-raider"));

        visits.EndVisit(effect.ContactId);
        Assert.IsTrue(visits.TryBeginVisit(effect.ContactId));
    }

    static ContactState Contact(string id, ContactKind kind, float health, bool overlapping, bool revealed)
    {
        var contact = new ContactState();
        contact.Id = id;
        contact.Kind = kind;
        contact.Health = health;
        contact.Overlapping = overlapping;
        contact.Revealed = revealed;
        return contact;
    }

    static void AssertNothing(ContactEffect effect)
    {
        Assert.IsFalse(effect.RaiseAlarm);
        Assert.IsFalse(effect.SpawnBoarder);
        Assert.AreEqual(string.Empty, effect.PathogenResourceName);
        Assert.AreEqual(0f, effect.HullDrainPerSecond);
    }
}
