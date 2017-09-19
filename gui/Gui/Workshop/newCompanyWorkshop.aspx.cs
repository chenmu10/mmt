﻿using Google.Maps;
using Google.Maps.StaticMaps;
using gui.Models;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace gui.Gui.Workshop
{
    public partial class newCompanyWorkshop : System.Web.UI.Page
    {

        // TODO fix db.insert - problem with dateTime type
        // TODO area activity shows number instead of string when choosing company

        List<Company> Companies = new List<Company>();
        DB db;
        int companyArea;

        override protected void OnInit(EventArgs e)
        {
            this.Load += new System.EventHandler(this.Page_Load);
            db = new DB();
            db.IsConnect();
            Companies = db.GetAllComapny();
            GetAllCompaniesToList();
        }

        private void GetAllCompaniesToList()
        {
            if (dropDownCompanyName.Items.Count < 1)
            {
                dropDownCompanyName.Items.Add(new ListItem("בחר/י", "0"));
                for (int i = 0; i < Companies.Count; i++)
                {
                    dropDownCompanyName.Items.Add(new ListItem(Companies[i].Company_Name, (i + 1).ToString()));
                }

            }
        }

        protected void FillCompanyDetails(object sender, EventArgs e)
        {
            List<ListItem> Areas = db.GetAllAreas();

            Company selectedComp = Companies[((DropDownList)sender).SelectedIndex - 1];
            companyID.Text = selectedComp.Company_ID.ToString();
            address.Text = selectedComp.Company_Address;
            area.Text = Areas[selectedComp.Company_Area_Activity].Text.ToString();
            companyArea = selectedComp.Company_Area_Activity; // needed for email function
           

        }


        protected void AddWorkshop(object sender, EventArgs e)
        {
            if (!IsEmptyFields())
            {
                CompanyWorkshop newcw = new CompanyWorkshop();
                newcw.CompanyWorkShopComments = comments.Text;
                //newcw.CompanyWorkShopDate = calendar.SelectedDate.ToString();
                newcw.WorkShop_Number_Of_StudentPredicted = Convert.ToInt32(PredictedStudentsNum.Text);
                newcw.CompanyID = Convert.ToInt32(companyID.Text);
                newcw.CompanyWorkShopDate = datetimePicker.Text;

                // insert not working - problem with dateTime type
                if (db.InsertNewCompanyWorkShop(newcw))
                {
                    Response.Write("<script>alert('הסדנא נוספה בהצלחה. ניתן להוסיף עוד סדנאות עבור החברה הנבחרת');</script>");

                    if(SendInvitesToSchools(newcw))
                    {
                        Msg.Text = "אימיילים נשלחו אל בתי ספר רלוונטים";
                    }
                    else 
                    {
                        Response.Write("<script>alert('שגיאה בשליחת אימיילים');</script>");
                    }
                    ClearWorkshopDetails();
                }
                else
                {
                    Response.Write("<script>alert('שגיאה ביצירת סדנא');</script>");
                }

            }
        }

       
        /// <summary>
        /// This function send emails to all the relevant school.
        /// inform them that new company workshop was created
        /// </summary>
        protected bool SendInvitesToSchools(CompanyWorkshop selectedWorkshop)
        {
            List<School> allSchools = db.GetAllSchools();
            List<Company> allCompany = db.GetAllComapny();

            int companyID = selectedWorkshop.CompanyID;
            Company SelectedComapny = allCompany.Find(x => x.Company_ID == companyID);

            allSchools = allSchools.FindAll(x => x.School_Area == SelectedComapny.Company_Area_Activity);
            EmailHelper email = new EmailHelper();
            return email.SendInitesToSchools(allSchools, selectedWorkshop);
        }

        private bool IsEmptyFields()
        {
            if (dropDownCompanyName.SelectedIndex == 0)
            {
                Response.Write("<script>alert('לא נבחרה חברה');</script>");
                return true;
            }
            //else if (PredictedStudentsNum.Text.Equals("") || hour.Text.Equals("") || minutes.Text.Equals(""))
            //{
            //    Response.Write("<script>alert('שדות חובה חסרים');</script>");
            //    return true;
            //}
            //else if (calendar.SelectedDate.Date.ToString().Equals("01/01/0001 00:00:00") || calendar.SelectedDate == null || calendar.SelectedDate == DateTime.Now)
            //{
            //    Response.Write("<script>alert('לא נבחר תאריך');</script>");
            //    return true;
            //}

            else
            {
                return false;
            }

        }

        private void ClearWorkshopDetails()
        {
            PredictedStudentsNum.Text = "";
            //hour.Text = "";
            //minutes.Text = "";
            comments.Text = "";
            //calendar.SelectedDate = DateTime.Now;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
          
        }

        protected void calendar_DayRender(object sender,
                         DayRenderEventArgs e)
        {
          
            if (e.Day.Date.CompareTo(DateTime.Today) < 0 || e.Day.Date.DayOfWeek == DayOfWeek.Saturday)
            {
                e.Day.IsSelectable = false;
                e.Cell.ForeColor = System.Drawing.Color.Gray;
            }
        }
    }
}
