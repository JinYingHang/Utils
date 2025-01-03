﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    /// <summary>    
    /// 类名：RSAFromPkcs8    
    /// 功能：RSA加密、解密、签名、验签 (支持1024位和2048位私钥)    
    /// 详细：该类对Java生成的密钥进行解密和签名以及验签专用类，不需要修改   
    /// </summary>
    public sealed class RSAUtil_PKCS
    {
        /**
         * RSA最大解密密文大小
         *     注意：这个和密钥长度有关系, 公式= 密钥长度 / 8
         */
        private const int MAX_DECRYPT_BLOCK = 128;

        /// <summary>    
        /// 签名    
        /// </summary>    
        /// <param name="content">待签名字符串</param>        
        /// <param name="input_charset">编码格式</param>    
        /// <returns>签名后字符串</returns>    
        public static string sign(string content, string privateKey, string input_charset) {
            byte[] Data = Encoding.GetEncoding(input_charset).GetBytes(content);
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] signData = rsa.SignData(Data, sh);
            return Convert.ToBase64String(signData);
        }

        /// <summary>    
        /// 验签    
        /// </summary>    
        /// <param name="content">待验签字符串</param>    
        /// <param name="signedString">签名</param>     
        /// <param name="input_charset">编码格式</param>    
        /// <returns>true(通过)，false(不通过)</returns>    
        public static bool verify(string content, string signedString, string publicKey, string input_charset) {
            bool result = false;
            byte[] Data = Encoding.GetEncoding(input_charset).GetBytes(content);
            byte[] data = Convert.FromBase64String(signedString);
            RSAParameters paraPub = ConvertFromPublicKey(publicKey);
            RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
            rsaPub.ImportParameters(paraPub);
            SHA1 sh = new SHA1CryptoServiceProvider();
            result = rsaPub.VerifyData(Data, sh, data);
            return result;
        }

        /// <summary>    
        /// 加密    
        /// </summary>    
        /// <param name="resData">需要加密的字符串</param>     
        /// <param name="input_charset">编码格式</param>    
        /// <returns>明文</returns>    
        public static string encryptData(string resData, string publicKey, string input_charset) {
            byte[] DataToEncrypt = Encoding.ASCII.GetBytes(resData);
            string result = encrypt(DataToEncrypt, publicKey, input_charset);
            return result;
        }


        /// <summary>    
        /// 解密    
        /// </summary>    
        /// <param name="resData">加密字符串</param>    
        /// <param name="privateKey">私钥</param>    
        /// <param name="input_charset">编码格式</param>    
        /// <returns>明文</returns>    
        public static string decryptData(string resData, string privateKey, string input_charset) {
            byte[] DataToDecrypt = Convert.FromBase64String(resData);
            string result = "";
            for (int j = 0; j < DataToDecrypt.Length / MAX_DECRYPT_BLOCK; j++) {
                byte[] buf = new byte[MAX_DECRYPT_BLOCK];
                for (int i = 0; i < MAX_DECRYPT_BLOCK; i++) {
                    buf[i] = DataToDecrypt[i + MAX_DECRYPT_BLOCK * j];
                }
                result += decrypt(buf, privateKey, input_charset);
            }
            return result;
        }

        #region 内部方法

        private static string encrypt(byte[] data, string publicKey, string input_charset) {
            RSACryptoServiceProvider rsa = DecodePemPublicKey(publicKey);
            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] result = rsa.Encrypt(data, false);

            return Convert.ToBase64String(result);
        }

        private static string decrypt(byte[] data, string privateKey, string input_charset) {
            string result = "";

            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
            //--- Edit for testing by lzyan ---
            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //RSAParameters paraPri = ConvertFromPrivateKey(privateKey);
            //rsa.ImportParameters(paraPri);
            //--- Edit End ---

            SHA1 sh = new SHA1CryptoServiceProvider();
            byte[] source = rsa.Decrypt(data, false);
            char[] asciiChars = new char[Encoding.GetEncoding(input_charset).GetCharCount(source, 0, source.Length)];
            Encoding.GetEncoding(input_charset).GetChars(source, 0, source.Length, asciiChars, 0);
            result = new string(asciiChars);
            //result = ASCIIEncoding.ASCII.GetString(source);    
            return result;
        }

        private static RSACryptoServiceProvider DecodePemPublicKey(string pemstr) {
            byte[] pkcs8publickkey;
            pkcs8publickkey = Convert.FromBase64String(pemstr);
            if (pkcs8publickkey != null) {
                RSACryptoServiceProvider rsa = DecodeRSAPublicKey(pkcs8publickkey);
                return rsa;
            }
            else
                return null;
        }

        private static RSACryptoServiceProvider DecodePemPrivateKey(string pemstr) {
            byte[] pkcs8privatekey;
            pkcs8privatekey = Convert.FromBase64String(pemstr);
            if (pkcs8privatekey != null) {
                RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                return rsa;
            }
            else
                return null;
        }

        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8) {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading    
            byte bt = 0;
            ushort twobytes = 0;

            try {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes    
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);        //read the Sequence OID    
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct    
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)    //expect an Octet string    
                    return null;

                bt = binr.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count    
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                    binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key    

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception) {
                return null;
            }

            finally { binr.Close(); }

        }

        private static bool CompareBytearrays(byte[] a, byte[] b) {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a) {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static RSACryptoServiceProvider DecodeRSAPublicKey(byte[] publickey) {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"    
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------    
            MemoryStream mem = new MemoryStream(publickey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading    
            byte bt = 0;
            ushort twobytes = 0;

            try {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes    
                else
                    return null;

                seq = binr.ReadBytes(15);       //read the Sequence OID    
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct    
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8203)
                    binr.ReadInt16();   //advance 2 bytes    
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)     //expect null byte next    
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes    
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)    
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus    
                else if (twobytes == 0x8202) {
                    highbyte = binr.ReadByte(); //advance 2 bytes    
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order    
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00) {   //if first byte (highest order) of modulus is zero, don't include it    
                    binr.ReadByte();    //skip this null byte    
                    modsize -= 1;   //reduce modulus buffer size by 1    
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes    

                if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data    
                    return null;
                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)    
                byte[] exponent = binr.ReadBytes(expbytes);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----    
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters();
                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception) {
                return null;
            }

            finally { binr.Close(); }

        }

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey) {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------    
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading    
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)    
                    binr.ReadByte();    //advance 1 byte    
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes    
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)    //version number    
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------  all private key components are Integer sequences ----    
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----    
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception) {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr) {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)        //expect integer    
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte    
            else
                if (bt == 0x82) {
                highbyte = binr.ReadByte();    // data size in next 2 bytes    
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else {
                count = bt;        // we already have the data size    
            }

            while (binr.ReadByte() == 0x00) {    //remove high order zeros in data    
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte    
            return count;
        }

        #endregion

        #region 解析.net 生成的Pem
        private static RSAParameters ConvertFromPublicKey(string pemFileConent) {
            if (string.IsNullOrEmpty(pemFileConent)) {
                throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
            }
            pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");
            byte[] keyData = Convert.FromBase64String(pemFileConent);
            bool keySize1024 = (keyData.Length == 162);
            bool keySize2048 = (keyData.Length == 294);
            if (!(keySize1024 || keySize2048)) {
                throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
            }
            byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, (keySize1024 ? 29 : 33), pemModulus, 0, (keySize1024 ? 128 : 256));
            Array.Copy(keyData, (keySize1024 ? 159 : 291), pemPublicExponent, 0, 3);
            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            return para;
        }

        /// <summary>  
        /// 将pem格式私钥(1024 or 2048)转换为RSAParameters  
        /// </summary>  
        /// <param name="pemFileConent">pem私钥内容</param>  
        /// <returns>转换得到的RSAParamenters</returns>  
        private static RSAParameters ConvertFromPrivateKey(string pemFileConent) {
            if (string.IsNullOrEmpty(pemFileConent)) {
                throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
            }
            pemFileConent = pemFileConent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");
            byte[] keyData = Convert.FromBase64String(pemFileConent);

            bool keySize1024 = (keyData.Length == 609 || keyData.Length == 610);
            bool keySize2048 = (keyData.Length == 1190 || keyData.Length == 1192);

            if (!(keySize1024 || keySize2048)) {
                throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
            }

            int index = (keySize1024 ? 11 : 12);
            byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);

            index += pemModulus.Length;
            index += 2;
            byte[] pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);

            index += 3;
            index += 4;
            if ((int)keyData[index] == 0) {
                index++;
            }
            byte[] pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);

            index += pemPrivateExponent.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);

            index += pemPrime1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);

            index += pemPrime2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);

            index += pemExponent1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);

            index += pemExponent2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            byte[] pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);

            RSAParameters para = new RSAParameters();
            para.Modulus = pemModulus;
            para.Exponent = pemPublicExponent;
            para.D = pemPrivateExponent;
            para.P = pemPrime1;
            para.Q = pemPrime2;
            para.DP = pemExponent1;
            para.DQ = pemExponent2;
            para.InverseQ = pemCoefficient;
            return para;
        }
        #endregion

        //extract valid content from PEM file
        public static string extractFromPemFile(string filePath) {
            string result = "";
            string COMMENT_BEGIN_FLAG = "-----";
            //=============
            //从头到尾以流的方式读出文本文件, 该方法会一行一行读出文本
            using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath, Encoding.UTF8)) {
                string str;
                StringBuilder sb = new StringBuilder();
                while ((str = sr.ReadLine()) != null) {
                    if (!str.StartsWith(COMMENT_BEGIN_FLAG)) {
                        sb.Append(str);
                    }
                }
                result = sb.ToString();
            }
            return result;
        }

        //extract valid content from PEM format string
        public static string extractFromPemFormat(string input) {
            string result = "";
            string COMMENT_BEGIN_FLAG = "-----";
            string[] splitFlagArray = { "\r", "\n" };
            string[] itemArray = input.Split(splitFlagArray, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            foreach (string item in itemArray) {
                if (!item.StartsWith(COMMENT_BEGIN_FLAG)) {
                    sb.Append(item);
                }
            }
            result = sb.ToString();

            return result;
        }
    }
}
