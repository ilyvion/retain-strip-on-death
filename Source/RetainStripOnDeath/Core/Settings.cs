using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RetainStripOnDeath
{
    public class Settings : ModSettings
    {
        private static bool _alsoOutsideHomeArea = true;

        internal static bool AlsoOutsideHomeArea { get => _alsoOutsideHomeArea; set => _alsoOutsideHomeArea = value; }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _alsoOutsideHomeArea, "alsoOutsideHomeArea", true);
        }

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled(
                "RetainStripOnDeath.AlsoOutsideHomeAreaLabel".Translate(),
                ref _alsoOutsideHomeArea,
                "RetainStripOnDeath.AlsoOutsideHomeAreaToolTip".Translate());

            listingStandard.End();
        }
    }
}
