using System;
using System.Diagnostics;

using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.OpenSsl;
using System.IO;

namespace GSTSecurity
{

    // Cyber mistake exceptions
    public class GSTSecurityException : Exception
    {
        public GSTSecurityException(string message) : base(message) { }
    }


    /**
     * Configuration Certification
     * An ECC Key Pair: Private Key signs the Configurations
     */
    public class ConfigCert
    {
        public static X9ECParameters ecParams = NistNamedCurves.GetByName("P-192"); // secp 192r1
        public static ECDomainParameters ecParameters = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H, ecParams.GetSeed());

        // The underlying key pair
        private readonly AsymmetricCipherKeyPair pair;

        // Random generation
        private readonly SecureRandom random = new SecureRandom();

        // No source file specified -- generate new key pair
        public ConfigCert()
        {
            ECKeyPairGenerator pGen = new ECKeyPairGenerator();
            ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(ecParameters, random);
            pGen.Init(genParam);
            pair = pGen.GenerateKeyPair();
        }

        // Load Private key 
        public ConfigCert(KeyManager km)
        {
            pair = km.SharePair();
        }

        // Return the signature of the passed Configuration as 2 Big Integers
        public BigInteger[] Signature(Configuration conf)
        {
            ParametersWithRandom param = new ParametersWithRandom(pair.Private, random);
            ECDsaSigner ecdsa = new ECDsaSigner(); // Only SHA-1 Compatible!!
                                                   // ECDsaSigner ecdsa = new ECDsaSigner(new HMacDsaKCalculator(new Sha256DiSgest())); // micro - ecc issue
            ecdsa.Init(true, param);
            BigInteger[] sig = ecdsa.GenerateSignature(conf.Digest());
            return sig;
        }

        // Configuration Signature returned as Base64
        public string SignatureBase64(Configuration conf)
        {
            byte[] sig = new byte[48];
            BigInteger[] result = Signature(conf);
            byte[] b1 = result[0].ToByteArrayUnsigned();
            System.Buffer.BlockCopy(b1, 0, sig, 0, b1.Length);
            byte[] b2 = result[1].ToByteArrayUnsigned();
            System.Buffer.BlockCopy(b2, 0, sig, 24, b2.Length);
            return Convert.ToBase64String(sig);
        }

        // Returns a byte stream of the signed configuration
        // r , s, digest, configuration
        public byte[] SignedConfiguration(Configuration conf)
        {
            byte[] stream = new byte[48 + conf.message.Length]; //  secpr192 sig size == 24 * 2
            BigInteger[] sig = Signature(conf);
            for (int j = 0; j < 2; j++)
            {
                byte[] src = sig[j].ToByteArrayUnsigned();
                for (int i = 0; i < 24; i++)
                {
                    stream[(j * 24) + i] = src[i];
                }
            }
            /*  byte[] digest = conf.Digest(); // Only for debugging usage ! insecure !
              for(int i = 0; i < 20; i++)
              {
                  stream[48 + i] = digest[i];
              } */
            for (int i = 0; i < conf.message.Length; i++)
            {
                stream[48 + i] = conf.message[i];
            }
            return stream;
        }

        // get clear x,y rep for public key
        public BigInteger[] PublicKey()
        {
            byte[] key = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pair.Public).GetEncoded();
            ECPublicKeyParameters bpubKey = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(key);
            ECPoint q = bpubKey.Q;
            BigInteger[] tuple = new BigInteger[2];
            tuple[0] = q.XCoord.ToBigInteger();
            tuple[1] = q.YCoord.ToBigInteger();
            return tuple;
        }

        // Base64 encoding of public key
        // Add other encodings depending on your own need
        public String PublicKeyBase64()
        {
            byte[] pbk = new byte[48];
            BigInteger[] pubkey = PublicKey();
            byte[] a1 = pubkey[0].ToByteArrayUnsigned();
            System.Buffer.BlockCopy(a1, 0, pbk, 0, a1.Length);
            byte[] a2 = pubkey[1].ToByteArrayUnsigned();
            System.Buffer.BlockCopy(a2, 0, pbk, 24, a2.Length);
            return Convert.ToBase64String(pbk);
        }
    }


    /**
     * A Configuration
     */
    public class Configuration
    {
        public readonly byte[] message;
        private readonly Sha1Digest digester = new Sha1Digest(); // use SHA1 for conf digesting

        // Create Configuration from byte representation of config
        public Configuration(byte[] config)
        {
            message = config;
        }

        // get A digest for signing
        public byte[] Digest()
        {
            digester.BlockUpdate(message, 0, message.Length);
            byte[] digest = new byte[digester.GetDigestSize()];
            digester.DoFinal(digest, 0);
            return digest;
        }
    }



    /**
     * Crypto Key Manager
     */
    public class KeyManager
    {

        /**
         * BouncyCastle helper class
         * From Maarten Bodewes @ Stack Overflow
         */
        private class PasswordFinder : IPasswordFinder
        {
            private string password;

            public PasswordFinder(string password)
            {
                this.password = password;
            }


            public char[] GetPassword()
            {
                return password.ToCharArray();
            }
        }

        private readonly string filepath;
        private AsymmetricCipherKeyPair pair;

        // store password protected PEM
        private void StoreKeyFile(string filepath, string password)
        {
            TextWriter textWriter = new StreamWriter(filepath, true);
            PemWriter pem = new PemWriter(textWriter);
            pem.WriteObject(pair.Private, "AES-256-CBC", password.ToCharArray(), new SecureRandom());
            textWriter.Close();
        }

        // Read password protected PEM
        private AsymmetricCipherKeyPair ReadKeyFile(string filepath, string password)
        {
            TextReader reader = File.OpenText(filepath);
            PemReader pem = new PemReader(reader, new PasswordFinder(password));
            AsymmetricCipherKeyPair keys = ((AsymmetricCipherKeyPair)pem.ReadObject());
            reader.Close();
            return keys;
            /* ECPrivateKeyParameters priv = 
             BigInteger d = new BigInteger(priv.D.ToByteArray());
             ECPoint q = ConfigCert.ecParameters.G.Multiply(d);
             ECPublicKeyParameters pub = new ECPublicKeyParameters(q,ConfigCert.ecParameters);
             return new AsymmetricCipherKeyPair(pub, priv); */
        }

        // create a manager by means of assigning it to a path
        // If the path exists it will attempt to read it.
        // If not it will create a new pair
        public KeyManager(string path, string passw)
        {
            filepath = System.IO.Path.GetFullPath(path);
            if (File.Exists(filepath))
            {
                pair = ReadKeyFile(path, passw);
            }
            else
            {
                ECKeyPairGenerator pGen = new ECKeyPairGenerator();
                ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(ConfigCert.ecParameters, new SecureRandom());
                pGen.Init(genParam);
                pair = pGen.GenerateKeyPair();
                StoreKeyFile(path, passw);
            }
        }

        // share a key pair
        public AsymmetricCipherKeyPair SharePair()
        {
            return pair;
        }
    }


    /**
     * Time based One time password generator
     * > Create one per device!
     * > Every device should have it's own unique secret
     */
    public class TOTPGenerator
    {
        public const int SECRET_SIZE = 32;

        // secret <-> id
        private readonly byte[] sharedsecret;
        private readonly long device_id;

        // Random generator
        private static readonly SecureRandom random = new SecureRandom();

        // TOTP with SHA 256
        private readonly Sha256Digest digester = new Sha256Digest(); // use SHA1 for conf digesting

        // Helper function to generator random secrets
        public static byte[] SecretGenerator()
        {
            byte[] secret = new byte[SECRET_SIZE];
            random.NextBytes(secret);
            return secret;
        }

        // @id should be some device identifier
        public TOTPGenerator(byte[] secret, long id)
        {
            if (secret.Length != SECRET_SIZE)
            {
                throw (new GSTSecurityException("Secret is not the right the size"));
            }
            sharedsecret = secret;
            device_id = id;
        }

        /**
         * hotp_value
         * TOTP is HOTP (RFC 4226) with x as time value
         */
        private int Hotp_Value(byte[] x)
        {
            byte[] combo = new byte[40];
            System.Buffer.BlockCopy(x, 0, combo, 0, x.Length);
            System.Buffer.BlockCopy(sharedsecret, 0, combo, x.Length, SECRET_SIZE);
            digester.BlockUpdate(combo, 0, combo.Length);
            byte[] digest = new byte[digester.GetDigestSize()];
            digester.DoFinal(digest, 0);

            // get last 31 bits
            byte[] last4 = new byte[4];
            System.Buffer.BlockCopy(digest, 28, last4, 0, 4);
            last4[0] = (byte)(last4[0] & 0x7f); // disable most significant byte

            // get last 8 digits
            uint src = BitConverter.ToUInt32(last4, 0);
            int result = (int)(src % 100000000);
            return result;
        }

        /**
         *  Time based 1-time Password
         * ----------------------------
         * RFC 6238 for time step (X) == 1 day
         * Returns: the totp code for [-1,1,+1] to deal with time sync issues
         */
        public int[] Generate_Code()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long daysSinceEpoch = (long)t.TotalDays;
            int[] triple = new int[3];
            for (int i = -1; i <= 1; i++)
            {
                byte[] brep = BitConverter.GetBytes(daysSinceEpoch + i);
                triple[i + 1] = Hotp_Value(brep);
            }
            return triple;
        }
    }
}
