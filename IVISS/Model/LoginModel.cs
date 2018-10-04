using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVISS.Model
{
    class LoginModel
    {

        public string username { set; get; }
        public string password { set; get; }

        public string Login()
        {
            using (var db = new IVISSEntities())
            {
                var adm = (from p in db.Admins
                           where (p.admin_id == username && p.admin_password == password)
                           select p).FirstOrDefault();

                if (adm != null)
                {
                    return adm.admin_type.ToUpper();
                }

                var mng = (from p in db.Guards
                           where p.guard_id == username && p.guard_password == password
                           select p).FirstOrDefault();

                if (mng != null)
                {
                    return "GUARD";
                }
            }

            return string.Empty;
        }
    }
}
