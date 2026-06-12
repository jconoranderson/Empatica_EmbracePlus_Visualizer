using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BioSignalVisualizer.Server.Controllers;

[ApiController]
[Route("api/export")]
public sealed class ExportController : ControllerBase
{
    private readonly PdfExporter _pdfExporter;

    public ExportController(PdfExporter pdfExporter)
    {
        _pdfExporter = pdfExporter;
    }

    [HttpPost]
    public IActionResult Create([FromBody] PdfExportRequest request)
    {
        var pdfBytes = _pdfExporter.CreateSummaryPdf(request);
        return File(pdfBytes, "application/pdf", $"{request.Participant}-{request.Date}.pdf");
    }
}
