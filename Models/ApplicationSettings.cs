namespace FTdx101MP_WebApp.Models
{
    public class ApplicationSettings
    {
        public string SerialPort { get; set; } = "COM3";
        public int BaudRate { get; set; } = 38400;
        public string WebAddress { get; set; } = "localhost";
        public int WebPort { get; set; } = 5000;
    }
}