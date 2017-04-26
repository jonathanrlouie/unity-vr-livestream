using UnityEngine.Networking;

namespace VoiceChat.Networking
{
    class VoiceChatMsgType
    {
        public const short Base = MsgType.Highest;

        public const short RequestProxy = Base + 1;
        public const short SpawnProxy   = Base + 2;
        public const short Packet       = Base + 3;
        public const short RequestTutor = Base + 4;
        public const short StudentRequestTutor = Base + 5;
        public const short RequestStudent = Base + 6;
        public const short RequestStudentDrop = Base + 7;
        public const short EndCall = Base + 8;
        public const short RemoveUser = Base + 9;
        public const short AddUser = Base + 10;
        public const short PopulateUserList = Base + 11;
    }
}
