using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Utilities
{
    public static class CertificateHelper
    {
        /// <summary>
        /// Get SHA256 fingerprint from certificate file
        /// </summary>
        public static string GetSha256Fingerprint(string certificatePath)
        {
            if (!File.Exists(certificatePath))
                throw new FileNotFoundException($"Certificate file not found: {certificatePath}");

            using var cert = new X509Certificate2(certificatePath);
            return GetSha256Fingerprint(cert);
        }

        /// <summary>
        /// Get SHA256 fingerprint from X509Certificate2
        /// </summary>
        public static string GetSha256Fingerprint(X509Certificate2 certificate)
        {
            var hash = SHA256.HashData(certificate.RawData);
            return BitConverter.ToString(hash).Replace("-", ":").ToUpperInvariant();
        }

        /// <summary>
        /// Get SHA1 fingerprint (legacy, less secure)
        /// </summary>
        public static string GetSha1Fingerprint(string certificatePath)
        {
            using var cert = new X509Certificate2(certificatePath);
            return cert.Thumbprint;
        }

        /// <summary>
        /// Get fingerprint from PEM file
        /// </summary>
        public static string GetFingerprintFromPem(string pemFilePath)
        {
            var pemContent = File.ReadAllText(pemFilePath);
            return GetFingerprintFromPemContent(pemContent);
        }

        /// <summary>
        /// Get fingerprint from PEM content string
        /// </summary>
        public static string GetFingerprintFromPemContent(string pemContent)
        {
            // Remove PEM headers and whitespace
            var base64Content = pemContent
                .Replace("-----BEGIN CERTIFICATE-----", "")
                .Replace("-----END CERTIFICATE-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace(" ", "");

            var certBytes = Convert.FromBase64String(base64Content);
            using var cert = new X509Certificate2(certBytes);
            return GetSha256Fingerprint(cert);
        }

        /// <summary>
        /// Download certificate from server and get fingerprint
        /// </summary>
        public static async Task<string> GetRemoteCertificateFingerprint(string hostname, int port = 443)
        {
            using var client = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    // We want to get the cert even if it has errors
                    return true;
                }
            });

            try
            {
                var response = await client.GetAsync($"https://{hostname}:{port}");
                // The certificate is captured in the validation callback above
            }
            catch
            {
                // We might get connection errors, but that's ok if we got the cert
            }

            // Alternative approach using TcpClient
            using var tcpClient = new System.Net.Sockets.TcpClient();
            await tcpClient.ConnectAsync(hostname, port);

            using var sslStream = new System.Net.Security.SslStream(tcpClient.GetStream(), false,
                (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (certificate != null)
                    {
                        var cert2 = new X509Certificate2(certificate);
                        var fingerprint = GetSha256Fingerprint(cert2);
                        Console.WriteLine($"Remote certificate fingerprint: {fingerprint}");
                    }
                    return true;
                });

            await sslStream.AuthenticateAsClientAsync(hostname);

            var remoteCert = sslStream.RemoteCertificate;
            if (remoteCert != null)
            {
                using var cert = new X509Certificate2(remoteCert);
                return GetSha256Fingerprint(cert);
            }

            throw new InvalidOperationException($"Could not retrieve certificate from {hostname}:{port}");
        }

        /// <summary>
        /// Validate certificate fingerprint format
        /// </summary>
        public static bool IsValidFingerprint(string fingerprint)
        {
            if (string.IsNullOrEmpty(fingerprint))
                return false;

            // SHA256 format: AA:BB:CC:DD... (64 chars + 31 colons = 95 total)
            // SHA1 format: AA:BB:CC:DD... (40 chars + 19 colons = 59 total)
            return fingerprint.Length == 95 || fingerprint.Length == 59;
        }
    }
}
