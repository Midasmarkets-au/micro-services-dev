using Bacera.Gateway.Vendor.PayPal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bacera.Gateway.Services.Common
{
    public static class UscScale
    {
        /// <summary>
        /// Amount * 10000
        /// </summary>
        public static int ToScaledFromCents(this int amount) => amount * 10000;
        /// <summary>
        /// Amount * 10000
        /// </summary>
        public static long ToScaledFromCents(this long amount) => amount * 10000;
        /// <summary>
        /// Amount * 10000
        /// </summary>
        public static ulong ToScaledFromCents(this ulong amount) => amount * 10000;
        /// <summary>
        /// Amount * 10000m
        /// </summary>
        public static decimal ToScaledFromCents(this decimal amount) => amount * 10000m;
        /// <summary>
        /// Amount * 10000d (with rounding to avoid floating-point precision errors)
        /// </summary>
        public static double ToScaledFromCents(this double amount) => Math.Round(amount * 10000d, 0, MidpointRounding.AwayFromZero);
        /// <summary>
        /// Amount / 10000
        /// </summary>
        public static int ToCentsFromScaled(this int amount) => amount / 10000;
        /// <summary>
        /// Amount / 10000
        /// </summary>
        public static long ToCentsFromScaled(this long amount) => amount / 10000;
        /// <summary>
        /// Amount / 10000
        /// </summary>
        public static ulong ToCentsFromScaled(this ulong amount) => amount / 10000;
        /// <summary>
        /// Amount / 10000m
        /// </summary>
        public static decimal ToCentsFromScaled(this decimal amount) => amount / 10000m;
        /// <summary>
        /// Amount / 10000d (with rounding to avoid floating-point precision errors)
        /// </summary>
        public static double ToCentsFromScaled(this double amount) => Math.Round(amount / 10000d, 2, MidpointRounding.AwayFromZero);
    }
}
