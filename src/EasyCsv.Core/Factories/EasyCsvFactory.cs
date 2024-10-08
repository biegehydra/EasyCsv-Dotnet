﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using EasyCsv.Core.Configuration;

[assembly: InternalsVisibleTo("EasyCsv.Tests.Core")]
namespace EasyCsv.Core
{
    public static class EasyCsvFactory
    {
        private static IEasyCsv? NullOrEasyCsv(IEasyCsv? easyCsv) => easyCsv?.CsvContent is not {Count: > 0} ? null : easyCsv;
        private static EasyCsvConfiguration GlobalConfig => EasyCsvConfiguration.Instance;
        private static EasyCsvConfiguration UserConfigOrGlobalConfig(EasyCsvConfiguration? userConfig) => userConfig ?? GlobalConfig;

        /// <summary>
        /// Creates IEasyCsv from byte[] synchronously
        /// </summary>
        /// <param name="fileContentBytes">CsvContent of the CSV file</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        public static IEasyCsv? FromBytes(byte[] fileContentBytes, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new EasyCsvInternal(fileContentBytes, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from byte[] as a background task
        /// </summary>
        /// <param name="fileContentBytes">CsvContent of CSV</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        public static async Task<IEasyCsv?> FromBytesAsync(byte[] fileContentBytes, EasyCsvConfiguration? config = null)
        {
            return await CreateCsvServiceInBackground(fileContentBytes, Encoding.UTF8.GetString(fileContentBytes), UserConfigOrGlobalConfig(config));
        }


        /// <summary>
        /// Creates IEasyCsv from string synchronously
        /// </summary>
        /// <param name="fileContentStr">CsvContent of CSV</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        public static IEasyCsv? FromString(string fileContentStr, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new EasyCsvInternal(fileContentStr, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from string as a background task
        /// </summary>
        /// <param name="fileContentStr">CsvContent of CSV</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        public static async Task<IEasyCsv?> FromStringAsync(string fileContentStr, EasyCsvConfiguration? config = null)
        {
            return await CreateCsvServiceInBackground(Encoding.UTF8.GetBytes(fileContentStr), fileContentStr, config);
        }

        private static async Task<IEasyCsv?> CreateCsvServiceInBackground(byte[] fileContentByte, string fileContentStr, EasyCsvConfiguration? config)
        {
            var easyCsv = new EasyCsvInternal(UserConfigOrGlobalConfig(config))
            {
                ContentBytes = fileContentByte,
                ContentStr = fileContentStr,
            };
            await easyCsv.CreateCsvContentInBackGround();
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from Stream synchronously
        /// </summary>
        /// <param name="fileStream">Stream of csv file</param>
        /// <param name="maxFileSize">Will throw exception if file is larger than file size</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        public static IEasyCsv? FromStream(Stream fileStream, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new EasyCsvInternal(fileStream, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from Stream asynchronously
        /// </summary>
        /// <param name="fileStream">Stream of csv file</param>
        /// <param name="maxFileSize">Will throw exception if file is larger than file size</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <returns>IEasyCsv</returns>
        private static async Task<IEasyCsv?> FromStreamAsync(Stream fileStream, int maxFileSize, EasyCsvConfiguration? config = null)
        {
            var fileContent = await ReadStreamToEndAsync(fileStream, maxFileSize);
            var easyCsv = new EasyCsvInternal(fileContent, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }

        private static async Task<byte[]> ReadStreamToEndAsync(Stream stream, int maxFileSize)
        {
            using var memoryStream = new MemoryStream(maxFileSize);
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Creates IEasyCsv from TextReader synchronously
        /// </summary>
        /// <param name="textReader">Stream of csv file</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        public static IEasyCsv? FromTextReader(TextReader textReader, EasyCsvConfiguration? config = null)
        {
            var easyCsv = new EasyCsvInternal(textReader, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from TextReader as a background task
        /// </summary>
        /// <param name="textReader">Stream of csv file</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        /// <remarks>
        /// Background tasks are not the same as asynchronous calls, though similar. Understand the differences when using this function
        /// </remarks>
        public static async Task<IEasyCsv?> FromTextReaderAsync(TextReader textReader, EasyCsvConfiguration? config = null)
        {
            var fileContentStr = await textReader.ReadToEndAsync();
            var easyCsv = await CreateCsvServiceInBackground(Encoding.UTF8.GetBytes(fileContentStr), fileContentStr, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from File synchronously
        /// </summary>
        /// <param name="filePath">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static IEasyCsv? FromFile(string filePath, int maxFileSize = int.MaxValue, EasyCsvConfiguration? config = null)
        {
            var bytes = ReadFromFile(filePath, maxFileSize);
            var easyCsv = new EasyCsvInternal(bytes, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }

        private static byte[] ReadFromFile(string filePath, int maxFileSize)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo == null || fileInfo.Length > maxFileSize)
            {
                throw new InvalidOperationException("File size too large");
            }
            return File.ReadAllBytes(filePath);
        }



        /// <summary>
        /// Creates IEasyCsv from File asynchronously
        /// </summary>
        /// <param name="filePath">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static async Task<IEasyCsv?> FromFileAsync(string filePath, int maxFileSize = int.MaxValue, EasyCsvConfiguration? config = null)
        {
            var bytes = await ReadFromFileAsync(filePath, maxFileSize);
            var easyCsv = await FromBytesAsync(bytes, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }

        private static async Task<byte[]> ReadFromFileAsync(string filePath, int maxFileSize)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo == null || fileInfo.Length > maxFileSize)
            {
                throw new InvalidOperationException("File size too large");
            }
#if NETSTANDARD2_0
            return await ReadAllBytesAsync(filePath);
            async Task<byte[]> ReadAllBytesAsync(string filePath)
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead, totalBytesRead = 0;

                while ((bytesRead = await fileStream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                }

                return buffer;
            }
#else
            return await File.ReadAllBytesAsync(filePath);
#endif
        }



        /// <summary>
        /// Creates IEasyCsv from Url synchronously
        /// </summary>
        /// <param name="url">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static IEasyCsv? FromUrl(string url, int maxFileSize, EasyCsvConfiguration? config = null)
        {
            using var httpClient = new HttpClient();
            using var responseStream = httpClient.GetStreamAsync(url).GetAwaiter().GetResult();
            var easyCsv = new EasyCsvInternal(responseStream, UserConfigOrGlobalConfig(config));
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from >Url asynchronously
        /// </summary>
        /// <param name="url">Path to csv file</param>
        /// <param name="maxFileSize">If file is larger an exception will be thrown</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static async Task<IEasyCsv?> FromUrlAsync(string url, int maxFileSize, EasyCsvConfiguration? config = null)
        {
            using var httpClient = new HttpClient();
            using var responseStream = await httpClient.GetStreamAsync(url);
            return await FromStreamAsync(responseStream, maxFileSize, UserConfigOrGlobalConfig(config));
        }


        /// <summary>
        /// Creates IEasyCsv from List asynchronously. Public instance variables become header names and values make up the rows
        /// </summary>
        /// <param name="objects">Objects to create the csv from</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static async Task<IEasyCsv?> FromObjectsAsync<T>(List<T> objects, EasyCsvConfiguration? config = null, CsvContextProfile? csvContextProfile = null)
        {
            var easyCsv = await EasyCsvInternal.FromObjectsAsync(objects, UserConfigOrGlobalConfig(config), csvContextProfile);
            return NullOrEasyCsv(easyCsv);
        }


        /// <summary>
        /// Creates IEasyCsv from List synchronously. Public instance variables become header names and values make up the rows
        /// </summary>
        /// <param name="objects">Objects to create the csv from</param>
        /// <param name="config">Determines whether to normalize fields. Default normalization makes them all lower, you can also define custom normalization methods</param>
        public static IEasyCsv? FromObjects<T>(List<T> objects, EasyCsvConfiguration? config = null, CsvContextProfile? csvContextProfile = null)
        {
            var easyCsv = EasyCsvInternal.FromObjects(objects, UserConfigOrGlobalConfig(config), csvContextProfile);
            return NullOrEasyCsv(easyCsv);
        }
    }
}
