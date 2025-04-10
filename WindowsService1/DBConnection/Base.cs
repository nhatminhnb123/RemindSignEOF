using DB.Framework.Common;
using DB.Framework.SQL;

namespace WindowsService1.DBConnection
{
    public class Base 
    {
        public string SCTVeSignConnectionString
        {
            get
            {
                string connString = WebSettings.GetValueFromAppSettings("SCTVeSignConnectionString");
                string saltKey = WebSettings.GetValueFromAppSettings("SaltKey");

                return ConnectionHelper.CreateConnectionStringDecryptPassword(connString, saltKey);
            }
        }
    }
}
