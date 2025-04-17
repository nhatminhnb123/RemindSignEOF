using System;
using System.ServiceProcess;
using System.Timers;
using WindowsService1.DBConnection;
using WindowsService1.Model;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private GetData dto;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log("Service starting...");

                dto = new GetData();
                timer = new Timer(60000); // 60 giây
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;

                Log("Service started.");
            }
            catch (Exception ex)
            {
                Log("Error in OnStart: " + ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                Log("Service stopping...");
                timer?.Stop();
                timer?.Dispose();
                Log("Service stopped.");
            }
            catch (Exception ex)
            {
                Log("Error in OnStop: " + ex);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false; // Ngăn overlap
                Log("Timer triggered.");

                if (dto == null)
                {
                    Log("dto is null. Skipping OnTimedEvent.");
                    return;
                }

                var reminders = dto.GetAllRemindEmail<RawReminder>();

                if (reminders == null || reminders.Count == 0)
                {
                    Log("No reminders found — skip.");
                    return;
                }

                foreach (var rs in reminders)
                {
                    if (!ShouldSendEmail(rs.RemindTimer, rs.NumberOfTimesSent, rs.DocumentsSignTypeId))
                        continue;

                    var prepared = dto.PrePareData<RawData>(rs.DocumentsSignTypeId);
                    if (prepared == null || prepared.Count == 0)
                        continue;

                    var data = prepared[0];
                    var users = dto.GetListUserSignNextAction<RawEmail>(data.DocId);

                    if (users == null || users.Count == 0)
                        continue;

                    foreach (var user in users)
                    {
                        if (user == null || user.IsNextStepEmail != 1 || string.IsNullOrEmpty(user.Email))
                            continue;

                        bool isSent = dto.InsertToEmailQueues(
                            data.DocId,
                            user.Email,
                            CreateEmailSubject(data.DocSetCode, data.Title),
                            CreateEmailBody(user.FullName, data.DocSetCode, data.Title, data.DepartName, data.CreateDate));

                        if (isSent)
                        {
                            dto.UpdateTimesRemind(rs.RemindEmailId);
                            Log($"Email sent to: {user.Email}");
                        }
                    }
                }

                Log("Timer execution complete.");
            }
            catch (Exception ex)
            {
                Log("Error in OnTimedEvent: " + ex);
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        // NẾU CHƯA KÝ THÌ RETURN: TRUE
        private bool ShouldSendEmail(DateTime? dateTime, long numberOfTimesSent, long docSignTypeId)
        {
            DateTime now = DateTime.Now;
            DateTime threshold = now.AddMinutes(-1);

            if (dateTime.HasValue && dateTime.Value >= threshold && numberOfTimesSent == 0)
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
        private void Log(string message)
        {
            string path = @"C:\RemindEmailEOF\Service_log.txt";
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                System.IO.File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { /* Không được ghi log cũng không crash */ }
        }

    }
}
