using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SwissLohnSystem.API.DTOs.Lohn;

namespace SwissLohnSystem.API.Documents
{
    public sealed class LohnSlipPdfDocument : IDocument
    {
        private readonly LohnDetailsDto _d;

        public LohnSlipPdfDocument(LohnDetailsDto dto)
        {
            _d = dto;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("Lohnabrechnung").Bold().FontSize(16);

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text($"{_d.EmployeeName} – {_d.Month:D2}/{_d.Year}");
                    col.Item().Text($"Brutto: {_d.BruttoSalary:N2} CHF");
                    col.Item().Text($"Netto: {_d.NetSalary:N2} CHF").Bold();

                    col.Item().LineHorizontal(1);

                    col.Item().Text("Abzüge Arbeitnehmer").Bold();
                    AddLine(col, "AHV/IV/EO", _d.EmployeeAhvIvEo);
                    AddLine(col, "ALV", _d.EmployeeAlv1 + _d.EmployeeAlv2);
                    AddLine(col, "NBU", _d.EmployeeNbu);
                    AddLine(col, "BVG", _d.EmployeeBvg);
                    AddLine(col, "KTG", _d.EmployeeKtg);
                    AddLine(col, "Quellensteuer", _d.EmployeeQst);

                    col.Item().LineHorizontal(1);

                    col.Item().Text("Arbeitgeber Beiträge").Bold();
                    AddLine(col, "AHV/IV/EO", _d.EmployerAhvIvEo);
                    AddLine(col, "ALV", _d.EmployerAlv1 + _d.EmployerAlv2);
                    AddLine(col, "BU", _d.EmployerBu);
                    AddLine(col, "BVG", _d.EmployerBvg);
                    AddLine(col, "KTG", _d.EmployerKtg);
                    AddLine(col, "FAK", _d.EmployerFak);
                    AddLine(col, "VK", _d.EmployerVk);
                });
            });
        }

        private static void AddLine(ColumnDescriptor col, string label, decimal value)
        {
            if (value == 0m) return;

            col.Item().Row(r =>
            {
                r.RelativeItem().Text(label);
                r.ConstantItem(100).AlignRight().Text($"{value:N2} CHF");
            });
        }
    }
}
