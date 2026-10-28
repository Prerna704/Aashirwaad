using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using QRCoder;
using Aashirwad.Models;

namespace Aashirwad.Services
{
    /// <summary>
    /// PDF generation service — equivalent to Python's reportlab code in admin.py
    /// Uses explicit PdfFont objects for bold/normal styling instead of fluent SetBold()
    /// which was removed in itext7 v9+
    /// </summary>
    public static class PdfService
    {
        /// <summary>
        /// Generates a Patient Discharge Summary PDF.
        /// Equivalent to the PDF generation block in Python's /admin/discharge POST route.
        /// </summary>
        public static byte[] GenerateDischargeSummaryPdf(DischargedPatient patient, string hospitalName)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdfDoc = new PdfDocument(writer);
            var document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(36, 36, 54, 36);

            // ── Fonts ─────────────────────────────────────────────────────────────
            // Equivalent to: FontFactory.GetFont / styles['Title'] / styles['Normal'] in reportlab
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // ── Title ────────────────────────────────────────────────────────────
            // Equivalent to: elements.append(Paragraph(f"<b>{hospital_name_patient}</b>", styles['Title']))
            document.Add(new Paragraph(hospitalName).SetFont(boldFont).SetFontSize(18));
            document.Add(new Paragraph(" ").SetFont(normalFont));
            document.Add(new Paragraph("Patient Discharge Summary").SetFont(boldFont).SetFontSize(14));
            document.Add(new Paragraph(" ").SetFont(normalFont));

            // ── Patient Details Table ─────────────────────────────────────────────
            // Equivalent to: Table(table_data, colWidths=[180, 300]) with dark blue header cells
            var table = new Table(new float[] { 180, 300 }).UseAllAvailableWidth();

            void AddRow(string key, string value)
            {
                var keyCell = new Cell()
                    .Add(new Paragraph(key).SetFont(boldFont).SetFontSize(10)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(new DeviceRgb(0, 0, 139))
                    .SetPadding(6);
                table.AddCell(keyCell);

                var valCell = new Cell()
                    .Add(new Paragraph(value ?? "").SetFont(normalFont).SetFontSize(10))
                    .SetPadding(6);
                table.AddCell(valCell);
            }

            AddRow("Patient ID", patient.PatientId);
            AddRow("Patient Name", patient.PatientName);
            AddRow("Admission Date", patient.AdmissionDate);
            AddRow("Discharge Date", patient.DischargeDate);
            AddRow("Diagnosis", patient.Diagnosis);
            AddRow("Treatment", patient.Treatment);
            AddRow("Doctor Name", patient.DoctorName);
            AddRow("Discharge Summary", patient.DischargeSummary);
            AddRow("Follow-Up Instructions", patient.FollowUpInstructions);
            AddRow("Medications", patient.Medications);
            AddRow("Contact Info", patient.ContactInfo);
            AddRow("Gender", patient.Gender);
            AddRow("Address", patient.Address);
            AddRow("Bed Type", patient.BedType);

            document.Add(table);
            document.Add(new Paragraph(" ").SetFont(normalFont));

            // ── Health Tips ──────────────────────────────────────────────────────
            // Equivalent to: for tip in health_tips: elements.append(Paragraph(tip, styles['Normal']))
            document.Add(new Paragraph("Health & Wellness Tips for Recovery:")
                .SetFont(boldFont).SetFontSize(12));

            var tips = new[]
            {
                "Drink Plenty of Water - Stay hydrated to help your body recover faster.",
                "Take a Bath Daily - Maintain good hygiene to prevent infections.",
                "Eat an Apple Every Day - 'An apple a day keeps the doctor away!'",
                "Consume a Balanced Diet - Include proteins, vitamins, and minerals.",
                "Avoid Junk Food - Say NO to excessive sugar, salt, and oily foods.",
                "Take Your Medicines on Time - Follow the prescribed dosage carefully.",
                "Get Enough Rest & Sleep - Allow your body to heal and regain energy.",
                "Avoid Smoking & Alcohol - These slow down recovery and harm your health.",
                "Do Light Exercise - Gentle movements help in faster recovery.",
                "Keep Your Surroundings Clean - Prevent infections and maintain hygiene.",
                "Regularly Change Wound Dressings - If applicable, as per doctor's advice.",
                "Follow Your Doctor's Instructions - Always stick to medical advice.",
                "Keep Emergency Contacts Handy - Save the hospital and doctor's numbers.",
                "Wash Hands Frequently - Avoid germs and stay safe.",
                "Monitor Your Symptoms - Report any unusual pain, fever, or discomfort.",
                "Attend All Follow-Up Appointments - Ensure complete recovery.",
                "Stay Positive & Stress-Free - Mental health is just as important."
            };

            foreach (var tip in tips)
                document.Add(new Paragraph($"* {tip}").SetFont(normalFont).SetFontSize(10));

            document.Add(new Paragraph(" ").SetFont(normalFont));

            // ── QR Code ──────────────────────────────────────────────────────────
            // Equivalent to: qrcode.QRCode(...) in Python's admin.py
            var qrData = string.Join("\n", new[]
            {
                $"Patient ID: {patient.PatientId}",
                $"Patient Name: {patient.PatientName}",
                $"Diagnosis: {patient.Diagnosis}",
                $"Doctor: {patient.DoctorName}"
            });

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.L);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(10);

            document.Add(new Paragraph("Scan QR for Full Details:").SetFont(boldFont).SetFontSize(12));
            document.Add(new Paragraph(" ").SetFont(normalFont));

            // Embed QR image — equivalent to Python's Image(qr_buffer, width=100, height=100)
            var qrImageData = ImageDataFactory.Create(qrBytes);
            var qrImage = new Image(qrImageData).SetWidth(100).SetHeight(100);
            document.Add(qrImage);

            document.Add(new Paragraph(" ").SetFont(normalFont));

            // ── Footer ───────────────────────────────────────────────────────────
            document.Add(new Paragraph(
                "Thank you for choosing our hospital. Wishing you a speedy recovery!")
                .SetFont(normalFont).SetFontSize(10));
            document.Add(new Paragraph("Hospital Address: Aashirwad Hospital, Healthcare Avenue")
                .SetFont(normalFont).SetFontSize(10));
            document.Add(new Paragraph("Emergency Helpline: +91 9798680120 (Prerna Prem) | Email: prernaprem720@gmail.com")
                .SetFont(normalFont).SetFontSize(10));

            document.Close();
            return ms.ToArray();
        }
    }
}
