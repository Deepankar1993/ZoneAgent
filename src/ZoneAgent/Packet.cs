﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZoneAgent
{
    /// <summary>
    /// Class for creating and  manipulating packets
    /// </summary>
    class Packet
    {
        /// <summary>
        /// Creating packet to connect to login server
        /// </summary>
        /// <returns>returns packet to connect to loginserver</returns>
        public static byte[] LoginServerConnectPacket()
        {
            var packet = CombineByteArray(new byte[] { 0x20 }, GetBytesFrom(GetNullString(7)));
            packet = CombineByteArray(packet, new byte[] { 0x02, 0xe0 });
            string aid = string.Format("{0:x}", Config.AGENT_ID);
            string sid = string.Format("{0:x}", Config.SERVER_ID);
            var tempByte = new[] { Convert.ToByte(sid, 16) };
            packet = CombineByteArray(packet, new[] { tempByte[0] });
            tempByte = new[] { Convert.ToByte(aid, 16) };
            packet = CombineByteArray(packet, new[] { tempByte[0] });
            packet = CombineByteArray(packet, GetBytesFrom(Config.ZA_IP.ToString()));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(16 - Config.ZA_IP.ToString().Length)));
            packet = CombineByteArray(packet, CreateReverseHexPacket(Config.ZA_PORT));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(2)));
            return packet;
        }
        /// <summary>
        /// Creating packet to connect to AccountServer,ZoneServer,BattleServer
        /// </summary>
        /// <returns>Returns packet to connect to Zone</returns>
        public static byte[] ZoneConnectPacket()
        {
            byte[] packet = new byte[] { 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0xE0};
            string aid = string.Format("{0:x}", Config.AGENT_ID);
            var tempByte = new[] { Convert.ToByte(aid, 16) };
            packet = CombineByteArray(packet, new[] { tempByte[0] });
            return packet;
        }
        /// <summary>
        /// Adding reverse of Client ID to the packet received from client
        /// </summary>
        /// <param name="pack">packet data</param>
        /// <param name="id">client id</param>
        /// <param name="size">size of packet</param>
        /// <returns>returns packet with ClientId contains in it</returns>
        public static byte[] AddClientID(byte[] pack, int id, int size)
        {
            byte[] packet = new byte[size];
            Array.Copy(pack, 0, packet, 0, 4);
            var temp = CreateReverseHexPacket(id);
            Array.Copy(temp, 0, packet, 4, temp.Length);
            Array.Copy(pack, 8, packet, 8, size - 8);
            return packet;
        }

        /// <summary>
        /// Gets character name from packet starting from specified index value
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <param name="index">index value</param>
        /// <returns>returns string fetched from packet data</returns>
        public static string GetCharName(byte[] packet, int index)
        {
            string character = "";
            while (packet[index] != '\0')
            {
                character += ((char)packet[index]).ToString();
                index++;
            }
            return character;
        }
        /// <summary>
        /// Gets Client ID from packet
        /// </summary>
        /// <param name="data">packet data</param>
        /// <returns>returns integer value that represents Client ID</returns>
        public static int GetClientId(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }
        /// <summary>
        /// Alter charcter packet received from AccountServer(.acl file) according to 562 client
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <returns>returns 952 bytes packet that contains character info compatible</returns>
        public static byte[] AlterAccountServerPacket(byte[] packet)
        {
            var tempbytes = Crypt.Decrypt(packet);
            tempbytes[35] = tempbytes[34];
            tempbytes[34] = tempbytes[33];
            tempbytes[33] = Convert.ToByte(1);
            tempbytes[32] = 0x00;
            tempbytes[223] = tempbytes[222];
            tempbytes[222] = tempbytes[221];
            tempbytes[221] = Convert.ToByte(1);
            tempbytes[220] = 0x00;
            tempbytes[411] = tempbytes[410];
            tempbytes[410] = tempbytes[409];
            tempbytes[409] = Convert.ToByte(1);
            tempbytes[408] = 0x00;
            tempbytes[599] = tempbytes[598];
            tempbytes[598] = tempbytes[597];
            tempbytes[597] = Convert.ToByte(1);
            tempbytes[596] = 0x00;
            tempbytes[787] = tempbytes[786];
            tempbytes[786] = tempbytes[785];
            tempbytes[785] = Convert.ToByte(1);
            tempbytes[784] = 0x00;
            return Crypt.Encrypt(tempbytes);
        }

        /// <summary>
        /// Create client status packet to send to LoginServer
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <param name="accountId">account name</param>
        /// <returns>returns client status info packet to be send to loginserver</returns>
        public static byte[] CreateClientStatusPacket(int clientId, string accountId)
        {
            var packet = new byte[4] { 0x1F, 0x00, 0x00, 0x00 };
            packet = CombineByteArray(packet, CreateReverseHexPacket(clientId));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(8 - packet.Length)));
            var temp = new byte[] { 0x02, 0xe3 };
            packet = CombineByteArray(packet, CombineByteArray(temp, GetBytesFrom(accountId)));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(31 - packet.Length)));
            return packet;
        }
        /// <summary>
        /// Create packet to get characters from Accountserver (this packet will be send to AccountServer)
        /// </summary>
        /// <param name="clientId">client Id</param>
        /// <param name="accountId">account name</param>
        /// <param name="ip">ip address of client</param>
        /// <returns>returns packet to get characters</returns>
        public static byte[] CreateGetCharacterPacket(int clientId, string accountId, string ip)
        {
            var packet = new byte[] { 0x92, 0x00, 0x00, 0x00 };
            packet = CombineByteArray(packet, CreateReverseHexPacket(clientId));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(8 - packet.Length)));
            packet = CombineByteArray(packet, new byte[] { 0x01, 0xE1 });
            packet = CombineByteArray(packet, GetBytesFrom(accountId));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(52 - packet.Length)));
            packet[34] = 0x01;
            packet = CombineByteArray(packet, GetBytesFrom(ip));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(68 - packet.Length)));
            packet = CombineByteArray(packet, new byte[] { 0x1A, 0x00, 0x70, 0xDD, 0x18, 0x00, 0x00, 0x00, 0x69, 0x77, 0xF0, 0xDA, 0x93 });
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(146 - packet.Length)));
            return packet;
        }
        /// <summary>
        /// Report to LS total no. of players online every 5 seconds
        /// </summary>
        /// <returns>Packet to send report packet to loginserver</returns>
        public static byte[] LSReporter()
        {
            byte[] packet = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xE1 };
            packet = CombineByteArray(packet, CreateReverseHexPacket(Config.PLAYER_COUNT));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(14 - packet.Length)));
            packet = CombineByteArray(packet, new byte[] { 0x03, 0x03 });
            return packet;
        }
        /// <summary>
        /// Gets current timestamp and returns it
        /// </summary>
        /// <returns>current timestamp</returns>
        public static string GetTime()
        {
            string time = "" + DateTime.Now.Year;
            string temp = "" + DateTime.Now.Month;
            if (temp.Length == 1)
                temp = "0" + temp;
            time += temp;
            temp = "" + DateTime.Now.Day;
            if (temp.Length == 1)
                temp = "0" + temp;
            time += temp;
            time += '\0';
            temp = "" + DateTime.Now.Hour;
            if (temp.Length == 1)
                temp = "0" + temp;
            time += temp;
            temp = "" + DateTime.Now.Minute;
            if (temp.Length == 1)
                temp = "0" + temp;
            time += temp;
            temp = "" + DateTime.Now.Second;
            if (temp.Length == 1)
                temp = "0" + temp;
            time += temp;
            if (time.Length < 16)
            {
                for (int i = time.Length; i < 16; i++)
                    time += '\0';
            }
            return time;
        }

        /// <summary>
        /// Disconnect packet to send to LoginServer to disconnect player from LoginServer
        /// </summary>
        /// <param name="clientid">client id</param>
        /// <param name="username">account name</param>
        /// <param name="time">timestamp</param>
        /// <returns>packet to send disconnect request to loginserver</returns>
 
        public static byte[] SendDCToLS(int clientid,string username,string time)
        {
            byte[] packet = new byte[] { 0x30, 0x00, 0x00, 0x00 };
            packet = CombineByteArray(packet, CreateReverseHexPacket(clientid));
            packet = CombineByteArray(packet,GetBytesFrom(GetNullString(8-packet.Length)));
            packet = CombineByteArray(packet, new byte[] { 0x02,0xE2,0x00});
            packet = CombineByteArray(packet, GetBytesFrom(username));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(32 - packet.Length)));
            packet = CombineByteArray(packet,GetBytesFrom(GetTime()));
            return packet;
        }
        /// <summary>
        /// Disconnect packet to send to ZoneServer to disconnect player from ZoneServer
        /// </summary>
        /// <param name="clientid">client id</param>
        /// <returns>packet to disconnect character from zoneserver</returns>
        public static byte[] SendDCToASZS(int clientid)
        {
            byte[] packet = new byte[] { 0x0B, 0x00, 0x00, 0x00 };
            packet = CombineByteArray(packet, CreateReverseHexPacket(clientid));
            packet = CombineByteArray(packet, GetBytesFrom(GetNullString(8 - packet.Length)));
            packet = CombineByteArray(packet, new byte[] { 0x01, 0xE2, 0x00 });
            return packet;
        }
        /// <summary>
        /// Get packet type from packet
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <param name="length">length of packet</param>
        /// <returns>packet type (to which server packet is to be send)</returns>
        public static int GetPacketType(byte[] packet,int length)
        {
            int packetType=Config.ZS_PACKET;
            if (length == 0 || length <= 10)//For packet length 0 or <=10
                packetType = Config.INVALID;
            else if (packet[8] == 0x01 && packet[9] == 0xE2)//Login Packet
                packetType = Config.LOGIN_PACKET;
            else if (packet[10] == 0x06 && packet[11] == 0x11)//Validating character to enter game
                packetType = Config.AS_PACKET;
            else if (packet[10] == 0x08 && packet[11] == 0x11)//Disconnet Packet
                packetType = Config.DISCONNECT_PACKET;
            else if (packet[10] == 0x01 && packet[11] == 0xA0)//Create char packet
                packetType = Config.AS_PACKET;
            else if (packet[10] == 0x02 && packet[11] == 0xA0)//Delete char packet
                packetType = Config.AS_PACKET;
            return packetType;
        }
        /// <summary>
        /// if client returns multiple packets in one packet then this nethod will split and add client id to each one of the packet and combine again
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <param name="clientId">client id</param>
        /// <param name="length">length of packet</param>
        /// <returns>packet with client id</returns>
        public static byte[] CheckForMultiplePackets(byte[] packet,int clientId,int length)
        {
            byte[] packetLength=new byte[4];
            Array.Copy(packet, packetLength, 4);
            int packetLen = GetClientId(packetLength);
            if (packetLen == length)
                return AddClientID(packet,clientId,length);
            else
            {
                int i=0;
                byte[] returnPacket = new byte[length];
                byte[] temp;
                while (i < length)
                {
                    Array.Copy(packet, i, packetLength, 0, 4);
                    packetLen = GetClientId(packetLength);
                    temp = new byte[packetLen];
                    Array.Copy(packet, i, temp, 0, packetLen);
                    Array.Copy(AddClientID(temp,clientId,temp.Length), 0, returnPacket, i, packetLen);
                    i+=packetLen;
                }
                //File.WriteAllBytes("Changed_" + Environment.TickCount + "_" + length, returnPacket);
                return returnPacket;
            }
        }
        /// <summary>
        /// Will split packet and add packet to packet list
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <param name="length">length of packet</param>
        /// <param name="spltPackets">list of packet by reference</param>
        public static void SplitPackets(byte[] packet, int length,ref List<byte[]> spltPackets)
        {
            byte[] packetLength = new byte[4];
            Array.Copy(packet, packetLength, 4);
            int packetLen = GetClientId(packetLength);
            if (packetLen == length)
                spltPackets.Add(packet);
            else
            {
                int i = 0;
                byte[] temp;
                while (i < length)
                {
                    Array.Copy(packet, i, packetLength, 0, 4);
                    packetLen = GetClientId(packetLength);
                    temp = new byte[packetLen];
                    Array.Copy(packet, i, temp, 0, packetLen);
                    spltPackets.Add(temp);
                    i += packetLen;
                }
            }
        }

        /// <summary>
        /// Combining 2 byte array
        /// </summary>
        /// <param name="a">byte array 1</param>
        /// <param name="b">byte array 2</param>
        /// <returns>returns combined byte array (byte array 1 + byte array 2)</returns>
        public static byte[] CombineByteArray(byte[] a, byte[] b)
        {
            var c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }
        /// <summary>
        /// getting byte[] from string
        /// </summary>
        /// <param name="str">string data</param>
        /// <returns>byte[] obtained from string data</returns>
        public static byte[] GetBytesFrom(string str)
        {
            var encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(str);
            return bytes;
        }
        /// <summary>
        /// Creating reverse byte[] of int value
        /// </summary>
        /// <param name="num">int value</param>
        /// <returns>reverse byte[] of int value</returns>
        private static byte[] CreateReverseHexPacket(int num)
        {
            if (num == 0)
                return new byte[] {0x00, 0x00};
            string hexPort = string.Format("{0:x}", num);
            while (hexPort.Length < 4)
                hexPort = "0" + hexPort;
            string temp = hexPort[2] + hexPort[3].ToString();
            string temp1 = hexPort[0] + hexPort[1].ToString();
            var tempByte = new[] { Convert.ToByte(temp, 16), Convert.ToByte(temp1, 16) };
            return tempByte;
        }
        /// <summary>
        /// Creating null string of specified no. of length
        /// </summary>
        /// <param name="length">length of packet</param>
        /// <returns>null string of specified length</returns>
        public static string GetNullString(int length)
        {
            string str = "";
            for (var i = 0; i < length; i++)
                str += char.ConvertFromUtf32(0);
            return str;
        }
        /// <summary>
        /// Triming packet according to length of packet
        /// </summary>
        /// <param name="packet">packet data</param>
        /// <param name="length">length of packet</param>
        /// <returns>trimed packet according to length specified</returns>
        public static byte[] TrimPacket(byte[] packet, int length)
        {
            var newPacket = new byte[] { 0x00 };
            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                    newPacket[i] = packet[i];
                else
                {
                    var temp = new[] { packet[i] };
                    newPacket = CombineByteArray(newPacket, temp);
                }
            }
            return newPacket;
        }
    }
}