using System.Globalization;
using ClosedXML.Excel;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using SGCP.Api.Patrones.Reportes;

namespace SGCP.Api.Servicios;

public sealed class FuenteReportes : IFontResolver
{
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) => new("SGCP-DejaVu");
    public byte[] GetFont(string faceName)
    {
        using var fuente = typeof(FuenteReportes).Assembly.GetManifestResourceStream("SGCP.Api.Recursos.DejaVuSans.ttf")
            ?? throw new InvalidOperationException("Falta la fuente de los reportes.");
        using var salida = new MemoryStream();
        fuente.CopyTo(salida);
        return salida.ToArray();
    }
}

public class ExportadorReportes
{
    static ExportadorReportes() => GlobalFontSettings.FontResolver = new FuenteReportes();

    public byte[] Excel(TablaReporte tabla)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Reporte");
        for (var col = 0; col < tabla.Columnas.Length; col++)
            hoja.Cell(1, col + 1).Value = tabla.Columnas[col];
        for (var fila = 0; fila < tabla.Filas.Count; fila++)
            for (var col = 0; col < tabla.Columnas.Length; col++)
            {
                var celda = hoja.Cell(fila + 2, col + 1);
                var valor = tabla.Filas[fila][col];
                if (valor is decimal numero)
                {
                    celda.Value = (double)numero;
                    celda.Style.NumberFormat.Format = tabla.Columnas[col].EndsWith("Bs") ? "0.00" : "0.000";
                }
                else if (valor is int entero)
                    celda.Value = entero;
                else if (valor is bool booleano)
                    celda.Value = booleano;
                else if (valor is DateTime fecha)
                {
                    celda.Value = fecha;
                    celda.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                }
                else
                    celda.Value = valor?.ToString() ?? "";
            }
        hoja.Row(1).Style.Font.Bold = true;
        hoja.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("224C3D");
        hoja.Row(1).Style.Font.FontColor = XLColor.White;
        hoja.Columns(1, tabla.Columnas.Length).Width = 24;
        hoja.Columns(1, tabla.Columnas.Length).Style.Alignment.WrapText = true;
        hoja.SheetView.FreezeRows(1);
        hoja.Range(1, 1, Math.Max(1, tabla.Filas.Count + 1), tabla.Columnas.Length).SetAutoFilter();
        using var salida = new MemoryStream();
        libro.SaveAs(salida);
        return salida.ToArray();
    }

    public byte[] Pdf(TablaReporte tabla, IReadOnlyList<(string Tipo, byte[] Contenido)>? evidencias = null)
    {
        using var doc = new PdfDocument();
        doc.Info.Title = tabla.Titulo;
        doc.Info.Author = "SGCP";
        var fuente = new XFont("SGCP", 8);
        var titulo = new XFont("SGCP", 15);
        XGraphics? g = null;
        double y = 0;
        double ancho = 0;
        void Pagina()
        {
            g?.Dispose();
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            ancho = page.Width.Point - 60;
            g = XGraphics.FromPdfPage(page);
            g.DrawString("SGCP · " + tabla.Titulo, titulo, XBrushes.DarkSlateGray, 30, 30);
            g.DrawString("Página " + doc.PageCount + " · Fechas en UTC · " + tabla.Filas.Count + " registros", fuente, XBrushes.Gray, 30, 47);
            y = 65;
            Fila(tabla.Columnas, true);
        }
        void Fila(string[] valores, bool cabecera = false)
        {
            var anchoCol = ancho / valores.Length;
            var lineas = valores.Select(t => Partir(g!, fuente, t, anchoCol - 10)).ToArray();
            var alto = lineas.Max(x => x.Count) * 11 + 10;
            if (y + alto > 560 && !cabecera)
                Pagina();
            for (var col = 0; col < valores.Length; col++)
            {
                var x = 30 + col * anchoCol;
                g!.DrawRectangle(cabecera ? XBrushes.LightGray : XBrushes.White, x, y, anchoCol, alto);
                g.DrawRectangle(XPens.LightGray, x, y, anchoCol, alto);
                for (var linea = 0; linea < lineas[col].Count; linea++)
                    g.DrawString(lineas[col][linea], fuente, XBrushes.Black, x + 5, y + 12 + linea * 11);
            }
            y += alto;
        }
        Pagina();
        foreach (var fila in tabla.Filas)
            Fila(fila.Select(Texto).ToArray());
        g?.Dispose();
        if (evidencias is not null)
            foreach (var evidencia in evidencias)
            {
                var page = doc.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                using var graficos = XGraphics.FromPdfPage(page);
                graficos.DrawString("Evidencia de recepción · SGCP", titulo, XBrushes.DarkSlateGray, 30, 35);
                using var datos = new MemoryStream(evidencia.Contenido);
                using var imagen = XImage.FromStream(datos);
                var escala = Math.Min((page.Width.Point - 60) / imagen.PointWidth, (page.Height.Point - 100) / imagen.PointHeight);
                graficos.DrawImage(imagen, 30, 65, imagen.PointWidth * escala, imagen.PointHeight * escala);
            }
        using var salida = new MemoryStream();
        doc.Save(salida, false);
        return salida.ToArray();
    }

    private static List<string> Partir(XGraphics g, XFont fuente, string texto, double ancho)
    {
        var lineas = new List<string>();
        var actual = "";
        foreach (var c in texto.Replace("\r", ""))
        {
            if (c == '\n' || g.MeasureString(actual + c, fuente).Width > ancho)
            {
                lineas.Add(actual);
                actual = c == '\n' ? "" : c.ToString();
            }
            else
                actual += c;
        }
        lineas.Add(actual);
        return lineas;
    }
    private static string Texto(object? valor) => valor switch
    {
        DateTime fecha => fecha.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        decimal numero => numero.ToString("0.###", CultureInfo.GetCultureInfo("es-BO")),
        bool b => b ? "Sí" : "No",
        _ => valor?.ToString() ?? "—"
    };
}
