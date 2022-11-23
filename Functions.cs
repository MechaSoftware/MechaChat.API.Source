using System;
using System.Web.Http;
using System.Linq;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Owin;
using MechaChat.API;
using MechaChat.API.ErrorHandling;
using MechaChat.API.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MultipartDataMediaFormatter;
using MultipartDataMediaFormatter.Infrastructure;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using CryptSharp;
using Multiformats.Base;

namespace MechaChat.API
{
    public class Functions
    {
        private static Random random = new Random();

        public string GenerateJSONWebToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("auo6S90Ubdoaaloom0nKlkw3ega0kWpJv8qiXE56KgsgxtMSAd"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken("mecha.chat", "mecha.chat", null, expires: DateTime.Now.AddDays(5), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string IdentifierGen()
        {
            var currentTS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var currentTSb = Convert.ToString(currentTS, 2);
            var currentTSd = Convert.ToInt64(currentTSb, 2);
            var currentEpochTS = currentTSd - 1640952000000;
            var quickCalc = currentEpochTS << 22;
            var randomDigits = new Random().Next(1, 100);

            return JoinNumber(quickCalc, randomDigits);
        }

        public static string RandomStringGen()
        {
            const string chars = "abcdefghijklmnopqrstuvqxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string JoinNumber(long x, int y)
        {
            return Convert.ToString(x) + Convert.ToString(y);
        }

        public static string EncodeStringBase58(string stringDecoded)
        {
            var stringBytes = Encoding.ASCII.GetBytes(stringDecoded);
            var base58Encoded = Multibase.Base58.Encode(stringBytes);

            return base58Encoded;
        }

        public static string DecodeStringBase58(string stringEncoded)
        {
            var base58Bytes = Multibase.Base58.Decode(stringEncoded);
            var base58Decoded = Encoding.ASCII.GetString(base58Bytes);

            return base58Decoded;
        }

        public static string Stringify(object data)
        {
            bool flag = data == null;
            string result;

            if (flag)
            {
                result = null;
            }
            else
            {
                string text = null;

                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    text = JsonConvert.SerializeObject(data, settings);
                }
                catch (Exception ex)
                {
                    text = null;
                    Console.WriteLine($"[Error] {ex}");
                }

                result = text;
            }

            return result;
        }
    }
}
