﻿// © 2011 Jon Hanna.
// This source code is licensed under the EUPL, Version 1.1 only (the “Licence”).
// You may not use, modify or distribute this work except in compliance with the Licence.
// You may obtain a copy of the Licence at:
// <http://joinup.ec.europa.eu/software/page/eupl/licence-eupl>
// A copy is also distributed with this source code.
// Unless required by applicable law or agreed to in writing, software distributed under the
// Licence is distributed on an “AS IS” basis, without warranties or conditions of any kind.

// A compiled binary is available from <http://hackcraft.github.com/Ariadne/> which if
// unmodified, may be used without restriction. (This dual-licensing is to provide a clear
// answer to the question of whether using the library in an application counts as creating
// a derivative work).

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ariadne
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct OffsetInt
    {
        //We want the offset to be Cache-line-size - sizeof(int).
        //On most chips that .NET or Mono run on at the moment, this would be 64 - 4 = 60.
        //On Itanium, it would be 128 - 4 = 124. Other sizes are both possible and quite likely
        //to appear in the future. We could P-Invoke and find the precise size to use (and use
        //an array of ints with gaps decided on that basis), but that wouldn’t be portable, so we
        //just assume that 64 is the cache-line size and while we don’t improve as much as we
        //can on Itanium, we’ll at least improve things.
        [FieldOffset(60)]
        public int Num;
    }
    /// <summary>A counter designed for highly concurrent use.</summary>
    /// <remarks>This counter tends to be appreciably slower than using <see cref="Interlocked.Increment(ref int)"/>
    /// when there is little contention. However, it is much faster in the face of contending threads, with the
    /// comparable cost of <see cref="Interlocked.Increment(ref int)"/> increasing nearly expotentially to
    /// the number of contending threads.</remarks>
    public sealed class Counter
    {
        private const int ArraySize = 32;
        private const int Mask = ArraySize - 1;
        private readonly OffsetInt[] _counters;
        /// <summary>Creates a new <see cref="Counter"/> with an initial value of zero.</summary>
        public Counter()
        {
            _counters = new OffsetInt[ArraySize];
        }
        /// <summary>Creates a new <see cref="Counter"/> with an initial value of <paramref name="startingValue"/>.</summary>
        /// <param name="startingValue">The initial value for the <see cref="Counter"/>.</param>
        public Counter(int startingValue)
            :this()
        {
            _counters[0].Num = startingValue;
        }
        /// <summary>The current value of the counter.</summary>
        public int Value
        {
            get
            {
                int sum = 0;
                for(int i = 0; i != ArraySize; ++i)
                    sum += _counters[i].Num;
                return sum;
            }
        }
        /// <summary>Returns the value of the <see cref="Counter"/>.</summary>
        /// <param name="counter">The <see cref="Counter"/> to cast.</param>
        /// <returns>An integer of the same value as the <see cref="Counter"/>.</returns>
	    public static implicit operator int(Counter counter)
	    {
	        return counter.Value;
	    }
	    private int GetIndex()
	    {
	        return Thread.CurrentThread.ManagedThreadId & Mask;
	    }
	    /// <summary>Atomically increments the <see cref="Counter"/> by one.</summary>
        public void Increment()
        {
            //We avoid different cores hitting the same counter, but don’t completely prohibit it, so we
            //still need Interlocked.
            Interlocked.Increment(ref _counters[GetIndex()].Num);
        }
        /// <summary>Atomically decrements the <see cref="Counter"/> by one.</summary>
        public void Decrement()
        {
            Interlocked.Decrement(ref _counters[GetIndex()].Num);
        }
        /// <summary>Atomically increments <paramref name="counter"/> by one.</summary>
        /// <param name="counter">The <see cref="Counter"/> to increment.</param>
        /// <returns>The <see cref="Counter"/> that was operated on.</returns>
        public static Counter operator ++(Counter counter)
        {
            counter.Increment();
            return counter;
        }
        /// <summary>Atomically decrements <paramref name="counter"/> by one.</summary>
        /// <param name="counter">The <see cref="Counter"/> to decrement.</param>
        /// <returns>The <see cref="Counter"/> that was operated on.</returns>
        public static Counter operator --(Counter counter)
        {
            counter.Decrement();
            return counter;
        }
    }
}