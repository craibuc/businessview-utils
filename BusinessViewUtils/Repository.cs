using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//TODO: add `using` statements for CrystalEntprise namespaces

namespace BusinessViewUtils
{
    class Repository
    {
        // get an InfoStore object
        public static InfoStore Authenticate(String account, String password, String serverName, String authType = "secEnterprise")
        {

            try
            {
                CrystalEnterprise.SessionMgr sessionMgr;
                CrystalEnterprise.EnterpriseSession session;
                session = sessionMgr.Logon(account, password, serverName, authType);

                return session.Service("", "InfoStore");

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        // query InfoStore; return InfoObjects collection
        public static InfoObjects Query(Object infoStore, String commandText)
        {
            return infoStore.query(commandText);
        }

    }
}
