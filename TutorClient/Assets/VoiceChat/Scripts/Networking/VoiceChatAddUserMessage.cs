using UnityEngine;
using UnityEngine.Networking;

namespace VoiceChat.Networking
{
    public class VoiceChatAddUserMessage : MessageBase
    {
        private short proxyId;
        private User user;

        public short ProxyId
        {
            get
            {
                return proxyId;
            }
        }

        public User User
        {
            get
            {
                return user;
            }
        }

        public VoiceChatAddUserMessage() { }
        public VoiceChatAddUserMessage(short proxyId, User user)
        {
            this.proxyId = proxyId;
            this.user = user;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(proxyId);
            writer.Write(user.Username);
            writer.Write(user.Role);
        }

        public override void Deserialize(NetworkReader reader)
        {
            proxyId = reader.ReadInt16();
            string username = reader.ReadString();
            string role = reader.ReadString();
            user = new User(username, role);
        }
    }
}
