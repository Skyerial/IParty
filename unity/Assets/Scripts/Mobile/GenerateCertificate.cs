using System;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

/**
 * @brief Generates and exports a self-signed X.509 certificate as a .pfx file.
 */
public static class GenerateCertificate
{
    /**
     * @brief Creates a self-signed certificate and saves it as a PKCS#12 (.pfx) file.
     */
    public static void Generate(string subjectName, string password, string outputPath)
    {
        // Generate random key pair
        var keyGen = new RsaKeyPairGenerator();
        keyGen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 2048));
        AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

        // Certificate attributes
        var certGen = new X509V3CertificateGenerator();
        var serialNumber = BigInteger.ProbablePrime(120, new Random());
        certGen.SetSerialNumber(serialNumber);
        certGen.SetIssuerDN(new X509Name("CN=" + subjectName));
        certGen.SetNotBefore(DateTime.UtcNow.Date);
        certGen.SetNotAfter(DateTime.UtcNow.Date.AddYears(5));
        certGen.SetSubjectDN(new X509Name("CN=" + subjectName));
        certGen.SetPublicKey(keyPair.Public);
        certGen.SetSignatureAlgorithm("SHA256WithRSA");

        // Extensions (optional but closer match to what Windows might generate)
        certGen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
        certGen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
        certGen.AddExtension(X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

        // Self-sign the certificate
        var cert = certGen.Generate(keyPair.Private);

        // Package into PKCS#12 (PFX)
        var store = new Pkcs12Store();
        string friendlyName = subjectName;
        var certEntry = new X509CertificateEntry(cert);
        store.SetCertificateEntry(friendlyName, certEntry);
        store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(keyPair.Private), new[] { certEntry });

        using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            store.Save(stream, password.ToCharArray(), new SecureRandom());
        }
    }
}
