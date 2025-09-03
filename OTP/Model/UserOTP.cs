using QRCoder;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using OtpNet;

namespace OTP.Model
{
    public class UserOTP : Iotp
    {
        public string Issuer { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Secret { get; set; } = null!;
        //Generate QR code URL
        public string GenQRcodeURL() =>
            $"otpauth://totp/{Label}?issuer={Uri.EscapeDataString(Issuer)}&secret={Uri.EscapeDataString(Secret)}";
        //因為.net在6.0把System.Drawing裡面的大多class移除了
        //所以這邊要用QRCoder裡面的PngByteQRCode來產生QR code
        public byte[] GenQRcode()
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(GenQRcodeURL(), QRCodeGenerator.ECCLevel.Q);
            var qrc = new PngByteQRCode(qrCodeData);
            return qrc.GetGraphic(20);
        }
    }
}
