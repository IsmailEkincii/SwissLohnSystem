// SwissLohnSystem.UI/Services/Lookups/QstUiLookups.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SwissLohnSystem.UI.Services.Lookups
{
    public static class QstUiLookups
    {
        public static List<SelectListItem> GetPermitTypes()
        {
            return new List<SelectListItem>
            {
                new("B – Aufenthaltsbewilligung", "B"),
                new("C – Niederlassungsbewilligung", "C"),
                new("L – Kurzaufenthaltsbewilligung", "L"),
                new("G – Grenzgängerbewilligung", "G"),
                new("F – Vorläufig aufgenommen", "F"),
                new("N – Asylsuchende", "N")
            };
        }

        public static List<SelectListItem> GetQstTariffCodes()
        {
            return new List<SelectListItem>
            {
                new("-- Bitte wählen --", ""),
                new("A0 – ledig, 1 Einkommen, keine Kinder", "A0"),
                new("A1 – ledig, 1 Einkommen, 1 Kind", "A1"),
                new("B0 – verheiratet, 2 Einkommen", "B0"),
                new("C0 – verheiratet, 1 Einkommen", "C0"),
                new("H – Alleinerziehende", "H")
            };
        }
    }
}
