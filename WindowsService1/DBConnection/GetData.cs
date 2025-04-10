using DB.Framework.Common;
using DB.Framework.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace WindowsService1.DBConnection
{
    public class GetData : Base
    {
        public bool CheckNotSignAndUpdateStatusId(Int64 DocSignTypeId)
        {
            try
            {
                var listParams = new List<SqlParameter>();
                listParams.Add(new SqlParameter("@DocumentSignTypeId", SqlDbType.Int) { Value = DocSignTypeId });
                var listOutput = new Dictionary<string, object> { { "@Result", 0 } };
                using (var bll = new BllHelper(SCTVeSignConnectionString))
                {
                    var dt = bll.ExecNonQueryProc("spDocument_CheckIsSignAndUpdateStatusId", listParams, listOutput);
                    if (listOutput["@Result"].ToString() != "1") return false;
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }
        public void UpdateTimesRemind(Int64 RemindEmailId)
        {
            try
            {
                var listParams = new List<SqlParameter>();
                listParams.Add(new SqlParameter("@RemindEmailId", SqlDbType.Int) { Value = RemindEmailId });
                using (var bll = new BllHelper(SCTVeSignConnectionString))
                {
                    var dt = bll.ExecNonQueryProc("spRemindEmail_UpdateTimes", listParams);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public List<T> PrePareData<T>(Int64 DocumentsSignTypeId)
        {
            try
            {
                var listParams = new List<SqlParameter>();
                listParams.Add(new SqlParameter("@DocSignTypeId", SqlDbType.Int) { Value = DocumentsSignTypeId });
                using (var bll = new BllHelper(SCTVeSignConnectionString))
                {
                    var dt = bll.GetTableStoreProc("spRemindEmail_PrePareData", listParams);
                    if (dt != null && dt.Rows.Count > 0) return dt.ToList<T>();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public List<T> GetAllRemindEmail<T>()
        {
            try
            {
                using (var bll = new BllHelper(SCTVeSignConnectionString))
                {
                    var dt = bll.GetTableStoreProc("spRemindEmail_GetAll");
                    if (dt != null && dt.Rows.Count > 0) return dt.ToList<T>();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public List<T> GetListUserSignNextAction<T>(Int64 docId)
        {
            try
            {
                var listParams = new List<SqlParameter>();
                listParams.Add(new SqlParameter("@DocId", SqlDbType.Int) { Value = docId });
                using (var bll = new BllHelper(SCTVeSignConnectionString))
                {
                    var dt = bll.GetTableStoreProc("spDocumentSet_GetListUserSignNextAction", listParams);
                    if (dt != null && dt.Rows.Count > 0) return dt.ToList<T>();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public bool InsertToEmailQueues(Int64 DocId, string ToEmails, string Subject, string body)
        {
            try
            {
                var listParams = new List<SqlParameter>
                {
                    new SqlParameter("@DocId", SqlDbType.BigInt) {Value = DocId},
                    new SqlParameter("@DocLogId", SqlDbType.BigInt) {Value = DBNull.Value},
                    new SqlParameter("@ToEmails", SqlDbType.NVarChar, 300) {Value = ToEmails},
                    new SqlParameter("@CcEmails", SqlDbType.NVarChar, 300) {Value = DBNull.Value},
                    new SqlParameter("@BccEmails", SqlDbType.NVarChar, 300) {Value = DBNull.Value},
                    new SqlParameter("@Subject", SqlDbType.NVarChar, 300) {Value = Subject},
                    new SqlParameter("@Body", SqlDbType.NVarChar, int.MaxValue){Value = body},
                    new SqlParameter("@GUIID", SqlDbType.UniqueIdentifier) {Value = DBNull.Value},
                    new SqlParameter("@Note", SqlDbType.NVarChar) {Value = DBNull.Value}
                };
                //listParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = model.Id });

                var listOutput = new Dictionary<string, object> { { "@Result", 0 } };

                using (var bll = new BllHelper(this.SCTVeSignConnectionString))
                {
                    var ketQua = bll.ExecNonQueryProc("spEmailSendQueues_Insert", listParams, listOutput);

                    if (ketQua == 0 && listOutput["@Result"].ToString() != "0")
                    {
                        return Int64.Parse(listOutput["@Result"].ToString()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}
