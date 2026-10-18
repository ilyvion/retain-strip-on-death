using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class SettingsPersistenceTests
{
    private bool alsoOutsideHomeAreaBefore;
    private string path = "";

    [SetUp]
    public void SetUp()
    {
        alsoOutsideHomeAreaBefore = Settings.AlsoOutsideHomeArea;
        path = Path.GetTempFileName();
    }

    [TearDown]
    public void TearDown()
    {
        if (Scribe.mode != LoadSaveMode.Inactive)
        {
            Scribe.ForceStop();
        }
        File.Delete(path);
        Settings.AlsoOutsideHomeArea = alsoOutsideHomeAreaBefore;
    }

    private void Save()
    {
        Scribe.saver.InitSaving(path, "SettingsBlock");
        new Settings().ExposeData();
        Scribe.saver.FinalizeSaving();
    }

    private void Load()
    {
        Scribe.loader.InitLoading(path);
        new Settings().ExposeData();
        Scribe.loader.FinalizeLoading();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SettingSurvivesARoundTrip(bool value)
    {
        Settings.AlsoOutsideHomeArea = value;
        Save();
        Settings.AlsoOutsideHomeArea = !value;

        Load();

        Expect.AreEqual(value, Settings.AlsoOutsideHomeArea);
    }

    [Test]
    public void MissingSettingDefaultsToRetainingStripEverywhere()
    {
        File.WriteAllText(path, "<SettingsBlock></SettingsBlock>");
        Settings.AlsoOutsideHomeArea = false;

        Load();

        Expect.IsTrue(Settings.AlsoOutsideHomeArea);
    }

    // Players' saved settings use this key, so renaming it would reset them.
    [Test]
    public void SettingIsReadFromItsSavedKey()
    {
        File.WriteAllText(
            path,
            "<SettingsBlock><alsoOutsideHomeArea>False</alsoOutsideHomeArea></SettingsBlock>"
        );
        Settings.AlsoOutsideHomeArea = true;

        Load();

        Expect.IsFalse(Settings.AlsoOutsideHomeArea);
    }
}
