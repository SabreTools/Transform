namespace Transform
{
    /// <summary>
    /// Determines the header skip operation
    /// </summary>
    internal enum TransformOperation
    {
        // Default
        None = 0,

        // Swaping operations
        Bitswap,
        Byteswap,
        Wordswap,
        WordByteswap,

        // Interleaving operations
        InterleaveByte,
        InterleaveWord,

        // Splitting operations
        SplitOneByte,
        SplitTwoBytes,
        SplitFourBytes,
        SplitEightBytes,
    }
}
