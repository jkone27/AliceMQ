using System.Text;

namespace AliceMQ.MailBox.Core
{
    public class DeliveryContent : IDeliveryContext
    {
        public DeliveryContent(string encoding, byte[] raw)
        {
            Encoding = Encoding.GetEncoding(encoding);
            Raw = raw;
        }

        public DeliveryContent(Encoding encoding, byte[] raw)
        {
            Encoding = encoding;
            Raw = raw;
        }

        public Encoding Encoding { get; }
        public byte[] Raw { get; }
        public string Payload => Encoding.GetString(Raw);
    }
}