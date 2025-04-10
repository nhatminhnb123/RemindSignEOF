using System;
using System.ServiceProcess;
using System.Timers;
using WindowsService1.DBConnection;
using WindowsService1.Model;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private static Timer timer;
        private static GetData dto;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            dto = new GetData();
            timer = new Timer(60000);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();

            dto = null;
        }

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var ListRemindEmail = dto.GetAllRemindEmail<RawReminder>();
            foreach (var rs in ListRemindEmail)
            {
                if (ShouldSendEmail(rs.RemindTimer, rs.NumberOfTimesSent, rs.DocumentsSignTypeId))
                {
                    var dataPrepare = dto.PrePareData<RawData>(rs.DocumentsSignTypeId)[0];
                    var listUserOnDuty = dto.GetListUserSignNextAction<RawEmail>(dataPrepare.DocId);
                    if (listUserOnDuty != null && listUserOnDuty.Count > 0)
                    {
                        foreach (var userOnDurty in listUserOnDuty)
                        {
                            if (userOnDurty.IsNextStepEmail == 1 && !string.IsNullOrEmpty(userOnDurty.Email) && userOnDurty != null)
                            {
                                if (dto.InsertToEmailQueues(
                                    dataPrepare.DocId,
                                    userOnDurty.Email,
                                    CreateEmailSubject(dataPrepare.DocSetCode, dataPrepare.Title),
                                    CreateEmailBody(userOnDurty.FullName, dataPrepare.DocSetCode, dataPrepare.Title, dataPrepare.DepartName, dataPrepare.CreateDate)))
                                {
                                    dto.UpdateTimesRemind(rs.RemindEmailId);
                                }
                            }
                        }

                    }
                }
            }
        }

        private static bool ShouldSendEmail(DateTime? dateTime, Int64 numberOfTimesSent, Int64 docSignTypeId)
        {

            DateTime now = DateTime.Now;
            DateTime rawNow = now.AddMinutes(-1); // 1 phút trước
            if (dateTime >= rawNow && numberOfTimesSent == 0)
            {
                return dto.CheckNotSignAndUpdateStatusId(docSignTypeId);
            }
            return false;
        }

        private static string CreateEmailSubject(string docSetCode, string docTitle)
        {
            return $@"Nhắc Ký: Bộ hồ sơ [" + docSetCode + "] - " + docTitle;
        }

        private static string CreateEmailBody(string FullName, string DocSetCode, string Title, string DepartName, string CreateDate)
        {
            return $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .email-container {{
            width: 100%;
            background-color: #f5f5f5;
            padding: 20px;
        }}
        .email-content {{
            max-width: 600px;
            margin: auto;
            background-color: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header, .footer {{
            text-align: center;
            padding: 10px 0;
        }}
        .header img {{
            max-width: 100px;
        }}
        .footer {{
            font-size: 12px;
            color: #999;
        }}
        .email-body {{
            margin: 20px 0;
        }}
        .email-body p {{
            margin: 10px 0;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-content'>
            <div class='header'>
                <img src='http://cskh.sctv.vn/Content/images/logo_sctv.png' alt='SCTV E-OFFICE' />
                <h2>Nhắc nhở ký bộ hồ sơ</h2>
            </div>
            <div class='email-body'>
                <p>Kính gửi: {FullName},</p>
                <p>Bạn có một bộ hồ sơ cần ký với thông tin như sau:</p>
                <ul>
                    <li><strong>Mã hồ sơ:</strong> {DocSetCode}</li>
                    <li><strong>Tiêu đề:</strong> {Title}</li>
                    <li><strong>Đơn vị tạo:</strong> {DepartName}</li>
                    <li><strong>Thời điểm tạo:</strong> {CreateDate}</li>
                </ul>
                <p>Vui lòng truy cập vào hệ thống để thực hiện việc ký hồ sơ.</p>
                <p>Trân trọng,</p>
                <p>Phòng QLPMUD</p>
            </div>
            <div class='footer'>
                <p>Email này được gửi từ hệ thống SCTV E-OFFICE - P.QLPMUD & LTDL</p>
            </div>
        </div>
    </div>
</body>
</html>
";
        }
        //private static void WriteLog(string message)
        //{
        //    string logFilePath = "C:\\Users\\Minh\\Desktop\\MyServiceLog.txt";
        //    using (var writer = new System.IO.StreamWriter(logFilePath, true))
        //    {
        //        writer.WriteLine(message);
        //    }
        //}

    }
}
