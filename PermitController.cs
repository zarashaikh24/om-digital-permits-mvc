using CleanValveManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CleanValveManagement.Controllers
{
    public class PermitController : Controller
    {
        private readonly IConfiguration _configuration;

        public PermitController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Dashboard()
        {
            string? userType = HttpContext.Session.GetString("UserType");
            int? empId = HttpContext.Session.GetInt32("Emp_ID");

            if (string.IsNullOrEmpty(userType) || empId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserType = userType;

            bool canViewCP = userType.Contains("CP");
            bool canViewCNG = userType.Contains("CNG");
            bool canViewSMA = userType.Contains("SMA");

            DashboardViewModel dashboard = new DashboardViewModel();

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                if (canViewCP)
                {
                    SqlCommand cpCmd = new SqlCommand("sp_CP_web_ReportData", con);
                    cpCmd.CommandType = CommandType.StoredProcedure;
                    cpCmd.Parameters.AddWithValue("@FDate", DBNull.Value);
                    cpCmd.Parameters.AddWithValue("@TDate", DBNull.Value);
                    cpCmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString("D2"));
                    cpCmd.Parameters.AddWithValue("@status", DBNull.Value);

                    SqlDataReader cpReader = cpCmd.ExecuteReader();

                    while (cpReader.Read())
                    {
                        dashboard.CPPermits++;
                        dashboard.TotalPermits++;

                        string permitStatus = cpReader["status"]?.ToString()?.Trim().ToUpper() ?? "";

                        if (permitStatus == "REQUESTED")
                            dashboard.CPRequested++;
                        else if (permitStatus == "APPROVED")
                            dashboard.CPApproved++;
                        else if (permitStatus == "REJECTED")
                            dashboard.CPRejected++;
                        else if (permitStatus == "CLOSED")
                            dashboard.CPClosed++;
                    }

                    cpReader.Close();
                }

                if (canViewCNG)
                {
                    SqlCommand cngCmd = new SqlCommand("sp_CNG_web_ReportData", con);
                    cngCmd.CommandType = CommandType.StoredProcedure;
                    cngCmd.Parameters.AddWithValue("@FDate", DBNull.Value);
                    cngCmd.Parameters.AddWithValue("@TDate", DBNull.Value);
                    cngCmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString("D2"));
                    cngCmd.Parameters.AddWithValue("@status", DBNull.Value);

                    SqlDataReader cngReader = cngCmd.ExecuteReader();

                    while (cngReader.Read())
                    {
                        dashboard.CNGPermits++;
                        dashboard.TotalPermits++;

                        string permitStatus = cngReader["status"]?.ToString()?.Trim().ToUpper() ?? "";

                        if (permitStatus == "REQUESTED")
                            dashboard.CNGRequested++;
                        else if (permitStatus == "APPROVED")
                            dashboard.CNGApproved++;
                        else if (permitStatus == "REJECTED")
                            dashboard.CNGRejected++;
                        else if (permitStatus == "CLOSED")
                            dashboard.CNGClosed++;
                    }

                    cngReader.Close();
                }

                if (canViewSMA)
                {
                    string empIdNormal = empId.Value.ToString();
                    string empIdJson = "[\"" + empIdNormal + "\"]";

                    SqlCommand smaCmd = new SqlCommand(@"
                SELECT status
                FROM PermitData
                WHERE
                    LTRIM(RTRIM(CAST(Emp_id AS NVARCHAR(50)))) = @EmpId
                    OR
                    LTRIM(RTRIM(CAST(Emp_id AS NVARCHAR(50)))) = @EmpIdJson
            ", con);

                    smaCmd.Parameters.Add("@EmpId", SqlDbType.NVarChar, 50).Value = empIdNormal;
                    smaCmd.Parameters.Add("@EmpIdJson", SqlDbType.NVarChar, 50).Value = empIdJson;

                    SqlDataReader smaReader = smaCmd.ExecuteReader();

                    while (smaReader.Read())
                    {
                        dashboard.SMAPermits++;
                        dashboard.TotalPermits++;

                        string permitStatus = smaReader["status"] == DBNull.Value
                            ? ""
                            : smaReader["status"].ToString().Trim().ToUpper();

                        if (permitStatus == "REQUESTED")
                            dashboard.SMARequested++;
                        else if (permitStatus == "APPROVED")
                            dashboard.SMAApproved++;
                        else if (permitStatus == "REJECTED")
                            dashboard.SMARejected++;
                        else if (permitStatus == "CLOSED")
                            dashboard.SMAClosed++;
                    }

                    smaReader.Close();
                }
            }


            return View(dashboard);
        }

        public IActionResult CathodicProtection(string? status, DateTime? fromDate, DateTime? toDate, string? permitNumber)
        {
            List<CPPermitReport> permits = new List<CPPermitReport>();

            int? empId = HttpContext.Session.GetInt32("Emp_ID");

            if (empId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool isFilterApplied =
                !string.IsNullOrWhiteSpace(status) ||
                fromDate.HasValue ||
                toDate.HasValue ||
                !string.IsNullOrWhiteSpace(permitNumber);

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand empCmd = new SqlCommand(
                    "SELECT Name, Designation, Agency_name, Contact_No FROM Login_VC WHERE Emp_ID = @Emp_ID",
                    con);

                empCmd.Parameters.Add("@Emp_ID", SqlDbType.Int).Value = empId.Value;

                using (SqlDataReader empReader = empCmd.ExecuteReader())
                {
                    if (empReader.Read())
                    {
                        ViewBag.EmpName = empReader["Name"]?.ToString();
                        ViewBag.Designation = empReader["Designation"]?.ToString();
                        ViewBag.AgencyName = empReader["Agency_name"]?.ToString();
                        ViewBag.ContactNo = empReader["Contact_No"]?.ToString();
                        ViewBag.EmpId = empId.Value;
                        ViewBag.LoginTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
                    }
                }

                SqlCommand cmd = new SqlCommand("sp_CP_web_ReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;

                cmd.Parameters.AddWithValue("@FDate", fromDate.HasValue ? fromDate.Value.ToString("dd-MM-yyyy") : DBNull.Value);
                cmd.Parameters.AddWithValue("@TDate", toDate.HasValue ? toDate.Value.ToString("dd-MM-yyyy") : DBNull.Value);
                cmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString("D2"));
                cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int permitId = Convert.ToInt32(reader["id"]);

                        if (!string.IsNullOrWhiteSpace(permitNumber) &&
                            permitId.ToString() != permitNumber.Trim())
                        {
                            continue;
                        }

                        permits.Add(new CPPermitReport
                        {
                            id = permitId,
                            PermitType = reader["PermitType"]?.ToString(),
                            AIC_name = reader["AIC_name"]?.ToString(),
                            TPE_name = reader["TPE_name"]?.ToString(),
                            PONo = reader["PONo"]?.ToString(),
                            STCNo = reader["STCNo"]?.ToString(),
                            created_at = reader["created_at"]?.ToString(),
                            status = reader["status"]?.ToString(),
                            review_msg = reader["review_msg"]?.ToString(),
                            approved_at = reader["approved_at"]?.ToString(),
                            closed_at = reader["closed_at"]?.ToString()
                        });
                    }
                }
            }

            if (!isFilterApplied)
            {
                permits = permits.Take(100).ToList();
            }

            if (permits.Count == 0)
            {
                ViewBag.NoData = "No permit records found for the selected filters.";
            }

            return View(permits);
        }

        public IActionResult CNG(string? status, DateTime? fromDate, DateTime? toDate, string? permitNumber)
        {
            List<CNGPermitReport> permits = new List<CNGPermitReport>();

            int? empId = HttpContext.Session.GetInt32("Emp_ID");

            if (empId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool isFilterApplied =
                !string.IsNullOrWhiteSpace(status) ||
                fromDate.HasValue ||
                toDate.HasValue ||
                !string.IsNullOrWhiteSpace(permitNumber);

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand empCmd = new SqlCommand(
                    "SELECT Name, Designation FROM Login_VC WHERE Emp_ID = @Emp_ID",
                    con);

                empCmd.Parameters.Add("@Emp_ID", SqlDbType.Int).Value = empId.Value;

                using (SqlDataReader empReader = empCmd.ExecuteReader())
                {
                    if (empReader.Read())
                    {
                        ViewBag.EmpName = empReader["Name"]?.ToString();
                        ViewBag.Designation = empReader["Designation"]?.ToString();
                        ViewBag.EmpId = empId.Value;
                    }
                }

                SqlCommand cmd = new SqlCommand("sp_CNG_web_ReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;

                cmd.Parameters.AddWithValue("@FDate", fromDate.HasValue ? fromDate.Value.ToString("dd-MM-yyyy") : DBNull.Value);
                cmd.Parameters.AddWithValue("@TDate", toDate.HasValue ? toDate.Value.ToString("dd-MM-yyyy") : DBNull.Value);
                cmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString("D2"));
                cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int permitId = Convert.ToInt32(reader["id"]);

                        if (!string.IsNullOrWhiteSpace(permitNumber) &&
                            permitId.ToString() != permitNumber.Trim())
                        {
                            continue;
                        }

                        permits.Add(new CNGPermitReport
                        {
                            id = permitId,
                            PermitType = reader["PermitType"]?.ToString(),
                            AIC_name = reader["AIC_name"]?.ToString(),
                            TPE_name = reader["TPE_name"]?.ToString(),
                            PONo = reader["PONo"]?.ToString(),
                            STCNo = reader["STCNo"]?.ToString(),
                            created_at = reader["created_at"]?.ToString(),
                            status = reader["status"]?.ToString(),
                            review_msg = reader["review_msg"]?.ToString(),
                            approved_at = reader["approved_at"]?.ToString(),
                            closed_at = reader["closed_at"]?.ToString()
                        });
                    }
                }
            }

            if (!isFilterApplied)
            {
                permits = permits.Take(100).ToList();
            }

            if (permits.Count == 0)
            {
                ViewBag.NoData = "No permit records found for the selected filters.";
            }

            return View(permits);
        }

        public IActionResult SMA(string status, DateTime? fromDate, DateTime? toDate, string permitNumber)
        {
            List<SMAReportModel> permits = new List<SMAReportModel>();

            int? sessionEmpId = HttpContext.Session.GetInt32("Emp_ID");

            if (sessionEmpId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string empId = sessionEmpId.Value.ToString();
            string empIdJson = "[\"" + empId + "\"]";

            bool isFilterApplied =
                !string.IsNullOrWhiteSpace(status) ||
                fromDate.HasValue ||
                toDate.HasValue ||
                !string.IsNullOrWhiteSpace(permitNumber);

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand empCmd = new SqlCommand(
                    "SELECT Name, Designation FROM Login_VC WHERE Emp_ID = @Emp_ID",
                    con);

                empCmd.Parameters.Add("@Emp_ID", SqlDbType.Int).Value = sessionEmpId.Value;

                using (SqlDataReader empReader = empCmd.ExecuteReader())
                {
                    if (empReader.Read())
                    {
                        ViewBag.EmpName = empReader["Name"]?.ToString();
                        ViewBag.Designation = empReader["Designation"]?.ToString();
                        ViewBag.EmpId = sessionEmpId.Value;
                    }
                }

                string topClause = isFilterApplied ? "" : "TOP 100";

                string query = $@"
SELECT {topClause}
    P.id,
    P.Gascoseeker,
    P.Hira,
    P.Tool,
    P.STC,
    P.Technician,
    P.Emergency,
    P.Other_image,
    C.Photo1,
    C.Photo2,
    C.Photo3,
    P.Valve_name,
    P.District,
    P.Valve_id,
    P.AIC_name,
    P.TPE_name,
    P.STCNo,
    P.created_at,
    P.status,
    P.Emp_id,
    P.review_msg,
    P.approved_at,
    P.closed_at
FROM PermitData P
OUTER APPLY
(
    SELECT TOP 1
        CP.Photo1,
        CP.Photo2,
        CP.Photo3
    FROM ClosedPath CP
    WHERE CP.Pid = P.id
    ORDER BY CP.Pid DESC
) C
WHERE
(
    LTRIM(RTRIM(CAST(P.Emp_id AS NVARCHAR(50)))) = @EmpId
    OR
    LTRIM(RTRIM(CAST(P.Emp_id AS NVARCHAR(50)))) = @EmpIdJson
)
AND (@Status IS NULL OR UPPER(LTRIM(RTRIM(P.status))) = UPPER(LTRIM(RTRIM(@Status))))
AND (@FromDate IS NULL OR CAST(P.created_at AS DATE) >= @FromDate)
AND (@ToDate IS NULL OR CAST(P.created_at AS DATE) <= @ToDate)
AND (@PermitNumber IS NULL OR CAST(P.id AS NVARCHAR(50)) = @PermitNumber)
ORDER BY P.id DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@EmpId", SqlDbType.NVarChar, 50).Value = empId;
                    cmd.Parameters.Add("@EmpIdJson", SqlDbType.NVarChar, 50).Value = empIdJson;

                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim();

                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value =
                        fromDate.HasValue ? fromDate.Value.Date : DBNull.Value;

                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value =
                        toDate.HasValue ? toDate.Value.Date : DBNull.Value;

                    cmd.Parameters.Add("@PermitNumber", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(permitNumber) ? DBNull.Value : permitNumber.Trim();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permits.Add(new SMAReportModel
                            {
                                PermitNo = reader["id"]?.ToString(),
                                Gascoseeker = reader["Gascoseeker"]?.ToString(),
                                HiraDocument = reader["Hira"]?.ToString(),
                                TBTDocument = reader["Tool"]?.ToString(),
                                STC = reader["STC"]?.ToString(),
                                Technician = reader["Technician"]?.ToString(),
                                Emergency = reader["Emergency"]?.ToString(),
                                OtherImage = reader["Other_image"]?.ToString(),
                                ClosurePhoto1 = reader["Photo1"]?.ToString(),
                                ClosurePhoto2 = reader["Photo2"]?.ToString(),
                                ClosurePhoto3 = reader["Photo3"]?.ToString(),
                                ValveName = reader["Valve_name"]?.ToString(),
                                District = reader["District"]?.ToString(),
                                ValveId = reader["Valve_id"]?.ToString(),
                                AICName = reader["AIC_name"]?.ToString(),
                                TPEName = reader["TPE_name"]?.ToString(),
                                STCNo = reader["STCNo"]?.ToString(),
                                CreatedOn = reader["created_at"]?.ToString(),
                                Status = reader["status"]?.ToString(),
                                EmpId = reader["Emp_id"]?.ToString(),
                                ReviewMessage = reader["review_msg"]?.ToString(),
                                ApprovedOn = reader["approved_at"]?.ToString(),
                                ClosedOn = reader["closed_at"]?.ToString()
                            });
                        }
                    }
                }
            }

            if (permits.Count == 0)
            {
                ViewBag.NoData = "No permit records found for the selected filters.";
            }

            ViewBag.Requested = permits.Count(x =>
                x.Status != null &&
                x.Status.Equals("Requested", StringComparison.OrdinalIgnoreCase));

            ViewBag.Approved = permits.Count(x =>
                x.Status != null &&
                x.Status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase));

            ViewBag.Closed = permits.Count(x =>
                x.Status != null &&
                x.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase));

            return View(permits);
        }


        public IActionResult Details(int id)
        {
            int? empId = HttpContext.Session.GetInt32("Emp_ID");

            if (empId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new PermitDetailsViewModel
            {
                PermitNumber = id
            };

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CP_web_ReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@TDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString());
                cmd.Parameters.AddWithValue("@status", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int permitId = Convert.ToInt32(reader["id"]);

                    if (permitId == id)
                    {
                        model.PermitType = reader["PermitType"].ToString();
                        model.RequestorName = reader["AIC_name"].ToString();
                        break;
                    }
                }
            }

            model.Images.Add(new PermitImageViewModel { Title = "Gascoseeker", ImagePath = "/images/permits/gascoseeker.png" });
            model.Images.Add(new PermitImageViewModel { Title = "First Aid Box", ImagePath = "/images/permits/firstaidbox.png" });
            model.Images.Add(new PermitImageViewModel { Title = "ERP", ImagePath = "/images/permits/erp.png" });
            model.Images.Add(new PermitImageViewModel { Title = "HIRA Document Photo 1", ImagePath = "/images/permits/hira1.png" });
            model.Images.Add(new PermitImageViewModel { Title = "HIRA Document Photo 2", ImagePath = "" });
            model.Images.Add(new PermitImageViewModel { Title = "STC Card Photo 1", ImagePath = "/images/permits/stc1.png" });
            model.Images.Add(new PermitImageViewModel { Title = "STC Card Photo 2", ImagePath = "" });
            model.Images.Add(new PermitImageViewModel { Title = "TBT Document Photo 1", ImagePath = "/images/permits/tbt1.png" });
            model.Images.Add(new PermitImageViewModel { Title = "TBT Document Photo 2", ImagePath = "/images/permits/tbt2.png" });
            model.Images.Add(new PermitImageViewModel { Title = "Site Barricading Photo 1", ImagePath = "/images/permits/barricading1.png" });
            model.Images.Add(new PermitImageViewModel { Title = "Site Barricading Photo 2", ImagePath = "" });

            ViewBag.BackAction = "CathodicProtection";
            return View(model);
        }

        public IActionResult CNGDetails(int id, string back = "CNG")
        {
            int? empId = HttpContext.Session.GetInt32("Emp_ID");

            if (empId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new PermitDetailsViewModel
            {
                PermitNumber = id
            };

            string connectionString = _configuration.GetConnectionString("SMAConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CNG_web_ReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@TDate", DBNull.Value);
                cmd.Parameters.AddWithValue("@Emp_id", empId.Value.ToString("D2"));
                cmd.Parameters.AddWithValue("@status", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int permitId = Convert.ToInt32(reader["id"]);

                    if (permitId == id)
                    {
                        model.PermitType = reader["PermitType"].ToString();
                        model.RequestorName = reader["AIC_name"].ToString();
                        break;
                    }
                }
            }

            model.Images.Add(new PermitImageViewModel { Title = "Gascoseeker", ImagePath = "/images/cng/cng_gascoseeker.png" });
            model.Images.Add(new PermitImageViewModel { Title = "First Aid Box", ImagePath = "" });
            model.Images.Add(new PermitImageViewModel { Title = "ERP", ImagePath = "/images/cng/cng_erp.png" });
            model.Images.Add(new PermitImageViewModel { Title = "HIRA Document", ImagePath = "" });
            model.Images.Add(new PermitImageViewModel { Title = "STC Card Photo", ImagePath = "/images/cng/cng_stc.png" });
            model.Images.Add(new PermitImageViewModel { Title = "TBT Document Photo", ImagePath = "/images/cng/cng_tbt.png" });
            model.Images.Add(new PermitImageViewModel { Title = "Site Barricading Photo", ImagePath = "/images/cng/cng_barricading.png" });

            ViewBag.BackAction = back;
            return View("Details", model);
        }

        public IActionResult CngElectricalPermitPrint(int id)
        {
            CngElectricalPermitPrintModel model = new CngElectricalPermitPrintModel();

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("SMAConnection")))
            {
                SqlCommand cmd = new SqlCommand("sp_CNG_Elec_web_FetchData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // 1. SITE DETAILS
                    if (dr.Read())
                    {
                        model.PermitNo = dr["Permit_No"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Permit_No"]);

                        model.SiteDate = dr["Site_date"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(dr["Site_date"]);

                        model.SiteName = dr["Site_name"]?.ToString();
                        model.Location = dr["Location"]?.ToString();
                        model.CrossReference = dr["CrossReference"]?.ToString();
                        model.NatureOfWork = dr["Nature_of_Work"]?.ToString();
                    }

                    // 2. APPARATUS DETAILS
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            model.ApparatusList.Add(new CngElectricalApparatus
                            {
                                Apparatus = dr["Apparatus"]?.ToString(),
                                Lock = dr["Lock"]?.ToString(),
                                Tag = dr["Tag"]?.ToString(),
                                Remarks = dr["Remarks"]?.ToString()
                            });
                        }
                    }

                    // 3. PPE DETAILS
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.VoltageLevel = dr["Voltage_level"]?.ToString();
                            model.KeySafe = dr["Key_safe"]?.ToString();
                            model.ESSPresent = dr["ESS_present"]?.ToString();
                            model.ITPresent = dr["IT_present"]?.ToString();
                            model.EGPresent = dr["EG_present"]?.ToString();
                            model.FEPresent = dr["FE_present"]?.ToString();
                            model.OtherPPEs = dr["Other_PPEs"]?.ToString();
                        }
                    }

                    // 4. ISSUING AUTHORITY / AUTHORIZATION
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.IssuingAuthority = new CngElectricalPerson
                            {
                                IdNo = dr["AIC_id"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value ||
                                             string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }

                    // 5. RECEIVING PERSON / SUPERVISOR
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.ReceivingPerson = new CngElectricalPerson
                            {
                                IdNo = dr["STC_no"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value ||
                                             string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }

                    // 6. CLOSURE / CANCELLATION PERSON
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.ClosurePerson = new CngElectricalPerson
                            {
                                IdNo = dr["AIC_id"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value ||
                                             string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }
                }
            }

            return View(model);
        }
        public IActionResult CPElectricalPermitPrint(int id)
        {
            CngElectricalPermitPrintModel model = new CngElectricalPermitPrintModel();

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("SMAConnection")))
            {
                SqlCommand cmd = new SqlCommand("sp_CP_Elec_web_FetchData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // 1. SITE DETAILS
                    if (dr.Read())
                    {
                        model.PermitNo = dr["Permit_No"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Permit_No"]);
                        model.SiteDate = dr["Site_date"] == DBNull.Value ? null : Convert.ToDateTime(dr["Site_date"]);
                        model.SiteName = dr["Site_name"]?.ToString();
                        model.Location = dr["Location"]?.ToString();
                        model.CrossReference = dr["CrossReference"]?.ToString();
                        model.NatureOfWork = dr["Nature_of_Work"]?.ToString();
                    }

                    // 2. APPARATUS
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            model.ApparatusList.Add(new CngElectricalApparatus
                            {
                                Apparatus = dr["Apparatus"]?.ToString(),
                                Lock = dr["Lock"]?.ToString(),
                                Tag = dr["Tag"]?.ToString(),
                                Remarks = dr["Remarks"]?.ToString()
                            });
                        }
                    }

                    // 3. PPE
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.VoltageLevel = dr["Voltage_level"]?.ToString();
                            model.KeySafe = dr["Key_safe"]?.ToString();
                            model.ESSPresent = dr["ESS_present"]?.ToString();
                            model.ITPresent = dr["IT_present"]?.ToString();
                            model.EGPresent = dr["EG_present"]?.ToString();
                            model.FEPresent = dr["FE_present"]?.ToString();
                            model.OtherPPEs = dr["Other_PPEs"]?.ToString();
                        }
                    }

                    // 4. ISSUING AUTHORITY
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.IssuingAuthority = new CngElectricalPerson
                            {
                                IdNo = dr["AIC_id"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value || string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }

                    // 5. RECEIVING PERSON
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.ReceivingPerson = new CngElectricalPerson
                            {
                                IdNo = dr["STC_no"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value || string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }

                    // 6. CLOSURE PERSON
                    if (dr.NextResult())
                    {
                        if (dr.Read())
                        {
                            model.ClosurePerson = new CngElectricalPerson
                            {
                                IdNo = dr["AIC_id"]?.ToString(),
                                Name = dr["Name"]?.ToString(),
                                Designation = dr["Designation"]?.ToString(),
                                ContactNo = dr["Contact_No"]?.ToString(),
                                Signature = dr["Signature"]?.ToString(),
                                TimeSigned = dr["time_signed"] == DBNull.Value || string.IsNullOrWhiteSpace(dr["time_signed"]?.ToString())
                                    ? null
                                    : Convert.ToDateTime(dr["time_signed"])
                            };
                        }
                    }
                }
            }

            return View("CngElectricalPermitPrint", model);
        }
        public IActionResult CSEPermitPrint(int id)
        {
            var model = new CSEPermitPrintModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SMAConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.sp_CP_CS_web_FetchData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            model.PermitNo = Convert.ToInt32(dr["Permit_No"]);
                            model.PrintDate = DateTime.Now;
                            model.StartDate = dr["Site_date"] == DBNull.Value ? null : Convert.ToDateTime(dr["Site_date"]);
                            model.SiteName = dr["Name"]?.ToString();
                            model.Location = dr["Location"]?.ToString();
                            model.CrossReference = dr["CrossReference"]?.ToString();
                        }

                        if (dr.NextResult())
                        {
                            int sr = 1;
                            while (dr.Read())
                            {
                                model.ChecklistItems.Add(new CSEChecklistItem
                                {
                                    SrNo = sr,
                                    Description = dr["Question"]?.ToString(),
                                    Answer = dr["Answer"]?.ToString(),
                                    Remark = dr["Remarks"]?.ToString()
                                });
                                sr++;
                            }
                        }

                        if (dr.NextResult() && dr.Read())
                        {
                            model.FirstSignatory.Name = dr["Name"]?.ToString();
                            model.FirstSignatory.Designation = dr["Designation"]?.ToString();
                            model.FirstSignatory.ContactNo = dr["Contact_No"]?.ToString();
                            model.FirstSignatory.Signature = dr["Signature"]?.ToString();
                            model.FirstSignatory.TimeSigned = dr["time_signed"] == DBNull.Value ? null : Convert.ToDateTime(dr["time_signed"]);
                        }

                        if (dr.NextResult() && dr.Read())
                        {
                            model.SecondSignatory.Name = dr["Name"]?.ToString();
                            model.SecondSignatory.Designation = dr["Designation"]?.ToString();
                            model.SecondSignatory.ContactNo = dr["Contact_No"]?.ToString();
                            model.SecondSignatory.Signature = dr["Signature"]?.ToString();
                            model.SecondSignatory.TimeSigned = dr["time_signed"] == DBNull.Value ? null : Convert.ToDateTime(dr["time_signed"]);
                        }

                        if (dr.NextResult() && dr.Read())
                        {
                            model.ThirdSignatory.Name = dr["Name"]?.ToString();
                            model.ThirdSignatory.Designation = dr["Designation"]?.ToString();
                            model.ThirdSignatory.ContactNo = dr["Contact_No"]?.ToString();
                            model.ThirdSignatory.Signature = dr["Signature"]?.ToString();
                            model.ThirdSignatory.TimeSigned = dr["time_signed"] == DBNull.Value ? null : Convert.ToDateTime(dr["time_signed"]);
                        }

                        if (dr.NextResult() && dr.Read())
                        {
                            model.Atmosphere.OxygenRequired = dr["Oxygen_Test"]?.ToString();
                            model.Atmosphere.NaturalGasRequired = dr["NG_Test"]?.ToString();

                            var oxygen = (dr["Oxygen_Readings"]?.ToString() ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries);
                            var ng = (dr["NG_Readings"]?.ToString() ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries);


                            if (oxygen.Length > 0) model.Atmosphere.O0 = oxygen[0];
                            if (oxygen.Length > 1) model.Atmosphere.O1 = oxygen[1];
                            if (oxygen.Length > 2) model.Atmosphere.O2 = oxygen[2];
                            if (oxygen.Length > 3) model.Atmosphere.O3 = oxygen[3];
                            if (oxygen.Length > 4) model.Atmosphere.O4 = oxygen[4];
                            if (oxygen.Length > 5) model.Atmosphere.O5 = oxygen[5];
                            if (oxygen.Length > 6) model.Atmosphere.O6 = oxygen[6];
                            if (oxygen.Length > 7) model.Atmosphere.O7 = oxygen[7];

                            string BlankIfZero(string value)
                            {
                                return value.Trim() == "0" ? "" : value;
                            }

                            if (ng.Length > 0) model.Atmosphere.N0 = BlankIfZero(ng[0]);
                            if (ng.Length > 1) model.Atmosphere.N1 = BlankIfZero(ng[1]);
                            if (ng.Length > 2) model.Atmosphere.N2 = BlankIfZero(ng[2]);
                            if (ng.Length > 3) model.Atmosphere.N3 = BlankIfZero(ng[3]);
                            if (ng.Length > 4) model.Atmosphere.N4 = BlankIfZero(ng[4]);
                            if (ng.Length > 5) model.Atmosphere.N5 = BlankIfZero(ng[5]);
                            if (ng.Length > 6) model.Atmosphere.N6 = BlankIfZero(ng[6]);
                            if (ng.Length > 7) model.Atmosphere.N7 = BlankIfZero(ng[7]);
                        }
                    }
                }
            }

            return View("CSEPermitPrint", model);
        }
        public IActionResult CPHotColdWorkPermitPrint(int id)
        {
            var model = new HotColdWorkPermitModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SMAConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CP_HAC_web_FetchData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // 1. Site details
                        if (dr.Read())
                        {
                            model.PermitNo = dr["Permit_No"]?.ToString();
                            model.SiteLocation = dr["Location"]?.ToString();
                            model.NatureOfWork = dr["Nature_of_Work"]?.ToString();

                            string siteDate = "";

                            if (dr["Site_date"] != DBNull.Value)
                            {
                                DateTime dt = Convert.ToDateTime(dr["Site_date"]);
                                siteDate = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }

                            model.AStartDate = siteDate;
                            model.AEndDate = siteDate;
                        }

                        // 2. Hazard table
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                string hazard = dr["Hazard"]?.ToString()?.Trim() ?? "";

                                if (hazard.Contains("Mechanical"))
                                {
                                    model.MP_HD = dr["Hazard_details"]?.ToString();
                                    model.MP_HE = dr["Hazard_exists"]?.ToString();
                                    model.MP_HI = dr["Hazard_isolated"]?.ToString();
                                    model.MP_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.MP_Lock = dr["Lock_no"]?.ToString();
                                    model.MP_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Pressurized"))
                                {
                                    model.PG_HD = dr["Hazard_details"]?.ToString();
                                    model.PG_HE = dr["Hazard_exists"]?.ToString();
                                    model.PG_HI = dr["Hazard_isolated"]?.ToString();
                                    model.PG_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.PG_Lock = dr["Lock_no"]?.ToString();
                                    model.PG_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Remotely"))
                                {
                                    model.ROE_HD = dr["Hazard_details"]?.ToString();
                                    model.ROE_HE = dr["Hazard_exists"]?.ToString();
                                    model.ROE_HI = dr["Hazard_isolated"]?.ToString();
                                    model.ROE_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.ROE_Lock = dr["Lock_no"]?.ToString();
                                    model.ROE_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Electricity"))
                                {
                                    model.Elec_HD = dr["Hazard_details"]?.ToString();
                                    model.Elec_HE = dr["Hazard_exists"]?.ToString();
                                    model.Elec_HI = dr["Hazard_isolated"]?.ToString();
                                    model.Elec_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.Elec_Lock = dr["Lock_no"]?.ToString();
                                    model.Elec_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Confined"))
                                {
                                    model.CSE_HD = dr["Hazard_details"]?.ToString();
                                    model.CSE_HE = dr["Hazard_exists"]?.ToString();
                                    model.CSE_HI = dr["Hazard_isolated"]?.ToString();
                                    model.CSE_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.CSE_Lock = dr["Lock_no"]?.ToString();
                                    model.CSE_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Others"))
                                {
                                    model.Other_HD = dr["Hazard_details"]?.ToString();
                                    model.Other_HE = dr["Hazard_exists"]?.ToString();
                                    model.Other_HI = dr["Hazard_isolated"]?.ToString();
                                    model.Other_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.Other_Lock = dr["Lock_no"]?.ToString();
                                    model.Other_Tag = dr["Tag_no"]?.ToString();
                                }
                            }
                        }

                        // 3. Site condition checklist
                        if (dr.NextResult() && dr.Read())
                        {
                            model.CheckBox1 = ConvertToBool(dr["SCC_Q1"]);
                            model.CheckBox2 = ConvertToBool(dr["SCC_Q2"]);
                            model.CheckBox3 = ConvertToBool(dr["SCC_Q3"]);
                            model.CheckBox4 = ConvertToBool(dr["SCC_Q4"]);
                            model.CheckBox5 = ConvertToBool(dr["SCC_Q5"]);
                            model.CheckBox6 = ConvertToBool(dr["SCC_Q6"]);
                            model.CheckBox7 = ConvertToBool(dr["SCC_Q7"]);

                            model.ProcedureRefNo = dr["Procedure_refno"]?.ToString();
                            model.OtherScc = dr["Other_SCC"]?.ToString();
                        }

                        // 4. Readings
                        if (dr.NextResult())
                        {
                            int i = 1;

                            while (dr.Read() && i <= 10)
                            {
                                string location = dr["Location_of_Reading"]?.ToString();
                                string time = dr["Time_of_reading"]?.ToString();
                                string reading = CleanReading(dr["Reading"]);


                                if (i == 1) { model.Lor1 = location; model.Tor1 = time; model.R1 = reading; }
                                if (i == 2) { model.Lor2 = location; model.Tor2 = time; model.R2 = reading; }
                                if (i == 3) { model.Lor3 = location; model.Tor3 = time; model.R3 = reading; }
                                if (i == 4) { model.Lor4 = location; model.Tor4 = time; model.R4 = reading; }
                                if (i == 5) { model.Lor5 = location; model.Tor5 = time; model.R5 = reading; }
                                if (i == 6) { model.Lor6 = location; model.Tor6 = time; model.R6 = reading; }
                                if (i == 7) { model.Lor7 = location; model.Tor7 = time; model.R7 = reading; }
                                if (i == 8) { model.Lor8 = location; model.Tor8 = time; model.R8 = reading; }
                                if (i == 9) { model.Lor9 = location; model.Tor9 = time; model.R9 = reading; }
                                if (i == 10) { model.Lor10 = location; model.Tor10 = time; model.R10 = reading; }

                                i++;
                            }
                        }

                        // 5. Authorization Engineer
                        if (dr.NextResult() && dr.Read())
                        {
                            model.AEngineerName = dr["Name"]?.ToString();
                            model.ADesignation = dr["Designation"]?.ToString();
                            model.ATelephone = dr["Contact_No"]?.ToString();
                            model.ASignature = dr["Signature"]?.ToString();

                            model.SiteSupervisorName = "";
                        }

                        // 6. Permit Acceptance Site Supervisor
                        if (dr.NextResult() && dr.Read())
                        {
                            model.ASSName = dr["Name"]?.ToString();
                            model.ASSDesignation = dr["Designation"]?.ToString();
                            model.ASSTelephone = dr["Contact_No"]?.ToString();
                            model.ASiteSupervisor = dr["Signature"]?.ToString();

                            model.SiteSupervisorName = dr["Name"]?.ToString();
                        }

                        // 7. Work Completed Closure
                        if (dr.NextResult() && dr.Read())
                        {
                            model.CEngineer = dr["Signature"]?.ToString();
                            model.CName = dr["Name"]?.ToString();
                            model.CDesignation = dr["Designation"]?.ToString();
                            model.CTelephone = dr["Contact_No"]?.ToString();
                        }
                    }
                }
            }

            return View("HotColdWorkPermitPrint", model);
        }
        private bool ConvertToBool(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;

            string text = value.ToString()?.Trim().ToLower() ?? "";

            return text == "true" || text == "1" || text == "yes" || text == "y";
        }
        private string CleanReading(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            string text = value.ToString()?.Trim() ?? "";

            if (text == "%" || text == "% " || text == " %")
                return "";

            return text;
        }


        public IActionResult CNGHotColdWorkPermitPrint(int id)
        {
            var model = new HotColdWorkPermitModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SMAConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CNG_HAC_web_FetchData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // 1. Site Details
                        if (dr.Read())
                        {
                            model.PermitNo = dr["Permit_No"]?.ToString();
                            model.SiteLocation = dr["Location"]?.ToString();
                            model.NatureOfWork = dr["Nature_of_Work"]?.ToString();

                            string siteDate = "";

                            if (dr["Site_date"] != DBNull.Value)
                            {
                                DateTime dt = Convert.ToDateTime(dr["Site_date"]);
                                siteDate = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }

                            model.AStartDate = siteDate;
                            model.AEndDate = siteDate;
                        }

                        // 2. Hazard Details
                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                string hazard = dr["Hazard"]?.ToString()?.Trim() ?? "";

                                if (hazard.Contains("Mechanical"))
                                {
                                    model.MP_HD = dr["Hazard_details"]?.ToString();
                                    model.MP_HE = dr["Hazard_exists"]?.ToString();
                                    model.MP_HI = dr["Hazard_isolated"]?.ToString();
                                    model.MP_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.MP_Lock = dr["Lock_no"]?.ToString();
                                    model.MP_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Pressurized"))
                                {
                                    model.PG_HD = dr["Hazard_details"]?.ToString();
                                    model.PG_HE = dr["Hazard_exists"]?.ToString();
                                    model.PG_HI = dr["Hazard_isolated"]?.ToString();
                                    model.PG_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.PG_Lock = dr["Lock_no"]?.ToString();
                                    model.PG_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Remotely"))
                                {
                                    model.ROE_HD = dr["Hazard_details"]?.ToString();
                                    model.ROE_HE = dr["Hazard_exists"]?.ToString();
                                    model.ROE_HI = dr["Hazard_isolated"]?.ToString();
                                    model.ROE_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.ROE_Lock = dr["Lock_no"]?.ToString();
                                    model.ROE_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Electricity"))
                                {
                                    model.Elec_HD = dr["Hazard_details"]?.ToString();
                                    model.Elec_HE = dr["Hazard_exists"]?.ToString();
                                    model.Elec_HI = dr["Hazard_isolated"]?.ToString();
                                    model.Elec_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.Elec_Lock = dr["Lock_no"]?.ToString();
                                    model.Elec_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Confined"))
                                {
                                    model.CSE_HD = dr["Hazard_details"]?.ToString();
                                    model.CSE_HE = dr["Hazard_exists"]?.ToString();
                                    model.CSE_HI = dr["Hazard_isolated"]?.ToString();
                                    model.CSE_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.CSE_Lock = dr["Lock_no"]?.ToString();
                                    model.CSE_Tag = dr["Tag_no"]?.ToString();
                                }
                                else if (hazard.Contains("Others"))
                                {
                                    model.Other_HD = dr["Hazard_details"]?.ToString();
                                    model.Other_HE = dr["Hazard_exists"]?.ToString();
                                    model.Other_HI = dr["Hazard_isolated"]?.ToString();
                                    model.Other_HMM = dr["Hazard_mitigation"]?.ToString();
                                    model.Other_Lock = dr["Lock_no"]?.ToString();
                                    model.Other_Tag = dr["Tag_no"]?.ToString();
                                }
                            }
                        }

                        // 3. Site Condition Checklist
                        if (dr.NextResult() && dr.Read())
                        {
                            model.CheckBox1 = ConvertToBool(dr["SCC_Q1"]);
                            model.CheckBox2 = ConvertToBool(dr["SCC_Q2"]);
                            model.CheckBox3 = ConvertToBool(dr["SCC_Q3"]);
                            model.CheckBox4 = ConvertToBool(dr["SCC_Q4"]);
                            model.CheckBox5 = ConvertToBool(dr["SCC_Q5"]);
                            model.CheckBox6 = ConvertToBool(dr["SCC_Q6"]);
                            model.CheckBox7 = ConvertToBool(dr["SCC_Q7"]);

                            model.ProcedureRefNo = dr["Procedure_refno"]?.ToString();
                            model.OtherScc = dr["Other_SCC"]?.ToString();
                        }

                        // 4. Readings
                        if (dr.NextResult())
                        {
                            int i = 1;

                            while (dr.Read() && i <= 10)
                            {
                                string location = dr["Location_of_Reading"]?.ToString();
                                string time = dr["Time_of_reading"]?.ToString();
                                string reading = CleanReading(dr["Reading"]);

                                if (i == 1) { model.Lor1 = location; model.Tor1 = time; model.R1 = reading; }
                                if (i == 2) { model.Lor2 = location; model.Tor2 = time; model.R2 = reading; }
                                if (i == 3) { model.Lor3 = location; model.Tor3 = time; model.R3 = reading; }
                                if (i == 4) { model.Lor4 = location; model.Tor4 = time; model.R4 = reading; }
                                if (i == 5) { model.Lor5 = location; model.Tor5 = time; model.R5 = reading; }
                                if (i == 6) { model.Lor6 = location; model.Tor6 = time; model.R6 = reading; }
                                if (i == 7) { model.Lor7 = location; model.Tor7 = time; model.R7 = reading; }
                                if (i == 8) { model.Lor8 = location; model.Tor8 = time; model.R8 = reading; }
                                if (i == 9) { model.Lor9 = location; model.Tor9 = time; model.R9 = reading; }
                                if (i == 10) { model.Lor10 = location; model.Tor10 = time; model.R10 = reading; }

                                i++;
                            }
                        }

                        // 5. Authorization
                        if (dr.NextResult() && dr.Read())
                        {
                            model.AEngineerName = dr["Name"]?.ToString();
                            model.ADesignation = dr["Designation"]?.ToString();
                            model.ATelephone = dr["Contact_No"]?.ToString();
                            model.ASignature = dr["Signature"]?.ToString();
                        }

                        // 6. Site Supervisor
                        if (dr.NextResult() && dr.Read())
                        {
                            model.ASSName = dr["Name"]?.ToString();
                            model.ASSDesignation = dr["Designation"]?.ToString();
                            model.ASSTelephone = dr["Contact_No"]?.ToString();
                            model.ASiteSupervisor = dr["Signature"]?.ToString();

                            model.SiteSupervisorName = dr["Name"]?.ToString();
                        }

                        // 7. Work Completed
                        if (dr.NextResult() && dr.Read())
                        {
                            model.CEngineer = dr["Signature"]?.ToString();
                            model.CName = dr["Name"]?.ToString();
                            model.CDesignation = dr["Designation"]?.ToString();
                            model.CTelephone = dr["Contact_No"]?.ToString();
                        }
                    }
                }
            }

            return View("HotColdWorkPermitPrint", model);
        }
        
        public IActionResult CNGWAHPrint(int id)
        {
            var model = new CNGWAHPrintModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SMAConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CNG_WAH_web_FetchData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // 1. Site Details
                        if (dr.Read())
                        {
                            model.PermitNo = dr["Permit_No"]?.ToString();
                            model.Date = dr["Site_date"]?.ToString();
                            model.TimeOfIssue = dr["Site_time"]?.ToString();
                            model.Location = dr["Location"]?.ToString();
                            model.CrossRef = dr["CrossReference"]?.ToString();
                            model.Activity = dr["Nature_of_Work"]?.ToString();
                        }

                        // 2. Question Responses
                        

                        // 3. Authorization
                        if (dr.NextResult() && dr.Read())
                        {
                            model.AuthName = dr["Name"]?.ToString();
                            model.AuthDesig = dr["Designation"]?.ToString();
                            model.AuthContact = dr["Contact_No"]?.ToString();
                            model.AuthSign = dr["Signature"]?.ToString();
                            model.AuthTime = dr["time_signed"]?.ToString();
                        }

                        // 4. Competent Person / TPE
                        if (dr.NextResult() && dr.Read())
                        {
                            model.TpeName = dr["Name"]?.ToString();
                            model.TpeDesig = dr["Designation"]?.ToString();
                            model.TpeContact = dr["Contact_No"]?.ToString();
                            model.TpeSign = dr["Signature"]?.ToString();
                            model.TpeTime = dr["time_signed"]?.ToString();
                        }

                        // 5. Supervisor
                        if (dr.NextResult() && dr.Read())
                        {
                            model.SuperName = dr["Name"]?.ToString();
                            model.SuperDesig = dr["Designation"]?.ToString();
                            model.SuperContact = dr["Contact_No"]?.ToString();
                            model.SuperSign = dr["Signature"]?.ToString();
                            model.SuperTime = dr["time_signed"]?.ToString();
                        }

                        // 6. Closure
                        if (dr.NextResult() && dr.Read())
                        {
                            model.CowName = dr["Name"]?.ToString();
                            model.CowDate = dr["date"]?.ToString();
                            model.CowContact = dr["Contact_No"]?.ToString();
                            model.CowSign = dr["Signature"]?.ToString();

                            if (HasColumn(dr, "in_time"))
                                model.CowInTime = dr["in_time"]?.ToString();

                            if (HasColumn(dr, "out_time"))
                                model.CowOutTime = dr["out_time"]?.ToString();

                            if (HasColumn(dr, "time_signed"))
                                model.CowOutTime = dr["time_signed"]?.ToString();

                            model.CowSign2 = model.CowSign;
                        }
                    }
                }
            }

            return View(model);
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public IActionResult CngPreventiveMaintenancePermit(int id)
        {
            var model = new CngPreventiveMaintenancePermitModel();

            string Val(string? value)
            {
                return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
            }

            string Tick(string? value)
            {
                value = value?.Trim().ToUpper() ?? "";

                if (value == "" || value == "0" || value == "NO" || value == "N" || value == "FALSE")
                    return "";

                return "✓";
            }

            string YesNo(string? value)
            {
                value = value?.Trim().ToUpper() ?? "";

                if (value == "1" || value == "YES" || value == "Y" || value == "TRUE")
                    return "Yes";

                if (value == "0" || value == "NO" || value == "N" || value == "FALSE")
                    return "No";

                return "";
            }

            string NAOnly(string? value)
            {
                value = value?.Trim() ?? "";
                return value == "1" ? "✓" : "";
            }

            string GetValue(SqlDataReader reader, params string[] names)
            {
                foreach (string name in names)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetName(i).Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            return reader.IsDBNull(i) ? "" : reader.GetValue(i)?.ToString()?.Trim() ?? "";
                        }
                    }
                }
                return "";
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SMAConnection")))
            {
                SqlCommand cmd = new SqlCommand("sp_CNG_PM_web_FetchData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // 1. Site Details
                    if (dr.Read())
                    {
                        model.PermitNo = GetValue(dr, "Permit_No");
                        model.DateOfIssue = GetValue(dr, "Site_date");
                        model.SiteLocation = GetValue(dr, "Location");
                        model.SiteName = GetValue(dr, "Name");
                    }

                    // 2. Main Checkpoints
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            string q = GetValue(dr, "Ques_id").Replace(" ", "").Trim().ToUpper();

                            string lockNo = Val(GetValue(dr, "Lock_No", "Lock_no"));
                            string tagNo = Val(GetValue(dr, "Tag_No", "Tag_no"));
                            string na = Tick(GetValue(dr, "NA"));

                            if (q == "2A") { model.Lno1 = lockNo; model.Tno1 = tagNo; model.Na1 = na; }
                            else if (q == "2B") { model.Lno2 = lockNo; model.Tno2 = tagNo; model.Na2 = na; }
                            else if (q == "2C") { model.Lno3 = lockNo; model.Tno3 = tagNo; model.Na3 = na; }
                            else if (q == "2D") { model.Lno4 = lockNo; model.Tno4 = tagNo; model.Na4 = na; }
                            else if (q == "2F") { model.Lno5 = lockNo; model.Tno5 = tagNo; model.Na5 = na; }
                            else if (q == "2E") { model.Lno6 = lockNo; model.Tno6 = tagNo; model.Na6 = na; }

                            else if (q == "3A") { model.Lno7 = lockNo; model.Tno7 = tagNo; model.Na7 = na; }
                            else if (q == "3B") { model.Lno8 = lockNo; model.Tno8 = tagNo; model.Na8 = na; }
                            else if (q == "3C") { model.Lno9 = lockNo; model.Tno9 = tagNo; model.Na9 = na; }

                            else if (q == "4A") { model.Lno10 = lockNo; model.Tno10 = tagNo; model.Na10 = na; }
                            else if (q == "4B") { model.Lno11 = lockNo; model.Tno11 = tagNo; model.Na11 = na; }
                            else if (q == "4C") { model.Lno12 = lockNo; model.Tno12 = tagNo; model.Na12 = na; }

                            else if (q == "5A") { model.Lno13 = lockNo; model.Tno13 = tagNo; model.Na13 = na; }
                            else if (q == "5B") { model.Lno14 = lockNo; model.Tno14 = tagNo; model.Na14 = na; }
                            else if (q == "6A") { model.Lno15 = lockNo; model.Tno15 = tagNo; model.Na15 = na; }
                        }
                    }

                    // 3. Gas Readings
                    if (dr.NextResult())
                    {
                        int i = 1;
                        while (dr.Read() && i <= 10)
                        {
                            typeof(CngPreventiveMaintenancePermitModel).GetProperty($"Lor{i}")?.SetValue(model, GetValue(dr, "Location_of_Reading"));
                            typeof(CngPreventiveMaintenancePermitModel).GetProperty($"Tor{i}")?.SetValue(model, GetValue(dr, "Time_of_reading"));
                            typeof(CngPreventiveMaintenancePermitModel).GetProperty($"R{i}")?.SetValue(model, GetValue(dr, "Reading"));
                            i++;
                        }
                    }

                    // 4. Authorized Signatory
                    if (dr.NextResult() && dr.Read())
                    {
                        model.AuthName = GetValue(dr, "Name");
                        model.AuthDesignation = GetValue(dr, "Designation");
                        model.AuthContactNo = GetValue(dr, "Contact_No");
                        model.AuthSign = GetValue(dr, "Signature");
                    }

                    // 5. Technician / Supervisor
                    if (dr.NextResult() && dr.Read())
                    {
                        model.TechnicianName = GetValue(dr, "Name");
                        model.TechnicianDesignation = GetValue(dr, "Designation");
                        model.TechnicianContactNo = GetValue(dr, "Contact_No");
                        model.TechnicianSign = GetValue(dr, "Signature");
                    }

                    // 6. Work Completed / Closure Sign
                    if (dr.NextResult() && dr.Read())
                    {
                        model.TpeName = GetValue(dr, "Name");
                        model.TpeDesignation = GetValue(dr, "Designation");
                        model.TpeContactNo = GetValue(dr, "Contact_No");
                        model.TpeSign = GetValue(dr, "Signature");
                    }

                    // 7. Closure / Energizing Checkpoints
                    while (dr.NextResult())
                    {
                        bool hasClosureQ = false;

                        for (int c = 0; c < dr.FieldCount; c++)
                        {
                            string col = dr.GetName(c).Trim();

                            if (col.Equals("Q_Srno", StringComparison.OrdinalIgnoreCase) ||
                                col.Equals("Q_Smo", StringComparison.OrdinalIgnoreCase))
                            {
                                hasClosureQ = true;
                                break;
                            }
                        }

                        if (!hasClosureQ)
                            continue;

                        while (dr.Read())
                        {
                            string q = GetValue(dr, "Q_Srno", "Q_Smo").Replace(" ", "").Trim().ToUpper();

                            string lockNo = Val(GetValue(dr, "Lock_no", "Lock_No"));
                            string tagNo = Val(GetValue(dr, "Tag_no", "Tag_No"));
                            string yesNo = YesNo(GetValue(dr, "Yes_no", "Yes_No"));
                            string na = NAOnly(GetValue(dr, "NA"));

                            if (q == "11AA") { model.Clno1 = lockNo; model.Ctno1 = tagNo; model.Cna1 = na; }
                            else if (q == "11AB") { model.Clno2 = lockNo; model.Ctno2 = tagNo; model.Cna2 = na; }

                            else if (q == "11BA") { model.Cyn1 = yesNo; model.Cyna1 = na; }
                            else if (q == "11BB") { model.Cyn2 = yesNo; model.Cyna2 = na; }
                            else if (q == "11BC") { model.Cyn3 = yesNo; model.Cyna3 = na; }
                            else if (q == "11BD") { model.Cyn4 = yesNo; model.Cyna4 = na; }

                            else if (q == "11CA") { model.Cyn5 = yesNo; model.Cyna5 = na; }
                            else if (q == "11CB") { model.Cyn6 = yesNo; model.Cyna6 = na; }
                            else if (q == "11CC") { model.Cyn7 = yesNo; model.Cyna7 = na; }

                            else if (q == "11DA") { model.Clno3 = lockNo; model.Ctno3 = tagNo; model.Cna3 = na; }
                        }

                        break;
                    }
                }
            }

            return View(model);
        }


    }

}