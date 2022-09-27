namespace AssetStudio
{
    public class MT19937_64
    {
        private const ulong N = 312;
        private const ulong M = 156;
        private const ulong MATRIX_A = 0xB5026F5AA96619E9L;
        private const ulong UPPER_MASK = 0xFFFFFFFF80000000;
        private const ulong LOWER_MASK = 0X7FFFFFFFUL;
        private static ulong[] mt = new ulong[N + 1];
        private static ulong mti = N + 1;

        public MT19937_64(ulong seed)
        {
            this.Seed(seed);
        }

        public void Seed(ulong seed)
        {
            mt[0] = seed;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (6364136223846793005L * (mt[mti - 1] ^ (mt[mti - 1] >> 62)) + mti);
            }
        }

        public ulong Int63()
        {
            ulong x = 0;
            ulong[] mag01 = new ulong[2] { 0x0UL, MATRIX_A };

            if (mti >= N)
            {
                ulong kk;
                if (mti == N + 1)
                {
                    Seed(5489UL);
                }
                for (kk = 0; kk < (N - M); kk++)
                {
                    x = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (x >> 1) ^ mag01[x & 0x1UL];
                }
                for (; kk < N - 1; kk++)
                {
                    x = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk - M] ^ (x >> 1) ^ mag01[x & 0x1UL];
                }
                x = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (x >> 1) ^ mag01[x & 0x1UL];

                mti = 0;
            }

            x = mt[mti++];
            x ^= (x >> 29) & 0x5555555555555555L;
            x ^= (x << 17) & 0x71D67FFFEDA60000L;
            x ^= (x << 37) & 0xFFF7EEE000000000L;
            x ^= (x >> 43);
            return x;
        }

        public ulong IntN(ulong value)
        {
            return Int63() % value;
        }
    }
}