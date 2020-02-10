namespace ChannelsExample
{
    public class Envelope
    {
        public Envelope(string payload)
        {
            Payload = payload;
        }

        public string Payload { get; }
    }
}