using System;

namespace WindowsService1.Model
{
    class RawReminder
    {
        public Int64 RemindEmailId { get; set; }
        public Int64 DocumentsSignTypeId { get; set; }
        public DateTime? RemindTimer { get; set; }
        public Int64 NumberOfTimesSent { get; set; }
        public Int64 SignProcId { get; set; }
        public string StatusId { get; set; }
    }
}
