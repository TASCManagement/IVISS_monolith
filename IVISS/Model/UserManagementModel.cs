using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVISS.Utility;

namespace IVISS.Model
{
    class UserManagementModel
    {
        public string firstName { set; get; }
        public string middleName { set; get; }
        public string lastName { set; get; }
        public string phone { set; get; }
        public string id { set; get; }
        public string password { set; get; }
        public string selectedID { set; get; }

        public DataTable FillGuards()
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("guard_id", typeof(String));
                dt.Columns.Add("guard_first", typeof(String));
                dt.Columns.Add("guard_middle", typeof(String));
                dt.Columns.Add("guard_last", typeof(String));
                dt.Columns.Add("guard_password", typeof(String));
                dt.Columns.Add("guard_phone", typeof(String));

                //dgView.DataSource = dt;
                //where objDetails.gate_no == global.m_Gate_No

                using (IVISSEntities db = new IVISSEntities())
                {

                    var detailQuery = from objDetails in db.Guards where objDetails.guard_id.ToLower() != Global.USER_NAME.ToLower()
                                      select new { objDetails.guard_id, objDetails.guard_first, objDetails.guard_middle, objDetails.guard_last, objDetails.guard_password, objDetails.guard_phone_1 };

                    DataRow dr;

                    foreach (var detail in detailQuery)
                    {
                        dr = dt.NewRow();
                        dr[0] = detail.guard_id;
                        dr[1] = detail.guard_first;
                        dr[2] = detail.guard_middle;
                        dr[3] = detail.guard_last;
                        dr[4] = detail.guard_password;
                        dr[5] = detail.guard_phone_1;

                        dt.Rows.Add(dr);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }


        public DataTable FillEmpty()
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("guard_id", typeof(String));
                dt.Columns.Add("guard_first", typeof(String));
                dt.Columns.Add("guard_middle", typeof(String));
                dt.Columns.Add("guard_last", typeof(String));
                dt.Columns.Add("guard_password", typeof(String));
                dt.Columns.Add("guard_phone", typeof(String));

                //dgView.DataSource = dt;
                //where objDetails.gate_no == global.m_Gate_No

               

              

                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public DataTable FillManagers()
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("guard_id", typeof(String));
                dt.Columns.Add("guard_first", typeof(String));
                dt.Columns.Add("guard_middle", typeof(String));
                dt.Columns.Add("guard_last", typeof(String));
                dt.Columns.Add("guard_password", typeof(String));
                dt.Columns.Add("guard_phone", typeof(String));

                //dgView.DataSource = dt;
                //  && objDetails.gate_no == global.m_Gate_No

                using (IVISSEntities db = new IVISSEntities())
                {

                    var detailQuery = from objDetails in db.Admins
                                      where objDetails.admin_id.ToLower() != Global.USER_NAME.ToLower()
                                      where objDetails.admin_type == "Manager"
                                      select new { objDetails.admin_id, objDetails.admin_first, objDetails.admin_middle, objDetails.admin_last, objDetails.admin_password, objDetails.admin_phone };

                    DataRow dr;

                    foreach (var detail in detailQuery)
                    {
                        dr = dt.NewRow();
                        dr[0] = detail.admin_id;
                        dr[1] = detail.admin_first;
                        dr[2] = detail.admin_middle;
                        dr[3] = detail.admin_last;
                        dr[4] = detail.admin_password;
                        dr[5] = detail.admin_phone;

                        dt.Rows.Add(dr);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        public bool RecordExists(string guardID, string selectedID)
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var query = (from g in db.Guards
                             where g.guard_id == guardID && g.guard_id != selectedID
                             select g).FirstOrDefault();

                if (query != null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool RecordExistsAdmin(string guardID, string selectedID)
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var query = (from g in db.Admins
                             where g.admin_id == guardID && g.admin_id != selectedID
                             select g).FirstOrDefault();

                if (query != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void InsertGuard()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var g = new Guard();

                g.guard_first = firstName;
                g.guard_middle = middleName;
                g.guard_last = lastName;

                g.guard_phone_1 = phone;
                g.guard_id = id;
                g.guard_password = StringCipher.Encrypt(password, Global.PASSPHRASE);

                db.Guards.Add(g);
                db.SaveChanges();
            }
        }

        public void InsertManager()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var adm = new Admin();

                adm.admin_first = firstName;
                adm.admin_middle = middleName;
                adm.admin_last = lastName;

                adm.admin_phone = phone;
                adm.admin_id = id;
                adm.admin_password = StringCipher.Encrypt(password, Global.PASSPHRASE);

                adm.admin_type = "Manager";

                //adm.gate_no = (byte)global.m_Gate_No;

                db.Admins.Add(adm);
                db.SaveChanges();
            }
        }

        public void UpdateGuard()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var query = (from g in db.Guards
                             where g.guard_id == selectedID
                             select g).FirstOrDefault();

                if (query != null)
                {
                    query.guard_first = firstName;
                    query.guard_middle = middleName;
                    query.guard_last = lastName;

                    query.guard_phone_1 = phone;
                   // query.guard_id = id;
                    query.guard_password = StringCipher.Encrypt(password, Global.PASSPHRASE);

                  

                    db.SaveChanges();

                    string strsql = "update Guard set guard_id='"+ id + "' where guard_id='"+ selectedID + "'";
                    db.Database.ExecuteSqlCommand(strsql);
                }
            }
        }

        public void UpdateManager()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var adm = (from a in db.Admins
                           where a.admin_id == selectedID
                           select a).FirstOrDefault();

                if (adm != null)
                {
                    adm.admin_first = firstName;
                    adm.admin_middle = middleName;
                    adm.admin_last = lastName;

                    adm.admin_phone = phone;
                   // adm.admin_id = id;
                    adm.admin_password = StringCipher.Encrypt(password, Global.PASSPHRASE);

                    adm.admin_type = "Manager";

                    //adm.gate_no = (byte)global.m_Gate_No;

                    db.SaveChanges();


                    string strsql = "update Admin set admin_id='" + id + "' where admin_id='" + selectedID + "'";
                    db.Database.ExecuteSqlCommand(strsql);
                }
            }
        }

        public void DeleteGuard()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var query = (from g in db.Guards
                             where g.guard_id == id
                             select g).FirstOrDefault();

                if (query != null)
                {
                    db.Guards.Remove(query);
                    db.SaveChanges();
                }
            }
        }

        public void DeleteManager()
        {
            using (IVISSEntities db = new IVISSEntities())
            {
                var adm = (from a in db.Admins
                           where a.admin_id == id
                           select a).FirstOrDefault();

                if (adm != null)
                {
                    db.Admins.Remove(adm);
                    db.SaveChanges();
                }
            }
        }
    }
}
