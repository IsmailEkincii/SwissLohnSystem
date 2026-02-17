using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SwissLohnSystem.API.DTOs.Lohn;

namespace SwissLohnSystem.API.Documents
{
    public sealed class LohnausweisPdfDocument : IDocument
    {
        private readonly LohnausweisDto _d;

        public LohnausweisPdfDocument(LohnausweisDto dto) => _d = dto;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(h =>
                {
                    h.Item().Text("Lohnausweis").Bold().FontSize(16);
                    h.Item().Text($"Jahr: {_d.Year}").FontSize(11);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text($"Arbeitnehmer: {_d.EmployeeName}");
                    col.Item().Text($"Adresse: {_d.EmployeeAddress} {_d.EmployeeZip} {_d.EmployeeCity}".Trim());

                    col.Item().Text($"Arbeitgeber: {_d.CompanyName}");
                    if (!string.IsNullOrWhiteSpace(_d.CompanyAddress))
                        col.Item().Text($"Adresse: {_d.CompanyAddress}");
                    if (!string.IsNullOrWhiteSpace(_d.CompanyPhone) || !string.IsNullOrWhiteSpace(_d.CompanyEmail))
                        col.Item().Text($"Kontakt: {_d.CompanyPhone} {_d.CompanyEmail}".Trim());

                    col.Item().LineHorizontal(1);

                    // Basit tablo: Form 11 alanları (core)
                    col.Item().Text("Werte (Jahressummen)").Bold();

                    AddRow(col, "8  Bruttolohn", _d.BruttoTotal_8);
                    AddRow(col, "2.2 Privatanteile / Privatanteil Geschäftsfahrzeug", _d.PrivateBenefit_2_2);

                    col.Item().Text("9  AHV/ALV/NBU (Arbeitnehmer)").Bold();
                    AddRow(col, "   AHV/IV/EO", _d.AhvIvEo_9);
                    AddRow(col, "   ALV (Total)", _d.AlvTotal_9);
                    AddRow(col, "   NBU", _d.Nbu_9);
                    AddRow(col, "   Total (9)", _d.SocialTotal_9);

                    AddRow(col, "10 Berufliche Vorsorge (BVG, AN)", _d.Bvg_10);
                    AddRow(col, "12 Quellensteuer", _d.Quellensteuer_12);

                    col.Item().Text("13 Spesen").Bold();
                    AddRow(col, "   Pauschalspesen", _d.PauschalSpesen_13);
                    AddRow(col, "   Effektivspesen", _d.EffektivSpesen_13);

                    AddRow(col, "11 Nettolohn (Summe NetSalary)", _d.NetTotal_11);

                    if (!string.IsNullOrWhiteSpace(_d.Remark_15))
                    {
                        col.Item().LineHorizontal(1);
                        col.Item().Text("15 Bemerkungen").Bold();
                        col.Item().Text(_d.Remark_15!);
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("SwissLohnSystem – Lohnausweis PDF");
                });
            });
        }

        private static void AddRow(ColumnDescriptor col, string label, decimal value)
        {
            col.Item().Row(r =>
            {
                r.RelativeItem().Text(label);
                r.ConstantItem(130).AlignRight().Text($"{value:N2} CHF");
            });
        }
    }
}
