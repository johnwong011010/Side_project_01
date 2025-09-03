namespace OTP.Model
{
    public interface Iotp
    {
        public string Issuer { get; set; }
        public string Label { get; set; }
        public string Secret { get; set; }
        public string GenQRcodeURL();
        public byte[] GenQRcode();
    }
}
