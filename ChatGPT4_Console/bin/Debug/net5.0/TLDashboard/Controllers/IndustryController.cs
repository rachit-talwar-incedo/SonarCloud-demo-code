using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Areas.TLDashboard.Models;
using TLMDomain;
using TLMBLL;

namespace TLM.Areas.TLDashboard.Controllers
{
    public class IndustryController : BaseController
    {
        // GET: TLDashboard/Industry
        public ActionResult Index(string tlId)
        {
            var id = Convert.ToInt32(Common.DecryptData(tlId != null && tlId != "" ? tlId : "0"));
            ViewBag.TlId = id;
            DataSet dsTL = new IndustryInteractionBLL().GetIndustryInteractionInfo(id);
            List<IndustryInteractionsViewModel> IndustryInteractions = new List<IndustryInteractionsViewModel>();

            if (dsTL != null && dsTL.Tables.Count > 0 && dsTL.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsTL.Tables[0].Rows)
                {
                    IndustryInteractions.Add(new IndustryInteractionsViewModel { IndustryName = dr["CompanyName"].ToString(), ProgramYear = dr["ProgramYear"].ToString() });
                }
            }
            var indStatus = dsTL.Tables[1].Rows[0];
            var hideStatus = dsTL.Tables[2].Rows[0];
            if (Convert.ToInt32(hideStatus["II"].ToString()) == 0 && indStatus["HideConf_Presetn"].ToString() == "Yes" && indStatus["HideExternal_Advisory"].ToString() == "Yes" && indStatus["HideEngagement_Activities"].ToString() == "Yes")
                return RedirectToAction("Index", "Dashboard", new { tlId = tlId });
            if (Convert.ToInt32(hideStatus["II"].ToString()) == 0)
            {
                if (indStatus["HideConf_Presetn"].ToString() == "No")
                    return RedirectToAction("IndustryActivities", new { tlId = tlId });
                else if (indStatus["HideExternal_Advisory"].ToString() == "No")
                    return RedirectToAction("Advisory", new { tlId = tlId });
                else return RedirectToAction("Speaking", new { tlId = tlId });
            }
            return View(IndustryInteractions);
        }

        #region MyRegion
        public ActionResult IndustryActivities(string tlId)
        {
            var id = Convert.ToInt32(Common.DecryptData(tlId != null && tlId != "" ? tlId : "0"));
            ViewBag.TlId = id;
            IndustryActivitiesViewModel IndustryActivities = new IndustryActivitiesViewModel();
            DataSet dsTL = new ThoughtLeaderBLL().GetGetTLTotalInteractionCount(Convert.ToInt32(ViewBag.TlId));

            if (dsTL != null && dsTL.Tables.Count > 0 && dsTL.Tables[0].Rows.Count > 0)
            {
                IndustryActivities.ConferencesCount = dsTL.Tables[0].Rows[0]["ConferencesCount"].ToString();
                IndustryActivities.InteractionsCount = dsTL.Tables[0].Rows[0]["InteractionsCount"].ToString();
                IndustryActivities.PresentationsCount = dsTL.Tables[0].Rows[0]["PresentationsCount"].ToString();
            }

            return View(IndustryActivities);
        }
        public ActionResult IndustryCongress(int tlId)
        {
            List<IndustryCongressViewModel> IndustryCongress = new List<IndustryCongressViewModel>();
            DataSet dsTL = new CongressActivityBAL().GetTLCongressActivity(tlId, 1);
            if (dsTL != null && dsTL.Tables.Count > 0 && dsTL.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsTL.Tables[0].Rows)
                {
                    IndustryCongress.Add(new IndustryCongressViewModel { Count = dr["Count"].ToString(), ThoughtLeaderID = dr["ThoughtLeaderID"].ToString(), Topic = dr["Topic"].ToString() });
                }
            }
            return PartialView(IndustryCongress);
        }
       
        #endregion
    }
}
