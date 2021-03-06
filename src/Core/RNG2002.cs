﻿/* C# Port of MT19937: Integer version (2002/1/26) algorithm used in the PS4?    */
/* More information, including original source can be found at the following     */
/* Link: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/MT2002/emt19937ar.html  */

/* 
   A C-program for MT19937, with initialization improved 2002/1/26.
   Coded by Takuji Nishimura and Makoto Matsumoto.

   Before using, initialize the state by using init_genrand(seed)  
   or init_by_array(init_key, key_length).

   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/


using System;

namespace FF12RNGHelper.Core
{
    public class RNG2002 : IRNG
    {
        /// <summary>
        /// This is the seed the PS4/FF12:ZA uses
        /// </summary>
        private const UInt32 DEFAULT_SEED = 4537U; // 5489U is default seed. PS2 and PS4/FF12:ZA uses 4537.


        /* Period parameters */
        private const Int32 N = 624;
        private const Int32 M = 397;
        private const UInt32 MATRIX_A = 0x9908b0dfU;   /* constant vector a */
        private const UInt32 UPPER_MASK = 0x80000000U; /* most significant w-r bits */
        private const UInt32 LOWER_MASK = 0x7fffffffU; /* least significant r bits */

        private UInt32[] mt = new UInt32[N]; /* the array for the state vector  */
        private Int32 mti = N + 1; /* mti==N+1 means mt[N] is not initialized */
        private int position = 0; // position in the RNG. For debuggin purposes

        public RNG2002(UInt32 seed = DEFAULT_SEED)
        {
            sgenrand(seed);
        }

        public void sgenrand()
        {
            sgenrand(DEFAULT_SEED);
        }

        /* initializes mt[N] with a seed */
        public void sgenrand(UInt32 s) //init_genrand
        {
            mt[0] = s & 0xffffffff;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] =
                (1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (UInt32)mti);
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                mt[mti] &= 0xffffffff;
                /* for >32 bit machines */
            }
            position = 0;
        }

        private UInt32[] mag01 = { 0x0U, MATRIX_A }; //Moved out of below method.
                                                     /* mag01[x] = x * MATRIX_A  for x=0,1 */

        /// <summary>
        /// Generates the next random number in the sequence
        /// on [0,0xffffffff]-interval.
        /// </summary>
        /// <returns>The next random number in the sequence.</returns>
        public UInt32 genrand() //genrand_int32
        {
            UInt32 y;
            //See above for what was moved out from here

            if (mti >= N)
            { /* generate N words at one time */
                int kk;

                if (mti == N + 1)   /* if init_genrand() has not been called, */
                    sgenrand(DEFAULT_SEED); /* a default initial seed is used */

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];

                mti = 0;
            }

            y = mt[mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);

            position++;
            return y;
        }


        /// <summary>
        /// Saves the state of the RNG
        /// </summary>
        /// <param name="rng"></param>
        /// <returns>RNGState structure</returns>
        public RNGState saveState()
        {
            return new RNGState
            {
                mti = this.mti,
                mt = this.mt.Clone() as uint[],
                position = this.position
            };
        }

        /// <summary>
        /// Loads the state of the RNG
        /// </summary>
        /// <param name="inmti">Input mti</param>
        /// <param name="inmt">Input mt</param>
        public void loadState(int mti, UInt32[] mt, int position)
        {
            this.mti = mti;
            mt.CopyTo(this.mt, 0);
            this.position = position;
        }

        /// <summary>
        /// Loads the state of the RNG
        /// </summary>
        /// <param name="inputState">Tuple<Input mti, Input mt, Input mag01></param>
        public void loadState(RNGState inputState)
        {
            mti = inputState.mti;
            inputState.mt.CopyTo(mt, 0);
            position = inputState.position;
        }

        /// <summary>
        /// Return a deep copy
        /// </summary>
        public IRNG DeepClone()
        {
            RNG2002 newRNG = new RNG2002();
            newRNG.loadState(saveState());

            return newRNG;
        }

        /// <summary>
        /// Return a deep copy
        /// </summary>
        object IDeepCloneable.DeepClone()
        {
            return DeepClone();
        }

        public int getPosition()
        {
            return position;
        }
    }
}
