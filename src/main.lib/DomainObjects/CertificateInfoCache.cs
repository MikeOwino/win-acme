﻿using Org.BouncyCastle.Crypto;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PKISharp.WACS.DomainObjects
{
    /// <summary>
    /// Special implementation of CertificateInfo which contains reference
    /// to a file in the cache
    /// </summary>
    public class CertificateInfoCache : ICertificateInfo
    {
        private readonly CertificateInfo _inner;

        /// <summary>
        /// Contruction with two attempts: as EphemeralKeySet first 
        /// and fallback to MachineKeySet second for missing/corrupt
        /// profiles
        /// </summary>
        /// <param name="file"></param>
        /// <param name="password"></param>
        public CertificateInfoCache(FileInfo file, string? password)  
        {
            CacheFile = file;
            CacheFilePassword = password;
            try
            {
                _inner = GenerateInner(X509KeyStorageFlags.EphemeralKeySet);
            } 
            catch (CryptographicException)
            {
                _inner = GenerateInner(X509KeyStorageFlags.MachineKeySet);
            }
        }  

        /// <summary>
        /// Load inner file from disk
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private CertificateInfo GenerateInner(X509KeyStorageFlags flags)
        {
            var ret = new X509Certificate2Collection();
            var finalFlags = X509KeyStorageFlags.Exportable | flags;
            ret.Import(CacheFile.FullName, CacheFilePassword, finalFlags);
            return new CertificateInfo(ret);
        }

        /// <summary>
        /// Location on disk
        /// </summary>
        public FileInfo CacheFile { get; private set; }

        /// <summary>
        /// Password used to protect the file on disk
        /// </summary>
        public string? CacheFilePassword { get; private set; }

        public X509Certificate2 Certificate => _inner.Certificate;
        public IEnumerable<X509Certificate2> Chain => _inner.Chain;
        public X509Certificate2Collection Collection => _inner.Collection;
        public Identifier? CommonName => _inner.CommonName;
        public AsymmetricKeyParameter? PrivateKey => _inner.PrivateKey;
        public IEnumerable<Identifier> SanNames => _inner.SanNames;
    }
}
