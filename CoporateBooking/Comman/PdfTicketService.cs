namespace CoporateBooking.Comman
{
    using PdfSharpCore.Drawing;
    using PdfSharpCore.Pdf;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text;
    using System.Drawing;
    using System.Xml.Linq;

    public class PdfTicketService
    {
        public byte[] GeneratePdfFromJson(string jsonString)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            JsonNode root = JsonNode.Parse(jsonString);
            var data = root?["data"];

            using var stream = new MemoryStream();
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var fontHeader = new XFont("Arial", 14, XFontStyle.Bold);
            var fontBody = new XFont("Arial", 11, XFontStyle.Regular);

            double y = 40;
            double lineHeight = 20;

            void WriteLine(string text, XFont? font = null)
            {
                gfx.DrawString(text, font ?? fontBody, XBrushes.Black, new XRect(40, y, page.Width - 80, lineHeight), XStringFormats.TopLeft);
                y += lineHeight;
            }
            WriteLine("Air India Express E-Ticket Summary", fontHeader);
            y += 10;

            WriteLine("Booking Overview", fontHeader);
            WriteLine($"PNR: {data?["recordLocator"]}");
            WriteLine($"Booking Date: {data?["info"]?["bookedDate"]}");

            var passengers = data?["passengers"]?.AsObject();
            if (passengers != null)
            {
                WriteLine("Passengers:");
                foreach (var kv in passengers)
                {
                    var name = kv.Value?["name"];
                    WriteLine($"- {name?["first"]} {name?["last"]}");
                }
            }

            y += 10;
            WriteLine("Fare Summary", fontHeader);
            WriteLine($"Total: ₹{data?["breakdown"]?["totalAmount"]}");

            y += 10;
            WriteLine("Contact", fontHeader);
            var contact = data?["contacts"]?["G"];
            WriteLine($"Name: {contact?["name"]?["first"]} {contact?["name"]?["last"]}");
            WriteLine($"Email: {contact?["emailAddress"]}");
            WriteLine($"Phone: {contact?["phoneNumbers"]?[0]?["number"]}");

            doc.Save(stream);
            return stream.ToArray();
        }
    }
}
