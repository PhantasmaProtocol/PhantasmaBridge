using Neo.Lux.Cryptography;
using Neo.Lux.Utils;

namespace Phantasma.Bridge.Core
{
    public class Mailbox
    {
        public readonly string name;
        public readonly UInt160 hash;
        public readonly string address;

        public Mailbox(string name, UInt160 hash)
        {
            this.name = name;
            this.hash = hash;
            this.address = hash.ToAddress();
        }

        public static bool ValidateMailboxName(byte[] mailbox_name)
        {
            if (mailbox_name.Length <= 4 || mailbox_name.Length >= 20)
                return false;

            int index = 0;
            while (index < mailbox_name.Length)
            {
                var c = mailbox_name[index];
                index++;

                if (c >= 97 && c <= 122) continue; // lowercase allowed
                if (c == 95) continue; // underscore allowed
                if (c >= 48 && c <= 57) continue; // numbers allowed

                return false;
            }

            return true;
        }
    }
}
