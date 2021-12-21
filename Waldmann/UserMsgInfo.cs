using System;
using System.Collections.Generic;
using System.Text;

namespace Waldmann
{
    public class UserMsgInfo
    {   
        public int MsgID { get; set; }

        public string name { get; set; }

        public string nachname { get; set; }

        public int UID { get; set; }

        public string msgContent { get; set; }

        public int sentToUID { get; set; }

        public UserMsgInfo(int mMsgID, string mName, string mNachname, int mUID, string mMsgContent, int mSentToUID)
        {
            MsgID = mMsgID;
            name = mName + " " + mNachname;
            UID = mUID;
            msgContent = mMsgContent;
            sentToUID = mSentToUID;
        }

        public override string ToString()
        {
            return string.Format("MsgID: {0} \nname:{1} \nusername: {2} \nUID: {3} \nmsgContent: {4} \nsentToUID: {5}", MsgID, name, nachname, UID, msgContent, sentToUID);
        }
    }
    
}
